using JetBrains.Annotations;

namespace Vostok.Hosting.Components.Log
{
    [PublicAPI]
    public class LogEventsMetrics
    {
        public LogEventsMetrics(
            int debugLogEvents,
            int infoLogEvents,
            int warnLogEvents,
            int errorLogEvents,
            int fatalLogEvents)
        {
            DebugLogEvents = debugLogEvents;
            InfoLogEvents = infoLogEvents;
            WarnLogEvents = warnLogEvents;
            ErrorLogEvents = errorLogEvents;
            FatalLogEvents = fatalLogEvents;
        }

        public int DebugLogEvents { get; }
        public int InfoLogEvents { get; }
        public int WarnLogEvents { get; }
        public int ErrorLogEvents { get; }
        public int FatalLogEvents { get; }
    }
}