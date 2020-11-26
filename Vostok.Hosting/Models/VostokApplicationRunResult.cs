using System;
using JetBrains.Annotations;
using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting.Models
{
    /// <summary>
    /// Represents a <see cref="IVostokApplication"/> run result.
    /// </summary>
    [PublicAPI]
    public class VostokApplicationRunResult
    {
        /// <summary>
        /// <para>Returns final state of application after <see cref="IVostokApplication.InitializeAsync"/> and <see cref="IVostokApplication.RunAsync"/> had been called.</para>
        /// <para>May return <see cref="VostokApplicationState.NotInitialized"/> if <see cref="VostokHost"/> was not launched.</para>
        /// <para>Possible final states:</para>
        /// <list type="bullet">
        ///     <item><description><see cref="VostokApplicationState.Exited"/></description></item>
        ///     <item><description><see cref="VostokApplicationState.Stopped"/></description></item>
        ///     <item><description><see cref="VostokApplicationState.StoppedForcibly"/></description></item>
        ///     <item><description><see cref="VostokApplicationState.CrashedDuringEnvironmentSetup"/></description></item>
        ///     <item><description><see cref="VostokApplicationState.CrashedDuringEnvironmentWarmup"/></description></item>
        ///     <item><description><see cref="VostokApplicationState.CrashedDuringInitialization"/></description></item>
        ///     <item><description><see cref="VostokApplicationState.CrashedDuringBeaconRegistration"/></description></item>
        ///     <item><description><see cref="VostokApplicationState.CrashedDuringRunning"/></description></item>
        ///     <item><description><see cref="VostokApplicationState.CrashedDuringStopping"/></description></item>
        /// </list>
        /// </summary>
        public readonly VostokApplicationState State;

        /// <summary>
        /// Returns an optional exception that may accompany failed <see cref="State"/>.
        /// </summary>
        [CanBeNull]
        public readonly Exception Error;

        public VostokApplicationRunResult(VostokApplicationState state, [CanBeNull] Exception error = null)
        {
            State = state;
            Error = error;
        }

        /// <summary>
        /// Returns <c>true</c> if the application or host has crashed with an exception or <c>false</c> otherwise.
        /// </summary>
        public bool Crashed => Error != null;

        /// <summary>
        /// Throws the <see cref="Error"/> if application has crashed.
        /// </summary>
        public VostokApplicationRunResult EnsureSuccess()
        {
            if (Error != null)
                throw Error;

            return this;
        }
    }
}