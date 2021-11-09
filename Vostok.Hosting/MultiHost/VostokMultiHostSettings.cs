using JetBrains.Annotations;
using Vostok.Hosting.Components.ThreadPool;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.MultiHost
{
    [PublicAPI]
    public class VostokMultiHostSettings
    {
        public VostokMultiHostSettings(VostokHostingEnvironmentSetup builder)
        {
            EnvironmentSetup = builder;
        }

        /// <summary>
        /// A delegate which will be used:
        ///  <list type="bullet">
        ///     <item><description>To configure common environment in <see cref="VostokMultiHost"/>.</description></item>
        ///     <item><description>As default settings in child applications.</description></item>
        /// </list>
        /// </summary>
        [NotNull]
        public VostokHostingEnvironmentSetup EnvironmentSetup { get; set; }

        /// <inheritdoc cref="VostokHostSettings.ConfigureStaticProviders"/>
        public bool ConfigureStaticProviders { get; set; } = true;

        /// <inheritdoc cref="VostokHostSettings.ConfigureThreadPool"/>
        public bool ConfigureThreadPool { get; set; } = true;

        /// <inheritdoc cref="VostokHostSettings.ThreadPoolTuningMultiplier"/>
        public int ThreadPoolTuningMultiplier { get; set; } = ThreadPoolConstants.DefaultThreadPoolMultiplier;

        /// <inheritdoc cref="VostokHostSettings.DiagnosticMetricsEnabled"/>
        public bool DiagnosticMetricsEnabled { get; set; }
    }
}