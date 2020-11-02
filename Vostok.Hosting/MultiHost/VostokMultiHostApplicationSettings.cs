using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Components.Shutdown;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.MultiHost
{
    [PublicAPI]
    public class VostokMultiHostApplicationSettings
    {
        public VostokMultiHostApplicationSettings([NotNull] IVostokApplication application, [NotNull] string applicationName, [NotNull] VostokHostingEnvironmentSetup environmentSetup)
        {
            Application = application ?? throw new ArgumentNullException(nameof(application));
            ApplicationName = applicationName ?? throw new ArgumentNullException(nameof(applicationName));
            EnvironmentSetup = environmentSetup ?? throw new ArgumentNullException(nameof(environmentSetup));
        }

        public IVostokApplication Application { get; }

        public string ApplicationName { get; }

        public VostokHostingEnvironmentSetup EnvironmentSetup { get; }
    }
}