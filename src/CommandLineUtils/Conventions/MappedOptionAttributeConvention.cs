// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using McMaster.Extensions.CommandLineUtils.Abstractions;
using McMaster.Extensions.CommandLineUtils.Attributes;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    /// <summary>
    /// Adds an <see cref="CommandOption"/> to match each usage of <see cref="MappedOptionAttribute"/>
    /// on the model type of <see cref="CommandLineApplication{TModel}"/>.
    /// </summary>
    public class MappedOptionAttributeConvention : OptionAttributeConventionBase<MappedOptionAttribute>, IConvention
    {
        /// <inheritdoc />
        public virtual void Apply(ConventionContext context)
        {
            if (context.ModelType == null)
            {
                return;
            }

            var props = ReflectionHelper.GetProperties(context.ModelType);
            foreach (var prop in props)
            {
                using var enumerator = prop.GetCustomAttributes<MappedOptionAttribute>().GetEnumerator();
                if (!enumerator.MoveNext())
                {
                    continue;
                }

                EnsureDoesNotHaveHelpOptionAttribute(prop);
                EnsureDoesNotHaveVersionOptionAttribute(prop);
                EnsureDoesNotHaveArgumentAttribute(prop);

                var modelAccessor = context.ModelAccessor;
                if (modelAccessor == null)
                {
                    throw new InvalidOperationException(Strings.ConventionRequiresModel);
                }

                var method = s_applyGeneric.MakeGenericMethod(context.ModelType, prop.PropertyType);
                try
                {
                    method.Invoke(this, new object[] { context, prop, enumerator });
                }
                catch (TargetInvocationException e)
                {
                    var innerException = e.InnerException;
                    if (innerException != null)
                    {
                        ExceptionDispatchInfo.Capture(innerException).Throw();
                    }
                    throw;
                }
            }
        }

        private static readonly MethodInfo s_applyGeneric
            = typeof(MappedOptionAttributeConvention)
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Single(m => m.Name == nameof(Apply) && m.IsGenericMethod);

        private void Apply<TModel, TValue>(ConventionContext context, PropertyInfo prop,
            IEnumerator<MappedOptionAttribute> enumerator)
            where TModel : class
        {
            var setterDelegate = ReflectionHelper.GetPropertySetter(prop);

            var option = new SingularPropertyMappedOption<TModel, TValue>(context.Application, setterDelegate)
            {
                Description = prop.Name
            };
            context.Application.AddOption(option);
            do
            {
                enumerator.Current.Configure(option, prop);
            } while (enumerator.MoveNext());
        }

        private static void EnsureDoesNotHaveVersionOptionAttribute(PropertyInfo prop)
        {
            var versionOptionAttr = prop.GetCustomAttribute<VersionOptionAttribute>();
            if (versionOptionAttr != null)
            {
                throw new InvalidOperationException(
                    Strings.BothHelpOptionAndVersionOptionAttributesCannotBeSpecified(prop));
            }
        }

        private static void EnsureDoesNotHaveHelpOptionAttribute(PropertyInfo prop)
        {
            var versionOptionAttr = prop.GetCustomAttribute<VersionOptionAttribute>();
            if (versionOptionAttr != null)
            {
                throw new InvalidOperationException(
                    Strings.BothHelpOptionAndVersionOptionAttributesCannotBeSpecified(prop));
            }
        }

        class SingularPropertyMappedOption<TModel, TValue>
            : MappedOption<TValue>, IInternalCommandParamOfT
            where TModel : class
        {
            private readonly SetPropertyDelegate _propertySetter;

            public SingularPropertyMappedOption(CommandLineApplication commandLineApplication,
                SetPropertyDelegate propertySetter)
                : base(commandLineApplication, CommandOptionType.SingleValue)
            {
                _propertySetter = propertySetter;
            }

            public void Parse(CultureInfo culture)
            {
                if (!HasValue())
                {
                    return;
                }

                switch (_commandLineApplication)
                {
                    case CommandLineApplication<TModel> appT:
                        {
                            _propertySetter(appT.Model, ParsedValue);
                            break;
                        }
                    case IModelAccessor modelAccessor:
                        {
                            _propertySetter((TModel)modelAccessor.GetModel(), ParsedValue);
                            break;
                        }
                    default:
                        {
                            throw new InvalidOperationException("Can not get the model of the current command");
                        }
                }
            }
        }
    }
}
