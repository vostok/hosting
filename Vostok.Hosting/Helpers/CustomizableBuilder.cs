using System;
using Vostok.Commons.Helpers;
using Vostok.Hosting.Components;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.Helpers
{
    internal class CustomizableBuilder<TBuilder, TResult>
        where TBuilder : IBuilder<TResult>
    {
        private readonly TBuilder builder;
        private readonly Customization<TBuilder> builderCustomization;

        private volatile IVostokHostingEnvironmentSetupContext environmentSetupContext;
        private volatile IVostokConfigurationSetupContext configurationSetupContext;

        public CustomizableBuilder(TBuilder builder)
        {
            this.builder = builder;
            builderCustomization = new Customization<TBuilder>();
        }

        public void AddCustomization(Action<TBuilder> setup)
            => builderCustomization.AddCustomization(setup);

        public void AddCustomization(Action<TBuilder, IVostokHostingEnvironmentSetupContext> setup)
            => builderCustomization.AddCustomization(b => setup(b, environmentSetupContext));

        public void AddCustomization(Action<TBuilder, IVostokConfigurationSetupContext> setup)
            => builderCustomization.AddCustomization(b => setup(b, configurationSetupContext));

        public TResult Build(BuildContext context)
        {
            environmentSetupContext = context.EnvironmentSetupContext;
            configurationSetupContext = context.ConfigurationSetupContext;

            builderCustomization.Customize(builder);

            return builder.Build(context);
        }

        public TBuilder GetCustomizedBuilderIntermediate(BuildContext context)
        {
            try
            {
                environmentSetupContext = context.EnvironmentSetupContext;
                configurationSetupContext = context.ConfigurationSetupContext;

                return builderCustomization.Customize(builder);
            }
            finally
            {
                environmentSetupContext = null;
                configurationSetupContext = null;
            }
        }
    }
}