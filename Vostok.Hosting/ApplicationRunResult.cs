using System;
using JetBrains.Annotations;

namespace Vostok.Hosting
{
    [PublicAPI]
    public class ApplicationRunResult
    {
        public readonly ApplicationRunStatus Status;

        [CanBeNull]
        public readonly Exception Error;

        public ApplicationRunResult(ApplicationRunStatus status, Exception error = null)
        {
            Status = status;
            Error = error;
        }
    }
}