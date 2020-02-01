using System;
using System.Threading.Tasks;
using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Commons.Helpers.Observable;
using Vostok.Configuration.Abstractions.SettingsTree;

namespace Vostok.Hosting.Components.Configuration
{
    internal class DevNullClusterConfigClient : IClusterConfigClient
    {
        public ISettingsNode Get(ClusterConfigPath prefix) 
            => null;

        public (ISettingsNode settings, long version) GetWithVersion(ClusterConfigPath prefix) 
            => (null, 0L);

        public Task<ISettingsNode> GetAsync(ClusterConfigPath prefix) 
            => Task.FromResult(null as ISettingsNode);

        public Task<(ISettingsNode settings, long version)> GetWithVersionAsync(ClusterConfigPath prefix)
            => Task.FromResult((null as ISettingsNode, 0L));

        public IObservable<ISettingsNode> Observe(ClusterConfigPath prefix) 
            => new CachingObservable<ISettingsNode>(null);

        public IObservable<(ISettingsNode settings, long version)> ObserveWithVersions(ClusterConfigPath prefix) 
            => new CachingObservable<(ISettingsNode settings, long version)>((null, 0L));
    }
}
