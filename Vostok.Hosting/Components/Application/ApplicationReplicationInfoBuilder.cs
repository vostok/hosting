using System;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Models;
using Vostok.Hosting.Setup;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Application
{
    internal class ApplicationReplicationInfoBuilder : IVostokApplicationReplicationInfoBuilder, IBuilder<Func<IVostokApplicationReplicationInfo>>
    {
        private volatile Func<IVostokApplicationReplicationInfo> replicationInfoProvider;

        public ApplicationReplicationInfoBuilder()
            => replicationInfoProvider = () => new VostokApplicationReplicationInfo(0, 1);

        public IVostokApplicationReplicationInfoBuilder SetReplicationInfo(IVostokApplicationReplicationInfo replicationInfo)
        {
            replicationInfo = replicationInfo ?? throw new ArgumentNullException(nameof(replicationInfo));
            replicationInfoProvider = () => replicationInfo;
            return this;
        }

        public IVostokApplicationReplicationInfoBuilder SetReplicationInfoProvider(Func<IVostokApplicationReplicationInfo> replicationInfoProvider)
        {
            this.replicationInfoProvider = replicationInfoProvider ?? throw new ArgumentNullException(nameof(replicationInfoProvider));
            return this;
        }

        Func<IVostokApplicationReplicationInfo> IBuilder<Func<IVostokApplicationReplicationInfo>>.Build(BuildContext context) =>
            replicationInfoProvider;
    }
}