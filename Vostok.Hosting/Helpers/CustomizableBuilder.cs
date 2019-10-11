﻿using System;
using System.Collections.Generic;
using Vostok.Hosting.Components;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.Helpers
{
    internal class CustomizableBuilder<TBuilder, TResult>
        where TBuilder : IBuilder<TResult>
    {
        private readonly TBuilder builder;
        private readonly Customization<TBuilder> builderCustomization;
        private IVostokConfigurationContext configurationContext;

        public CustomizableBuilder(TBuilder builder)
        {
            this.builder = builder;
            builderCustomization = new Customization<TBuilder>();
        }

        public void AddCustomization(Action<TBuilder> setup)
        {
            builderCustomization.AddCustomization(setup);
        }

        public void AddCustomization(Action<TBuilder, IVostokConfigurationContext> setup)
        {
            builderCustomization.AddCustomization(b => setup(b, configurationContext));
        }

        public TResult Build(BuildContext context)
        {
            configurationContext = context.ConfigurationContext;

            builderCustomization.Customize(builder);

            return builder.Build(context);
        }
    }
}