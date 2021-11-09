using System;
using Vostok.Configuration.Primitives;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Diagnostics;

namespace Vostok.Hosting.Components.Diagnostics.InfoProviders
{
    internal class ApplicationInfoProvider : IDiagnosticInfoProvider
    {
        private readonly IVostokApplicationIdentity identity;
        private readonly IVostokApplicationLimits limits;
        private readonly Func<IVostokApplicationReplicationInfo> replication;

        public ApplicationInfoProvider(
            IVostokApplicationIdentity identity,
            IVostokApplicationLimits limits,
            Func<IVostokApplicationReplicationInfo> replication)
        {
            this.identity = identity;
            this.limits = limits;
            this.replication = replication;
        }

        public object Query() => new
        {
            Identity = new
            {
                identity.Project,
                identity.Subproject,
                identity.Application,
                identity.Environment,
                identity.Instance
            },
            Replication = replication(),
            Limits = new
            {
                Cpu = limits.CpuUnits.HasValue ? $"{limits.CpuUnits.Value:F2} core(s)" : "<unlimited>",
                Memory = limits.MemoryBytes.HasValue ? limits.MemoryBytes.Value.Bytes().ToString() : "<unlimited>"
            }
        };
    }
}