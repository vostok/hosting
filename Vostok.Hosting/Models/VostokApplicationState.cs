using JetBrains.Annotations;
using Vostok.Hosting.Abstractions;
using Vostok.ServiceDiscovery.Abstractions;

namespace Vostok.Hosting.Models
{
    /// <summary>
    /// Represents a state of <see cref="IVostokApplication"/> lifecycle.
    /// </summary>
    [PublicAPI]
    public enum VostokApplicationState
    {
        /// <summary>
        /// Initial state of application.
        /// </summary>
        NotInitialized,

        /// <summary>
        /// <see cref="IVostokHostingEnvironment"/> is being constructed.
        /// </summary>
        EnvironmentSetup,

        /// <summary>
        /// <see cref="IVostokHostingEnvironment"/> is being warmed up.
        /// </summary>
        EnvironmentWarmup,

        /// <summary>
        /// <see cref="IVostokApplication.InitializeAsync"/> has been called.
        /// </summary>
        Initializing,

        /// <summary>
        /// <see cref="IVostokApplication.InitializeAsync"/> has completed successfully.
        /// </summary>
        Initialized,

        /// <summary>
        /// <see cref="IVostokApplication.RunAsync"/> has been called.
        /// </summary>
        Running,

        /// <summary>
        /// <see cref="IVostokHostingEnvironment.ShutdownToken"/> has been canceled.
        /// </summary>
        Stopping,

        /// <summary>
        /// Application successfully exited after <see cref="IVostokHostingEnvironment.ShutdownToken"/> had been canceled.
        /// </summary>
        Stopped,

        /// <summary>
        /// Application did not exit in time after <see cref="IVostokHostingEnvironment.ShutdownToken"/> had been canceled.
        /// </summary>
        StoppedForcibly,

        /// <summary>
        /// <see cref="IVostokApplication.RunAsync"/> has completed successfully.
        /// </summary>
        Exited,

        /// <summary>
        /// Construction of <see cref="IVostokHostingEnvironment"/> has failed with an exception.
        /// </summary>
        CrashedDuringEnvironmentSetup,

        /// <summary>
        /// Warmup of <see cref="IVostokHostingEnvironment"/> has failed with an exception.
        /// </summary>
        CrashedDuringEnvironmentWarmup,

        /// <summary>
        /// <see cref="IVostokApplication.InitializeAsync"/> has failed with an exception.
        /// </summary>
        CrashedDuringInitialization,

        /// <summary>
        /// <see cref="IVostokApplication.RunAsync"/> has failed with an exception
        /// or <see cref="IServiceBeacon"/> has not registered in <see cref="VostokHostSettings.BeaconRegistrationTimeout"/>.
        /// </summary>
        CrashedDuringRunning,

        /// <summary>
        /// Application crashed after <see cref="IVostokHostingEnvironment.ShutdownToken"/> had been canceled.
        /// </summary>
        CrashedDuringStopping
    }
}