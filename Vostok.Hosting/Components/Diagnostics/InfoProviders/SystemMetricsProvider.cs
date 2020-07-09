using System;
using Vostok.Commons.Time;
using Vostok.Configuration.Primitives;
using Vostok.Hosting.Abstractions.Diagnostics;
using Vostok.Metrics.System.Process;

namespace Vostok.Hosting.Components.Diagnostics.InfoProviders
{
    internal class SystemMetricsProvider : IDiagnosticInfoProvider, IObserver<CurrentProcessMetrics>
    {
        private static readonly TimeSpan ObservationPeriod = 5.Seconds();

        private readonly CurrentProcessMonitor monitor = new CurrentProcessMonitor();
        private volatile CurrentProcessMetrics metrics = new CurrentProcessMetrics();

        public SystemMetricsProvider()
            => monitor.ObserveMetrics(ObservationPeriod).Subscribe(this);

        public object Query()
        {
            return new
            {
                CpuTotalCores = System.Environment.ProcessorCount,
                CpuUtilizedPercent = (metrics.CpuUtilizedFraction * 100d).ToString("0.00"),
                CpuUtilizedCores = metrics.CpuUtilizedCores.ToString("0.00"),
                MemoryResident = metrics.MemoryResident.Bytes(),
                MemoryPrivate = metrics.MemoryPrivate.Bytes(),
                GcHeapSize = metrics.GcHeapSize.Bytes(),
                GcGen0Size = metrics.GcGen0Size.Bytes(),
                GcGen1Size = metrics.GcGen1Size.Bytes(),
                GcGen2Size = metrics.GcGen2Size.Bytes(),
                GcLOHSize = metrics.GcLOHSize.Bytes(),
                metrics.GcGen0Collections,
                metrics.GcGen1Collections,
                metrics.GcGen2Collections,
                metrics.GcTimePercent,
                ThreadPoolTotalThreads = metrics.ThreadPoolTotalCount,
                ThreadPoolQueue = metrics.ThreadPoolQueueLength,
                ThreadPoolWorkers = $"{metrics.ThreadPoolBusyWorkers}/{metrics.ThreadPoolMinWorkers}", 
                ThreadPoolIO = $"{metrics.ThreadPoolBusyIo}/{metrics.ThreadPoolMinIo}",
                metrics.HandlesCount,
                metrics.ActiveTimersCount,
                AllocationRate = metrics.GcAllocatedBytes.Bytes() / ObservationPeriod,
                ContentionsPerSecond = (metrics.LockContentionCount / ObservationPeriod.TotalSeconds).ToString("0.00"),
                ExceptionsPerSecond = (metrics.LockContentionCount / ObservationPeriod.TotalSeconds).ToString("0.00")
            };
        }

        public void OnNext(CurrentProcessMetrics value)
            => metrics = value;

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }
    }
}
