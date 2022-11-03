using System;
using JetBrains.Annotations;
using Vostok.Commons.Time;
using Vostok.Hosting.Helpers;

namespace Vostok.Hosting
{
    [PublicAPI]
    public class VostokHostingEnvironmentFactorySettings
    {
        /// <summary>
        /// <para>Determines whether to configure static providers before running the application.</para>
        /// <para>See <see cref="StaticProvidersHelper.Configure"/> for more details.</para>
        /// </summary>
        public bool ConfigureStaticProviders { get; set; } = true;

        /// <inheritdoc cref="VostokHostSettings.BeaconShutdownTimeout"/>
        public TimeSpan BeaconShutdownTimeout { get; set; } = 5.Seconds();

        /// <inheritdoc cref="VostokHostSettings.BeaconShutdownWaitEnabled"/>
        public bool BeaconShutdownWaitEnabled { get; set; } = true;

        /// <inheritdoc cref="VostokHostSettings.DisposeComponentTimeout"/>
        public TimeSpan DisposeComponentTimeout { get; set; } = 5.Seconds();

        /// <inheritdoc cref="VostokHostSettings.SendAnnotations"/>
        public bool SendAnnotations { get; set; } = true;

        /// <inheritdoc cref="VostokHostSettings.DiagnosticMetricsEnabled"/>
        public bool DiagnosticMetricsEnabled { get; set; } = true;

        /// <summary>
        /// <list type="bullet">
        ///     <listheader>
        ///         <term>If set to true:</term>
        ///     </listheader>
        ///     <item>
        ///         <description>Shutdown timeout won't be cut</description>
        ///     </item>
        ///     <item>
        ///         <description>Service beacon won't be stopped</description>
        ///     </item>
        /// </list>
        /// </summary>
        public bool SimpleShutdown { get; set; }
    }
}