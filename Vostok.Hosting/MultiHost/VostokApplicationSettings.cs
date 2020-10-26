using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Components.Shutdown;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.MultiHost
{
    [PublicAPI]
    public class VostokApplicationSettings
    {
        public VostokApplicationSettings([NotNull] IVostokApplication application, [NotNull] string applicationName, [NotNull] VostokHostingEnvironmentSetup environmentSetup)
        {
            Application = application ?? throw new ArgumentNullException(nameof(application));
            ApplicationName = applicationName ?? throw new ArgumentNullException(nameof(applicationName));
            EnvironmentSetup = environmentSetup ?? throw new ArgumentNullException(nameof(environmentSetup));
        }

        public IVostokApplication Application { get; }

        public string ApplicationName { get; }

        public VostokHostingEnvironmentSetup EnvironmentSetup { get; set; }

        // CR(iloktionov): Remove these settings (we haven't come up with sensible scenarios for them so far).
        public bool LogApplicationConfiguration { get; set; }

        public bool WarmupConfiguration { get; set; } = true;

        public TimeSpan ShutdownTimeout { get; set; } = ShutdownConstants.DefaultShutdownTimeout;

        public TimeSpan BeaconShutdownTimeout { get; set; } = ShutdownConstants.DefaultBeaconShutdownTimeout;

        public bool BeaconShutdownWaitEnabled { get; set; } = true;

        public List<Action<IVostokHostingEnvironment>> BeforeInitializeApplication { get; set; } = new List<Action<IVostokHostingEnvironment>>();
    }
}