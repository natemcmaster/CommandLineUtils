using System;
using System.Linq;
using System.Reflection;

namespace McMaster.Extensions.CommandLineUtils.Conventions
{
    /// <summary>
    /// Uses an instance of <see cref="IServiceProvider" /> to provide externally initialized model
    /// </summary>
    public class InjectedModelConvention : IConvention
    {
        private readonly IServiceProvider _additionalServices;

        /// <summary>
        /// Initializes an instance of <see cref="InjectedModelConvention" />.
        /// </summary>
        public InjectedModelConvention()
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="InjectedModelConvention" />.
        /// </summary>
        /// <param name="additionalServices">Additional services use to inject the constructor of the model</param>
        public InjectedModelConvention(IServiceProvider additionalServices)
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

            s_applyMethod.MakeGenericMethod(context.ModelType).Invoke(this, new object[] {context});
        }

        private static readonly MethodInfo s_applyMethod
            = typeof(InjectedModelConvention).GetRuntimeMethods().Single(m => m.Name == nameof(ApplyImpl));

        private void ApplyImpl<TModel>(ConventionContext context)
            where TModel : class
        {
            var service = ((IServiceProvider) context.Application).GetService(typeof(TModel));

            if (service != null)
            {
                ((CommandLineApplication<TModel>) context.Application).ModelFactory = () => (TModel) service;
            }
        }
    }
}
