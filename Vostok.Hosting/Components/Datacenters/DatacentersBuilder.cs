using System;
using System.Collections.Generic;
using System.Net;
using Vostok.Commons.Helpers;
using Vostok.Datacenters;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Datacenters
{
    internal class DatacentersBuilder : IVostokDatacentersBuilder, IBuilder<IDatacenters>
    {
        private readonly Customization<DatacentersSettings> settingsCustomization;
        private volatile Func<IPAddress, string> datacenterMapping;
        private volatile Func<IReadOnlyCollection<string>> activeDatacentersProvider;
        private volatile IDatacenters instance;

        public DatacentersBuilder() =>
            settingsCustomization = new Customization<DatacentersSettings>();

        public IDatacenters Build(BuildContext context)
        {
            if (instance != null)
            {
                context.ExternalComponents.Add(instance);
                return instance;
            }

            if (datacenterMapping == null)
            {
                context.LogDisabled("Datacenters", "unconfigured mapping");
                return null;
            }

            if (activeDatacentersProvider == null)
            {
                context.LogDisabled("Datacenters", "unconfigured active datacenters provider");
                return null;
            }

            var settings = new DatacentersSettings(datacenterMapping, activeDatacentersProvider);

            settingsCustomization.Customize(settings);

            var result = new Vostok.Datacenters.Datacenters(settings);

            // note (kungurtsev, 10.09.2021): warm up providers to prevent cyclic dependency with ClusterConfigClient
            try
            {
                datacenterMapping.Invoke(IPAddress.None);
                activeDatacentersProvider.Invoke();
            }
            catch (Exception error)
            {
                context.Log.Error(error, "Failed to warm up datacenters.");
            }
            
            return result;
        }

        public IVostokDatacentersBuilder UseInstance(IDatacenters datacenters)
        {
            instance = datacenters ?? throw new ArgumentNullException(nameof(datacenters));
            return this;
        }

        public IVostokDatacentersBuilder SetDatacenterMapping(Func<IPAddress, string> datacenterMapping)
        {
            this.datacenterMapping = datacenterMapping ?? throw new ArgumentNullException(nameof(datacenterMapping));

            instance = null;

            return this;
        }

        public IVostokDatacentersBuilder SetActiveDatacentersProvider(Func<IReadOnlyCollection<string>> activeDatacentersProvider)
        {
            this.activeDatacentersProvider = activeDatacentersProvider ?? throw new ArgumentNullException(nameof(activeDatacentersProvider));

            instance = null;

            return this;
        }

        public IVostokDatacentersBuilder CustomizeSettings(Action<DatacentersSettings> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization ?? throw new ArgumentNullException(nameof(settingsCustomization)));

            instance = null;

            return this;
        }
    }
}