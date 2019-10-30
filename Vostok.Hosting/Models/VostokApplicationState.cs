using JetBrains.Annotations;
using Vostok.Hosting.Abstractions;

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
        /// Application <see cref="IVostokApplication.InitializeAsync"/> has been called.
        /// </summary>
        Initializing,

        /// <summary>
        /// Application <see cref="IVostokApplication.InitializeAsync"/> has been completed successfully.
        /// </summary>
        Initialized,

        /// <summary>
        /// Application <see cref="IVostokApplication.RunAsync"/> has been called.
        /// </summary>
        Running,

        /// <summary>
        /// Application <see cref="IVostokHostingEnvironment.ShutdownToken"/> has been canceled.
        /// </summary>
        Stopping,

        /// <summary>
        /// Application successfully exited after <see cref="IVostokHostingEnvironment.ShutdownToken"/> has been canceled.
        /// </summary>
        Stopped,

        /// <summary>
        /// Application not exited after <see cref="IVostokHostingEnvironment.ShutdownToken"/> has been canceled within given <see cref="VostokHostSettings.ShutdownTimeout"/>.
        /// </summary>
        StoppedForcibly,

        /// <summary>
        /// Application <see cref="IVostokApplication.RunAsync"/> has been completed successfully.
        /// </summary>
        Exited,

        /// <summary>
        /// Application <see cref="IVostokApplication.InitializeAsync"/> has been failed.
        /// </summary>
        CrashedDuringInitialization,

        /// <summary>
        /// Application <see cref="IVostokApplication.RunAsync"/> has been failed.
        /// </summary>
        CrashedDuringRunning,

        /// <summary>
        /// Application crashed after <see cref="IVostokHostingEnvironment.ShutdownToken"/> has been canceled.
        /// </summary>
        CrashedDuringStopping
    }
}