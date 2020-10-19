namespace Vostok.Hosting.VostokMultiHost
{
    public enum VostokMultiHostState
    {
        /// <summary>
        /// Initial state of <see cref="VostokMultiHost"/>.
        /// </summary>
        NotInitialized,

        /// <summary>
        /// Common environment components are being constructed.
        /// </summary>
        EnvironmentSetup,

        /// <summary>
        /// <see cref="VostokMultiHost.RunAsync"/> has been called.
        /// </summary>
        Running,

        /// <summary>
        /// <see cref="VostokMultiHost.StopAsync"/> has been called.
        /// </summary>
        Stopping,

        /// <summary>
        /// <see cref="VostokMultiHost"/> successfully stopped after <see cref="VostokMultiHost.StopAsync"/> had been called.
        /// </summary>
        Stopped,

        /// <summary>
        /// <see cref="VostokMultiHost.RunAsync"/> has completed successfully.
        /// </summary>
        Exited,

        /// <summary>
        /// Construction of common environment components has failed with an exception.
        /// </summary>
        CrashedDuringEnvironmentSetup,

        // TODO: Handle inner app crashes.
        /// <summary>
        /// <see cref="VostokMultiHost.RunAsync"/> has failed with an exception.
        /// </summary>
        CrashedDuringRunning,

        /// <summary>
        /// Application crashed after <see cref="VostokMultiHost.StopAsync"/> had been called.
        /// </summary>
        CrashedDuringStopping
    }
}