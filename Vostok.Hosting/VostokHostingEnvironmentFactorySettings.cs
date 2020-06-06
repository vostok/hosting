using JetBrains.Annotations;
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

        internal bool DisconnectShutdownToken { get; set; }
    }
}
