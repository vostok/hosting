using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using JetBrains.Annotations;
using Vostok.Hosting.Models;

namespace Vostok.Hosting.MultiHost
{
    /// <summary>
    /// Represents a <see cref="VostokMultiHost"/> run result.
    /// </summary>
    [PublicAPI]
    public class VostokMultiHostRunResult
    {
        /// <summary>
        /// <para>Returns final state of <see cref="VostokMultiHost"/>.</para>
        /// <para>May return <see cref="VostokMultiHostState.NotInitialized"/> if <see cref="VostokMultiHost"/> was not launched.</para>
        /// <para>Possible final states:</para>
        /// <list type="bullet">
        ///     <item><description><see cref="VostokMultiHostState.Exited"/></description></item>
        ///     <item><description><see cref="VostokMultiHostState.CrashedDuringEnvironmentSetup"/></description></item>
        ///     <item><description><see cref="VostokApplicationState.CrashedDuringStopping"/></description></item>
        /// </list>
        /// </summary>
        public readonly VostokMultiHostState State;

        /// <summary>
        /// Returns an optional exception that may accompany failed <see cref="State"/>.
        /// </summary>
        [CanBeNull]
        public readonly Exception Error;

        /// <summary>
        /// Contains information about <see cref="VostokApplicationRunResult"/> of applications that were added to <see cref="VostokMultiHost"/>.
        /// </summary>
        [CanBeNull]
        public readonly Dictionary<VostokMultiHostApplicationIdentifier, VostokApplicationRunResult> ApplicationRunResults;

        public VostokMultiHostRunResult(VostokMultiHostState state, [CanBeNull] Exception error = null)
        {
            State = state;
            Error = error;
        }

        public VostokMultiHostRunResult(VostokMultiHostState state, Dictionary<VostokMultiHostApplicationIdentifier, VostokApplicationRunResult> applicationRunResults, [CanBeNull] Exception error = null)
        {
            State = state;
            Error = error;
            ApplicationRunResults = applicationRunResults;
        }

        /// <summary>
        /// Returns <c>true</c> if <see cref="VostokMultiHost"/> has crashed with an exception or <c>false</c> otherwise.
        /// </summary>
        public bool Crashed => Error != null;

        /// <summary>
        /// Throws the <see cref="Error"/> if <see cref="VostokMultiHost"/> has crashed.
        /// </summary>
        public VostokMultiHostRunResult EnsureSuccess()
        {
            if (Error != null)
                ExceptionDispatchInfo.Capture(Error).Throw();
            ;

            return this;
        }
    }
}