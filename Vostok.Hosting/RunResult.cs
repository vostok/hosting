using System;
using JetBrains.Annotations;

namespace Vostok.Hosting
{
    [PublicAPI]
    public class RunResult
    {
        public readonly RunResultStatus Status;

        [CanBeNull]
        public readonly Exception Error;

        public RunResult(RunResultStatus status, Exception error = null)
        {
            Status = status;
            Error = error;
        }
    }
}