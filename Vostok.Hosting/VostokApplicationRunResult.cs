using System;
using JetBrains.Annotations;

namespace Vostok.Hosting
{
    [PublicAPI]
    public class VostokApplicationRunResult
    {
        public readonly VostokApplicationRunStatus Status;

        [CanBeNull]
        public readonly Exception Error;

        public VostokApplicationRunResult(VostokApplicationRunStatus status, [CanBeNull] Exception error = null)
        {
            Status = status;
            Error = error;
        }
    }
}