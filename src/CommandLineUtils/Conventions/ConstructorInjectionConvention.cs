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

            if (constructors.Length == 0)
            {
                throw new InvalidOperationException("Could not find any public constructors on " + typeof(TModel).FullName);
            }

            // find the constructor with the most parameters first
            foreach (var ctorCandidate in constructors.OrderByDescending(c => c.GetParameters().Count()))
            {
                var parameters = ctorCandidate.GetParameters().ToArray();
                var args = new object[parameters.Length];
                var matched = false;
                for (var i = 0; i < parameters.Length; i++)
                {
                    var paramType = parameters[i].ParameterType;
                    var service = ((IServiceProvider)context.Application).GetService(paramType);
                    if (service == null)
                    {
                        break;
                    }
                    args[i] = service;
                    matched = i == parameters.Length - 1;
                }

                if (matched)
                {
                    (context.Application as CommandLineApplication<TModel>).ModelFactory =
                        () => (TModel)ctorCandidate.Invoke(args);
                    return;
                }
            }
        }
    }
}
