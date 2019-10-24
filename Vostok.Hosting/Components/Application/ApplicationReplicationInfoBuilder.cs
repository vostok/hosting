using System;
using Vostok.Hosting.Setup;
// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Application
{
    internal class ApplicationReplicationInfoBuilder : IVostokApplicationReplicationInfoBuilder, IBuilder<Func<(int instanceIndex, int instancesCount)>>
    {
        private Func<(int instanceIndex, int instancesCount)> replicationInfoProvider;

        public ApplicationReplicationInfoBuilder()
        {
            replicationInfoProvider = () => (0, 1);
        }

        public IVostokApplicationReplicationInfoBuilder SetReplicationInfo(int instanceIndex, int instancesCount)
        {
            replicationInfoProvider = () => (instanceIndex, instancesCount);
            return this;
        }

        public IVostokApplicationReplicationInfoBuilder SetReplicationInfoProvider(Func<(int instanceIndex, int instancesCount)> replicationInfoProvider)
        {
            this.replicationInfoProvider = replicationInfoProvider;
            return this;
        }

        public Func<(int instanceIndex, int instancesCount)> Build(BuildContext context) =>
            replicationInfoProvider;
    }
}