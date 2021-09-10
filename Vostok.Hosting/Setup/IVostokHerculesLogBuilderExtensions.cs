using JetBrains.Annotations;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Hercules;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public static class IVostokHerculesLogBuilderExtensions
    {
        public static IVostokHerculesLogBuilder SetElkIndex([NotNull] this IVostokHerculesLogBuilder builder, string elkIndex) =>
            builder.CustomizeLog(log => log.WithProperty(WellKnownHerculesLogProperties.ElkIndex, elkIndex));
    }
}