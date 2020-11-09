using System;
using System.Collections.Generic;
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
        // CR(iloktionov): StopAsync can return a result with 'NotInitialized' state which is not listed among terminal ones here. Who's actually wrong about this?)
        /// <summary>
        /// <para>Final state of <see cref="VostokMultiHost"/>.</para>
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
        public readonly Dictionary<string, VostokApplicationRunResult> ApplicationRunResults;

        public VostokMultiHostRunResult(VostokMultiHostState state, [CanBeNull] Exception error = null)
        {
            State = state;
            Error = error;
        }

        public VostokMultiHostRunResult(VostokMultiHostState state, Dictionary<string, VostokApplicationRunResult> applicationRunResults, [CanBeNull] Exception error = null)
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
                throw Error;

            return this;
        }
    }
}