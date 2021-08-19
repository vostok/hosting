using JetBrains.Annotations;

namespace Vostok.Hosting.Components.Diagnostics
{
    /// <summary>
    /// Built-in hosting diagnostic health checks names.
    /// </summary>
    [PublicAPI]
    public class WellKnownHealthCheckNames
    {
        public static string DatacenterWhitelist = "Datacenter whitelist";

        public static string ThreadPoolStarvation = "Thread pool";

        public static string ZooKeeperConnection = "ZooKeeper connection";
    }
}