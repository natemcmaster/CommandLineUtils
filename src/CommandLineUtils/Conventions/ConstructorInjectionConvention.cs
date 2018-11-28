// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    /// <summary>
    /// Uses an instance of <see cref="IServiceProvider" /> to call constructors
    /// when creating models.
    /// </summary>
    public class ConstructorInjectionConvention : IConvention
    {
        private readonly IServiceProvider _additionalServices;

        /// <summary>
        /// Initializes an instance of <see cref="ConstructorInjectionConvention" />.
        /// </summary>
        public ConstructorInjectionConvention()
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="ConstructorInjectionConvention" />.
        /// </summary>
        /// <param name="additionalServices">Additional services use to inject the constructor of the model</param>
        public ConstructorInjectionConvention(IServiceProvider additionalServices)
        {
            _additionalServices = additionalServices;
        }

        /// <inheritdoc />
        public virtual void Apply(ConventionContext context)
        {
            if (_additionalServices != null)
            {
                context.Application.AdditionalServices = _additionalServices;
            }

            if (context.ModelType == null)
            {
                return;
            }

            s_applyMethod.MakeGenericMethod(context.ModelType).Invoke(this, new object[] { context });
        }

        private static readonly MethodInfo s_applyMethod
         = typeof(ConstructorInjectionConvention).GetRuntimeMethods().Single(m => m.Name == nameof(ApplyImpl));

        private void ApplyImpl<TModel>(ConventionContext context)
            where TModel : class
        {
            var constructors = typeof(TModel)
                .GetTypeInfo()
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            var factory = FindMatchedConstructor<TModel>(constructors, context.Application,
                constructors.Length == 1);

            if (factory != null)
            {
                ((CommandLineApplication<TModel>) context.Application).ModelFactory = factory;
            }
        }

        private static Func<TModel> FindMatchedConstructor<TModel>(
            ConstructorInfo[] constructors,
            IServiceProvider services,
            bool throwIfNoParameterTypeRegistered = false)
        {
            if (constructors.Length == 0)
            {
                return () => throw new InvalidOperationException(Strings.NoAnyPublicConstuctorFound(typeof(TModel)));
            }

            foreach (var ctorCandidate in constructors.OrderByDescending(c => c.GetParameters().Length))
            {
                var parameters = ctorCandidate.GetParameters().ToArray();
                if (parameters.Length == 0)
                {
                    return null;
                }

                var args = new object[parameters.Length];
                for (var i = 0; i < parameters.Length; i++)
                {
                    var paramType = parameters[i].ParameterType;
                    var service = services.GetService(paramType);
                    if (service == null)
                    {
                        if (!throwIfNoParameterTypeRegistered)
                        {
                            goto nextCtor;
                        }

                        return () =>
                            throw new InvalidOperationException(
                                Strings.NoParameterTypeRegistered(ctorCandidate.DeclaringType, paramType));
                    }

                    args[i] = service;
                    if (i == parameters.Length - 1)
                    {
                        return () => (TModel) ctorCandidate.Invoke(args);
                    }
                }

                nextCtor: ;
            }

            return () => throw new InvalidOperationException(Strings.NoMatchedConstructorFound(typeof(TModel)));
        }
    }
}
