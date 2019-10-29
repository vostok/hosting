using System;
using System.Collections.Generic;
using System.Net;
using Vostok.Datacenters;
using Vostok.Hosting.Helpers;
using Vostok.Hosting.Setup;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Datacenters
{
    internal class DatacentersBuilder : IVostokDatacentersBuilder, IBuilder<IDatacenters>
    {
        private readonly Customization<DatacentersSettings> settingsCustomization;
        private volatile Func<IPAddress, string> datacenterMapping;
        private volatile Func<IReadOnlyCollection<string>> activeDatacentersProvider;

        public DatacentersBuilder() =>
            settingsCustomization = new Customization<DatacentersSettings>();

        public IDatacenters Build(BuildContext context)
        {
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

            return new Vostok.Datacenters.Datacenters(settings);
        }

        public IVostokDatacentersBuilder SetDatacenterMapping(Func<IPAddress, string> datacenterMapping)
        {
            this.datacenterMapping = datacenterMapping;
            return this;
        }

        public IVostokDatacentersBuilder SetActiveDatacentersProvider(Func<IReadOnlyCollection<string>> activeDatacentersProvider)
        {
            this.activeDatacentersProvider = activeDatacentersProvider;
            return this;
        }

        public IVostokDatacentersBuilder CustomizeSettings(Action<DatacentersSettings> settingsCustomization)
        {
            this.settingsCustomization.AddCustomization(settingsCustomization);
            return this;
        }
    }
}