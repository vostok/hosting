using JetBrains.Annotations;
using Vostok.Commons.Environment;

namespace Vostok.Hosting
{
    [PublicAPI]
    public static class VostokEnvironmentVariables
    {
        public const string IdentityProject = "VOSTOK_IDENTITY_PROJECT";
        public const string IdentitySubproject = "VOSTOK_IDENTITY_SUBPROJECT";
        public const string IdentityEnvironment = "VOSTOK_IDENTITY_ENVIRONMENT";
        public const string IdentityApplication = "VOSTOK_IDENTITY_APPLICATION";
        public const string IdentityInstance = "VOSTOK_IDENTITY_INSTANCE";

        public const string LocalDatacenter = Datacenters.Datacenters.LocalDatacenterVariable;
        public const string LocalHostname = EnvironmentInfo.LocalHostnameVariable;
        public const string LocalFQDN = EnvironmentInfo.LocalFQDNVariable;
        public const string LocalServiceDiscoveryIPv4 = EnvironmentInfo.LocalServiceDiscoveryIPv4;

        public const string HostingType = "VOSTOK_HOSTING_TYPE";
    }
}