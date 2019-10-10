using Vostok.Hosting.Abstractions;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.Helpers
{
    internal static class ILogExtensions
    {
        public static ILog WithApplicationIdentityProperties(this ILog log, IVostokApplicationIdentity applicationIdentity)
        {
            if (applicationIdentity == null)
                return log;

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