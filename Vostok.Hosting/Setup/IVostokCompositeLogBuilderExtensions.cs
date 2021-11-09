using JetBrains.Annotations;
using Vostok.Logging.File.Configuration;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public static class IVostokCompositeLogBuilderExtensions
    {
        /// <summary>
        /// Enables file log.
        /// </summary>
        public static IVostokCompositeLogBuilder SetupFileLog([NotNull] this IVostokCompositeLogBuilder builder) =>
            builder.SetupFileLog(
                fileLogBuilder =>
                {
                    fileLogBuilder.CustomizeSettings(
                        settings =>
                        {
                            settings.RollingStrategy.Type = RollingStrategyType.ByTime;
                            settings.RollingStrategy.Period = RollingPeriod.Day;
                        });
                });

        /// <summary>
        /// Enables console log.
        /// </summary>
        public static IVostokCompositeLogBuilder SetupConsoleLog([NotNull] this IVostokCompositeLogBuilder builder) =>
            builder.SetupConsoleLog(_ => {});
    }
}