using JetBrains.Annotations;

namespace Vostok.Hosting.Components.Log
{
    [PublicAPI]
    public class LogLevelStatistics
    {
        public static LogLevelStatistics Zero = new LogLevelStatistics(
            0,
            0,
            0,
            0,
            0);

        public LogLevelStatistics(
            int debugLogAttemptsCount,
            int infoLogAttemptsCount,
            int warnLogAttemptsCount,
            int errorLogAttemptsCount,
            int fatalLogAttemptsCount)
        {
            DebugLogAttemptsCount = debugLogAttemptsCount;
            InfoLogAttemptsCount = infoLogAttemptsCount;
            WarnLogAttemptsCount = warnLogAttemptsCount;
            ErrorLogAttemptsCount = errorLogAttemptsCount;
            FatalLogAttemptsCount = fatalLogAttemptsCount;
        }

        public int DebugLogAttemptsCount { get; }
        public int InfoLogAttemptsCount { get; }
        public int WarnLogAttemptsCount { get; }
        public int ErrorLogAttemptsCount { get; }
        public int FatalLogAttemptsCount { get; }
    }
}