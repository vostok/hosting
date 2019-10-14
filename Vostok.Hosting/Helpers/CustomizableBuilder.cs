using System;
using Vostok.Hosting.Components;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.Helpers
{
    internal class CustomizableBuilder<TBuilder, TResult>
        where TBuilder : IBuilder<TResult>
    {
        private readonly TBuilder builder;
        private readonly Customization<TBuilder> builderCustomization;
        private IVostokHostingEnvironmentSetupContext configurationContext;

        public CustomizableBuilder(TBuilder builder)
        {
            this.builder = builder;
            builderCustomization = new Customization<TBuilder>();
        }

        public void AddCustomization(Action<TBuilder> setup)
        {
            builderCustomization.AddCustomization(setup);
        }

        public void AddCustomization(Action<TBuilder, IVostokHostingEnvironmentSetupContext> setup)
        {
            builderCustomization.AddCustomization(b => setup(b, configurationContext));
        }

        public TResult Build(BuildContext context)
        {
            configurationContext = context.SetupContext;

            builderCustomization.Customize(builder);

            return builder.Build(context);
        }
    }
}