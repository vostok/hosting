using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Hosting.Models;

namespace Vostok.Hosting.VostokMultiHost
{
    public class VostokMultiHostRunResult
    {
        public readonly VostokMultiHostState State;
        
        [CanBeNull]
        public readonly Exception Error;

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