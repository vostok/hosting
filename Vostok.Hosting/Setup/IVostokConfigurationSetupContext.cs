using JetBrains.Annotations;
using Vostok.Datacenters;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.Setup
{
    /// <summary>
    /// Provides a way to use log and application identity during configuration setup.
    /// </summary>
    [PublicAPI]
    public interface IVostokConfigurationSetupContext
    {
        /// <inheritdoc cref="IVostokHostingEnvironment.Log"/>
        [NotNull]
        ILog Log { get; }

        /// <inheritdoc cref="IVostokHostingEnvironment.ApplicationIdentity"/>
        /// <para>Please note that only statically configured fields of the identity are available in this context.</para>
        /// <para>Access to any fields that rely on configuration to be set up will result in exceptions to prevent circular dependencies.</para>
        [NotNull]
        IVostokApplicationIdentity ApplicationIdentity { get; }
        
        /// <inheritdoc cref="IVostokHostingEnvironment.Datacenters"/>
        /// <para>Please note that all methods will return <c>null</c> before configuration initialization.</para>
        [NotNull]
        IDatacenters Datacenters { get; }
    }
}
