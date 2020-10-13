using Vostok.Commons.Environment;
using Vostok.Metrics;

namespace Vostok.Hosting.Components.Metrics
{
    internal static class AnnotationsHelper
    {
        private const string HostTag = "host";
        private const string EventTypeTag = "vostokEvent";

        public static void ReportLaunching(IMetricContext context)
            => context.SendAnnotation($"Application is launching (just started) on host {EnvironmentInfo.Host}.", 
                (HostTag, EnvironmentInfo.Host), (EventTypeTag, "Launching"));

        public static void ReportInitialized(IMetricContext context)
            => context.SendAnnotation($"Application has initialized (running now) on host {EnvironmentInfo.Host}.",
                (HostTag, EnvironmentInfo.Host), (EventTypeTag, "Initialized"));

        public static void ReportStopping(IMetricContext context)
            => context.SendAnnotation($"Application is stopping (graceful shutdown) on host {EnvironmentInfo.Host}.",
                (HostTag, EnvironmentInfo.Host), (EventTypeTag, "Stopping"));
    }
}
