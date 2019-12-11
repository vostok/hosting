using System;
using JetBrains.Annotations;
using Vostok.ClusterConfig.Client;
using Vostok.Commons.Time;
using Vostok.Configuration;
using Vostok.Datacenters;
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
        /// <para>Determines whether or not to configure following static providers before running the application:</para>
        /// <list type="bullet">
        ///     <item><description><see cref="LogProvider"/></description></item>
        ///     <item><description><see cref="TracerProvider"/></description></item>
        ///     <item><description><see cref="HerculesSinkProvider"/></description></item>
        ///     <item><description><see cref="DatacentersProvider"/></description></item>
        ///     <item><description>ClusterConfigClient.<see cref="ClusterConfigClient.Default"/></description></item>
        ///     <item><description>ConfigurationProvider.<see cref="ConfigurationProvider.Default"/></description></item>
        /// </list>
        /// </summary>
        public bool ConfigureStaticProviders { get; set; } = true;

        /// <summary>
        /// <para>Whether or not to configure thread pool.</para>
        /// <para>If set to <c>false</c>, thread pool should be configured manually before <see cref="VostokHost.RunAsync"/> will be called.</para>
        /// </summary>
        public bool ConfigureThreadPool { get; set; } = true;

        /// <summary>
        /// Timeout for application graceful shutdown after <see cref="IVostokHostingEnvironment.ShutdownToken"/> has been canceled.
        /// </summary>
        public TimeSpan ShutdownTimeout { get; set; } = 5.Seconds();
    }
}