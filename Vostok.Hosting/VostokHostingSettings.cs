using System;
using JetBrains.Annotations;
using Vostok.Commons.Time;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting
{
    [PublicAPI]
    public class VostokHostingSettings
    {
        public VostokHostingSettings([NotNull] IVostokApplication application, [CanBeNull] EnvironmentSetup environmentSetup = null)
        {
            Application = application ?? throw new ArgumentNullException(nameof(application));
            EnvironmentSetup = environmentSetup ?? throw new ArgumentNullException(nameof(environmentSetup));
        }

        [NotNull]
        public IVostokApplication Application { get; }

        [NotNull]
        public EnvironmentSetup EnvironmentSetup { get; }

        public TimeSpan ShutdownTimeout { get; set; } = 5.Seconds();
    }
}