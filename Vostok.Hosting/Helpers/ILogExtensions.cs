using Vostok.Commons.Environment;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Components;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Context;
using Vostok.Logging.Tracing;

namespace Vostok.Hosting.Helpers
{
    internal static class ILogExtensions
    {
        public static ILog WithEnrichedProperties(this ILog log, BuildContext context) =>
            log
                .WithApplicationIdentityProperties(context.ApplicationIdentity)
                .WithTracingProperties(context.Tracer)
                .WithOperationContext()
                .WithProperty("hostName", EnvironmentInfo.Host);

        private static ILog WithApplicationIdentityProperties(this ILog log, IVostokApplicationIdentity applicationIdentity)
        {
            log = log.WithProperty(WellKnownApplicationIdentityProperties.Project, applicationIdentity.Project);
            if (applicationIdentity.Subproject != null)
                log = log.WithProperty(WellKnownApplicationIdentityProperties.Subproject, applicationIdentity.Subproject);
            log = log.WithProperty(WellKnownApplicationIdentityProperties.Environment, applicationIdentity.Environment);
            log = log.WithProperty(WellKnownApplicationIdentityProperties.Application, applicationIdentity.Application);
            log = log.WithProperty(WellKnownApplicationIdentityProperties.Instance, applicationIdentity.Instance);
            return log;
        }
    }
}