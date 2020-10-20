using Vostok.Commons.Environment;
using Vostok.Hosting.Abstractions;
using Vostok.Metrics;

namespace Vostok.Hosting.Components.Metrics
{
    internal static class AnnotationsHelper
    {
        private const string HostTag = "host";
        private const string EventTypeTag = "vostokEvent";

        public static void ReportLaunching(IVostokApplicationIdentity identity, IMetricContext context)
            => context.SendAnnotation($"Instance {identity.Instance} is launching (just started) on host {EnvironmentInfo.Host}.", 
                (HostTag, EnvironmentInfo.Host), (EventTypeTag, "Launching"));

        public static void ReportInitialized(IVostokApplicationIdentity identity, IMetricContext context)
            => context.SendAnnotation($"Instance {identity.Instance} has initialized (running now) on host {EnvironmentInfo.Host}.",
                (HostTag, EnvironmentInfo.Host), (EventTypeTag, "Initialized"));

        public static void ReportStopping(IVostokApplicationIdentity identity, IMetricContext context)
            => context.SendAnnotation($"Instance {identity.Instance} is stopping (graceful shutdown triggered) on host {EnvironmentInfo.Host}.",
                (HostTag, EnvironmentInfo.Host), (EventTypeTag, "Stopping"));
    }
}
