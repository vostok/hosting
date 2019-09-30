using Vostok.Clusterclient.Core;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.ClusterClient
{
    internal class ClusterClientSetupBuilder : IBuilder<ClusterClientSetup>
    {
        private ClusterClientSetup setup;

        public ClusterClientSetupBuilder()
        {
            setup = _ => {};
        }

        public void Setup(ClusterClientSetup newSetup)
        {
            var oldSetup = setup;

            setup = c =>
            {
                oldSetup(c);
                newSetup(c);
            };
        }

        public ClusterClientSetup Build(BuildContext context)
        {
            return setup;
        }
    }
}