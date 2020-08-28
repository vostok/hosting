using JetBrains.Annotations;

namespace Vostok.Hosting.Components.Log
{
    [PublicAPI]
    public class LogLevelStatistics
    {
        public LogLevelStatistics(
            int debugLogEventsPerMinute,
            int infoLogEventsPerMinute,
            int warnLogEventsPerMinute,
            int errorLogEventsPerMinute,
            int fatalLogEventsPerMinute)
        {
            DebugLogEventsPerMinute = debugLogEventsPerMinute;
            InfoLogEventsPerMinute = infoLogEventsPerMinute;
            WarnLogEventsPerMinute = warnLogEventsPerMinute;
            ErrorLogEventsPerMinute = errorLogEventsPerMinute;
            FatalLogEventsPerMinute = fatalLogEventsPerMinute;
        }

        public int DebugLogEventsPerMinute { get; }
        public int InfoLogEventsPerMinute { get; }
        public int WarnLogEventsPerMinute { get; }
        public int ErrorLogEventsPerMinute { get; }
        public int FatalLogEventsPerMinute { get; }
    }
}