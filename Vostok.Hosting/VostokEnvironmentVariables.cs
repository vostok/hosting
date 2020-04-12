using JetBrains.Annotations;

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

        public const string LocalDatacenter = "VOSTOK_LOCAL_DATACENTER";
        public const string LocalHostname = "VOSTOK_LOCAL_HOSTNAME";
        public const string LocalFQDN = "VOSTOK_LOCAL_FQDN";

        public const string HostingType = "VOSTOK_HOSTING_TYPE";
    }
}
