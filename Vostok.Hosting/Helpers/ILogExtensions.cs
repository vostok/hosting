using Vostok.Hosting.Abstractions;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.Helpers
{
    internal static class ILogExtensions
    {
        public static void LogDisabled(this ILog log, string name) =>
            log.Info("{ComponentName} has been disabled.", name);

        public static void LogDisabled(this ILog log, string name, string reason) =>
            log.Info("{ComponentName} has been disabled due to {ComponentDisabledReason}.", name, reason);

        public static ILog WithApplicationIdentityProperties(this ILog log, IVostokApplicationIdentity applicationIdentity)
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