using JetBrains.Annotations;
using Vostok.Commons.Environment;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Hercules;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public static class IVostokHerculesLogBuilderExtensions
    {
        public static IVostokHerculesLogBuilder SetHostname(this IVostokHerculesLogBuilder builder, [CanBeNull] string hostName = null) =>
            builder.CustomizeLog(log => log.WithProperty("hostName", hostName ?? EnvironmentInfo.Host));

        public static IVostokHerculesLogBuilder SetElkIndex([NotNull] this IVostokHerculesLogBuilder builder, string elkIndex) =>
            builder.CustomizeLog(log => log.WithProperty(WellKnownHerculesLogProperties.ElkIndex, elkIndex));

        public static IVostokHerculesLogBuilder SetMinimumLevel([NotNull] this IVostokHerculesLogBuilder builder, LogLevel minLevel) =>
            builder.SetupMinimumLevelProvider(() => minLevel);
    }
}