using System.Collections.Generic;
using Vostok.ClusterConfig.Client;
using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Configuration.Sources.Templating;
using Vostok.Datacenters;
using Vostok.Hosting.Abstractions;
using Vostok.ServiceDiscovery.Abstractions;

namespace Vostok.Hosting.Components.Configuration
{
    internal static class SubstitutionsProvider
    {
        public static IEnumerable<Substitution> Provide(
            IVostokApplicationIdentity identity,
            IClusterConfigClient clusterConfig,
            IServiceBeacon beacon,
            IDatacenters datacenters)
        {
            if (identity != null)
            {
                yield return new Substitution(VostokConfigurationPlaceholders.IdentityProject, () => identity.Project);
                yield return new Substitution(VostokConfigurationPlaceholders.IdentitySubproject, () => identity.Subproject);
                yield return new Substitution(VostokConfigurationPlaceholders.IdentityEnvironment, () => identity.Environment);
                yield return new Substitution(VostokConfigurationPlaceholders.IdentityApplication, () => identity.Application);
                yield return new Substitution(VostokConfigurationPlaceholders.IdentityInstance, () => identity.Instance);
            }

            if (beacon != null)
            {
                yield return new Substitution(VostokConfigurationPlaceholders.ServiceDiscoveryEnvironment, () => beacon.ReplicaInfo.Environment);
                yield return new Substitution(VostokConfigurationPlaceholders.ServiceDiscoveryApplication, () => beacon.ReplicaInfo.Application);
            }

            if (datacenters != null)
                yield return new Substitution(VostokConfigurationPlaceholders.LocalDatacenter, datacenters.GetLocalDatacenter);

            if (clusterConfig != null)
                yield return new Substitution(VostokConfigurationPlaceholders.ClusterConfigZone, () => (clusterConfig as ClusterConfigClient)?.Zone ?? "default");
        }
    }
}