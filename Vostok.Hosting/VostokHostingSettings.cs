using System;
using JetBrains.Annotations;
using Vostok.Commons.Time;
using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting
{
    [PublicAPI]
    public class VostokHostingSettings
    {
        public VostokHostingSettings([NotNull] IVostokApplication application, [NotNull] VostokHostingEnvironment environment)
        {
            Application = application ?? throw new ArgumentNullException(nameof(application));
            Environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        [NotNull]
        public IVostokApplication Application { get; }

        [NotNull]
        public VostokHostingEnvironment Environment { get; }

        public TimeSpan ShutdownTimeout { get; set; } = 5.Seconds();
    }
}