using System;
using Vostok.Clusterclient.Core;
using Vostok.ClusterConfig.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Components.Log;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Abstractions.Wrappers;
using Vostok.ServiceDiscovery.Abstractions;

namespace Vostok.Hosting.Components
{
    internal class Context
    {
        public IVostokApplicationIdentity ApplicationIdentity { get; set; }
        public IServiceLocator ServiceLocator { get; set; }
        public IClusterConfigClient ClusterConfigClient { get; set; }
        public IHerculesSink HerculesSink { get; set; }

        public ILog Log
        {
            get => substitutableLog;
            set => substitutableLog.SubstituteWith(value);
        }

        private readonly SubstitutableLog substitutableLog;
        
        public Context()
        {
            substitutableLog = new SubstitutableLog();
        }
    }
}