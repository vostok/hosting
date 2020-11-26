using JetBrains.Annotations;

namespace Vostok.Hosting.MultiHost
{
    /// <summary>
    /// Represents a state of <see cref="VostokMultiHost"/> lifecycle.
    /// </summary>
    [PublicAPI]
    public enum VostokMultiHostState
    {
        /// <summary>
        /// Initial state of <see cref="VostokMultiHost"/>.
        /// </summary>
        NotInitialized,

        /// <summary>
        /// <see cref="VostokMultiHost"/> has started and ready to work.
        /// </summary>
        Running,

        /// <summary>
        /// <see cref="VostokMultiHost"/> successfully finished it's work.
        /// </summary>
        Exited,

        /// <summary>
        /// Construction of common environment components has failed with an exception.
        /// </summary>
        CrashedDuringEnvironmentSetup,

        /// <summary>
        /// Application crashed after <see cref="VostokMultiHost.StopAsync"/> had been called.
        /// </summary>
        CrashedDuringStopping
    }
}