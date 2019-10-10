using System;
using System.Collections.Generic;
using Vostok.Hosting.Components;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.Helpers
{
    internal class BuilderWithSetup<TBuilder, TResult>
        where TBuilder : IBuilder<TResult>
    {
        private readonly TBuilder builder;
        private readonly List<Action<TBuilder, IVostokConfigurationContext>> setups;

        public BuilderWithSetup(TBuilder builder)
        {
            this.builder = builder;
            setups = new List<Action<TBuilder, IVostokConfigurationContext>>();
        }

        public void Setup(Action<TBuilder> setup)
        {
            setups.Add((t, _) => setup(t));
        }

        public void Setup(Action<TBuilder, IVostokConfigurationContext> setup)
        {
            setups.Add(setup);
        }

        public TResult Build(BuildContext context)
        {
            foreach (var setup in setups)
                setup(builder, context.ConfigurationContext);
            setups.Clear();

            return builder.Build(context);
        }
    }
}