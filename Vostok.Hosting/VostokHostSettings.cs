using System;
using JetBrains.Annotations;
using Vostok.Commons.Time;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting
{
    [PublicAPI]
    public class VostokHostSettings
    {
        public VostokHostSettings([NotNull] IVostokApplication application, [CanBeNull] VostokHostingEnvironmentSetup environmentSetup = null)
        {
            Application = application ?? throw new ArgumentNullException(nameof(application));
            EnvironmentSetup = environmentSetup ?? throw new ArgumentNullException(nameof(environmentSetup));
        }

        [NotNull]
        public IVostokApplication Application { get; }

        [NotNull]
        public VostokHostingEnvironmentSetup EnvironmentSetup { get; }

        public bool ConfigureStaticProviders { get; set; } = true;

        public bool ConfigureThreadPool { get; set; } = true;

        public TimeSpan ShutdownTimeout { get; set; } = 5.Seconds();
    }
}