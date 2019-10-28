using System;
using System.Collections.Generic;
using System.Net;
using JetBrains.Annotations;
using Vostok.Datacenters;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokDatacentersBuilder
    {
        IVostokDatacentersBuilder SetDatacenterMapping([NotNull] Func<IPAddress, string> datacenterMapping);

        IVostokDatacentersBuilder SetActiveDatacentersProvider([NotNull] Func<IReadOnlyCollection<string>> activeDatacentersProvider);

        IVostokDatacentersBuilder CustomizeSettings([NotNull] Action<DatacentersSettings> settingsCustomization);
    }
}