using JetBrains.Annotations;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public static class IVostokServiceBeaconBuilderExtensions
    {
        public static IVostokServiceBeaconBuilder UseFQDN([NotNull] this IVostokServiceBeaconBuilder builder)
            => builder.CustomizeSettings(settings => settings.UseFQDN = true);

        public static IVostokServiceBeaconBuilder UseHostname([NotNull] this IVostokServiceBeaconBuilder builder)
            => builder.CustomizeSettings(settings => settings.UseFQDN = false);
    }
}