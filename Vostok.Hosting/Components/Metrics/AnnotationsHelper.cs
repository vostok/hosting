using System.Linq;
using JetBrains.Annotations;
using Vostok.Commons.Environment;
using Vostok.Hosting.Abstractions;
using Vostok.Metrics;

namespace Vostok.Hosting.Components.Metrics
{
    internal static class AnnotationsHelper
    {
        private const string HostTag = "host";
        private const string EventTypeTag = "vostokEvent";

        public static void ReportLaunching(IVostokApplicationIdentity identity, IMetricContext context, [CanBeNull] LifecycleAnnotationsAdditionalTags additionalTags)
            => context.SendAnnotation($"Instance {identity.Instance} is launching (just started) on host {EnvironmentInfo.Host}.",
                ConstructTags("Launching", additionalTags));

        public static void ReportInitialized(IVostokApplicationIdentity identity, IMetricContext context, [CanBeNull] LifecycleAnnotationsAdditionalTags additionalTags)
            => context.SendAnnotation($"Instance {identity.Instance} has initialized (running now) on host {EnvironmentInfo.Host}.",
                ConstructTags("Initialized", additionalTags));

        public static void ReportStopping(IVostokApplicationIdentity identity, IMetricContext context, [CanBeNull] LifecycleAnnotationsAdditionalTags additionalTags)
            => context.SendAnnotation($"Instance {identity.Instance} is stopping (graceful shutdown triggered) on host {EnvironmentInfo.Host}.",
                ConstructTags("Stopping", additionalTags));

        private static (string, string)[] ConstructTags(string eventType, [CanBeNull] LifecycleAnnotationsAdditionalTags additionalTags)
        {
            var tags = additionalTags?.Tags ?? Enumerable.Empty<(string, string)>();
            return tags
                .Append((HostTag, EnvironmentInfo.Host.ToLowerInvariant()))
                .Append((EventTypeTag, eventType))
                .ToArray();
        }
    }
}