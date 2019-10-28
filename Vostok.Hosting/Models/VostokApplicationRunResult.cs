using System;
using JetBrains.Annotations;

namespace Vostok.Hosting.Models
{
    [PublicAPI]
    public class VostokApplicationRunResult
    {
        public readonly VostokApplicationState Status;

        [CanBeNull]
        public readonly Exception Error;

        public VostokApplicationRunResult(VostokApplicationState status, [CanBeNull] Exception error = null)
        {
            Status = status;
            Error = error;
        }
    }
}