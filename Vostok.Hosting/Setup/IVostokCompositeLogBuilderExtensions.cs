using System;
using JetBrains.Annotations;
using Vostok.Commons.Environment;
using Vostok.Logging.Abstractions;
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

        /// <summary>
        /// Enriches composite log properties with given <see cref="hostName"/>.
        /// Note that properties are enriched with <see cref="EnvironmentInfo.Host"/> by default.
        /// </summary>
        public static IVostokCompositeLogBuilder SetHostName([NotNull] this IVostokCompositeLogBuilder builder, [NotNull] string hostName) =>
            builder.CustomizeLog(log => log.WithProperty("hostName", hostName ?? throw new ArgumentNullException(nameof(hostName))));
    }
}