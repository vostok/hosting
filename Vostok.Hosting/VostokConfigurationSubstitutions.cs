using JetBrains.Annotations;

namespace Vostok.Hosting
{
    [PublicAPI]
    public static class VostokConfigurationPlaceholders
    {
        public const string IdentityProject = "Vostok.Identity.Project";
        public const string IdentitySubproject = "Vostok.Identity.Subproject";
        public const string IdentityEnvironment = "Vostok.Identity.Environment";
        public const string IdentityApplication = "Vostok.Identity.Application";
        public const string IdentityInstance = "Vostok.Identity.Instance";

        public const string ClusterConfigZone = "Vostok.ClusterConfig.Zone";

        public const string ServiceDiscoveryEnvironment = "Vostok.ServiceDiscovery.Environment";
        public const string ServiceDiscoveryApplication = "Vostok.ServiceDiscovery.Application";

        public const string LocalDatacenter = "Vostok.Datacenters.Local";
    }
}
