using System;
using JetBrains.Annotations;
using Vostok.Commons.Time;

namespace Vostok.Hosting.Components.Shutdown
{
    [PublicAPI]
    public static class ShutdownConstants
    {
        public const double CutMaximumRelativeValue = 0.1d;

        public static readonly TimeSpan DefaultShutdownTimeout = 15.Seconds();

        public static readonly TimeSpan DefaultBeaconShutdownTimeout = 5.Seconds();

        public static readonly TimeSpan CutAmountForExternalTimeout = 500.Milliseconds();

        public static readonly TimeSpan CutAmountForBeaconTimeout = 100.Milliseconds();
    }
}