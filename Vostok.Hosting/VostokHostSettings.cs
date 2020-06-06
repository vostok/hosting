using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Commons.Time;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Helpers;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting
{
    /// <summary>
    /// Represents configuration of <see cref="VostokHost"/>.
    /// </summary>
    [PublicAPI]
    public class VostokHostSettings
    {
        public VostokHostSettings([NotNull] IVostokApplication application, [NotNull] VostokHostingEnvironmentSetup environmentSetup)
        {
            Application = application ?? throw new ArgumentNullException(nameof(application));
            EnvironmentSetup = environmentSetup ?? throw new ArgumentNullException(nameof(environmentSetup));
        }

        /// <summary>
        /// An application which will be run.
        /// </summary>
        [NotNull]
        public IVostokApplication Application { get; set; }

        /// <summary>
        /// A delegate which will be used to configure <see cref="IVostokHostingEnvironment"/>.
        /// </summary>
        [NotNull]
        public VostokHostingEnvironmentSetup EnvironmentSetup { get; set; }

        /// <summary>
        /// <para>Determines whether to configure static providers before running the application.</para>
        /// <para>See <see cref="StaticProvidersHelper.Configure"/> for more details.</para>
        /// </summary>
        public bool ConfigureStaticProviders { get; set; } = true;

        /// <summary>
        /// <para>Whether or not to configure thread pool.</para>
        /// <para>If set to <c>false</c>, thread pool should be configured manually before <see cref="VostokHost.RunAsync"/> will be called.</para>
        /// </summary>
        public bool ConfigureThreadPool { get; set; } = true;

        /// <summary>
        /// If set to <c>true</c>, logs application configuration after assembling <see cref="IVostokHostingEnvironment"/>. Requires <see cref="WarmupConfiguration"/>.
        /// </summary>
        public bool LogApplicationConfiguration { get; set; }

        /// <summary>
        /// If set to <c>true</c>, warms up configuration sources before initializing the application. Required by <see cref="LogApplicationConfiguration"/>.
        /// </summary>
        public bool WarmupConfiguration { get; set; } = true;

        /// <summary>
        /// If set to <c>true</c>, warms up ZooKeeper client before initializing the application.
        /// </summary>
        public bool WarmupZooKeeper { get; set; } = true;

        /// <summary>
        /// Timeout for application graceful shutdown after <see cref="IVostokHostingEnvironment.ShutdownToken"/> has been canceled.
        /// </summary>
        public TimeSpan ShutdownTimeout { get; set; } = 15.Seconds();

        /// <summary>
        /// If enabled, <see cref="VostokHost"/> will wait for up to 1/3 of <see cref="ShutdownTimeout"/> (or 5 seconds, whichever is smaller) between stopping <see cref="IVostokHostingEnvironment.ServiceBeacon"/> and initiating app shutdown.
        /// </summary>
        public bool UseGracefulDiscoveryShutdown { get; set; } = true;

        /// <summary>
        /// Per-core thread pool configuration multiplier used when <see cref="ConfigureThreadPool"/> is <c>true</c>.
        /// </summary>
        public int ThreadPoolTuningMultiplier { get; set; } = 32;

        /// <summary>
        /// Additional actions that will be executed right before application initialization.
        /// </summary>
        [NotNull]
        public List<Action<IVostokHostingEnvironment>> BeforeInitializeApplication { get; set; } = new List<Action<IVostokHostingEnvironment>>();
    }
}