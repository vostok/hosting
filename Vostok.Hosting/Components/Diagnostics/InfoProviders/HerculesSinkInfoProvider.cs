using System.Linq;
using Vostok.Configuration.Primitives;
using Vostok.Hercules.Client;
using Vostok.Hosting.Abstractions.Diagnostics;

namespace Vostok.Hosting.Components.Diagnostics.InfoProviders
{
    internal class HerculesSinkInfoProvider : IDiagnosticInfoProvider
    {
        private readonly HerculesSink sink;

        public HerculesSinkInfoProvider(HerculesSink sink)
            => this.sink = sink;

        public object Query()
        {
            var statistics = sink.GetStatistics();

            return new
            {
                Total = Convert(statistics.Total),
                PerStream = statistics.PerStream
                    .OrderBy(pair => pair.Key)
                    .ToDictionary(pair => pair.Key, pair => Convert(pair.Value))
            };
        }

        private object Convert(HerculesSinkCounters counters)
            => new
            {
                SentRecords = $"{counters.SentRecords.Count} of {counters.SentRecords.Size.Bytes()}",
                StoredRecords = $"{counters.StoredRecords.Count} of {counters.StoredRecords.Size.Bytes()}",
                RejectedRecords = $"{counters.RejectedRecords.Count} of {counters.RejectedRecords.Size.Bytes()}",
                Capacity = counters.Capacity.Bytes(),
                counters.TotalLostRecords,
                counters.RecordsLostDueToBuildFailures,
                counters.RecordsLostDueToSizeLimit,
                counters.RecordsLostDueToOverflows,
            };
    }
}
