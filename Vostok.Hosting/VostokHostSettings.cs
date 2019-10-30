using System;
using JetBrains.Annotations;
using Vostok.Commons.Time;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.Tracing.Abstractions;

namespace Vostok.Hosting
{
    /// <summary>
    /// Represents configuration of <see cref="VostokHost"/>.
    /// </summary>
    [PublicAPI]
    public class VostokHostSettings
    {
        public VostokHostSettings([NotNull] IVostokApplication application, [CanBeNull] VostokHostingEnvironmentSetup environmentSetup = null)
        {
            Application = application ?? throw new ArgumentNullException(nameof(application));
            EnvironmentSetup = environmentSetup ?? throw new ArgumentNullException(nameof(environmentSetup));
        }

        /// <summary>
        /// An application which will be run.
        /// </summary>
        [NotNull]
        public IVostokApplication Application { get; }

        /// <summary>
        /// A delegate which will be used to configure <see cref="IVostokHostingEnvironment"/>.
        /// </summary>
        [NotNull]
        public VostokHostingEnvironmentSetup EnvironmentSetup { get; }

        /// <summary>
        /// <para>Whether or not to configure static providers before running application.</para>
        /// <para>See <see cref="LogProvider"/>, <see cref="TracerProvider"/> and <see cref="HerculesSinkProvider"/> for more information.</para>
        /// </summary>
        public bool ConfigureStaticProviders { get; set; } = true;

        /// <summary>
        /// <para>Whether or not to configure thread pool.</para>
        /// <para>If set to <c>false</c>, should be configured manually before <see cref="VostokHost.RunAsync"/> will be called.</para>
        /// </summary>
        public bool ConfigureThreadPool { get; set; } = true;

        /// <summary>
        /// Timeout for application graceful shutdown, after <see cref="IVostokHostingEnvironment.ShutdownToken"/> has been canceled.
        /// </summary>
        public TimeSpan ShutdownTimeout { get; set; } = 5.Seconds();
    }
}