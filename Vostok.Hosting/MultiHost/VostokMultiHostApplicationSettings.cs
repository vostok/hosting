using System;
using JetBrains.Annotations;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.MultiHost
{
    /// <summary>
    /// Represents configuration of <see cref="IVostokApplication"/> inside <see cref="VostokMultiHost"/>.
    /// </summary>
    [PublicAPI]
    public class VostokMultiHostApplicationSettings
    {
        public VostokMultiHostApplicationSettings([NotNull] IVostokApplication application, [NotNull] VostokMultiHostApplicationIdentifier identifier, VostokHostingEnvironmentSetup environmentSetup = null)
        {
            Application = application ?? throw new ArgumentNullException(nameof(application));
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            EnvironmentSetup = environmentSetup ?? (builder => {});
        }

        /// <summary>
        /// An application which will be run.
        /// </summary>
        public IVostokApplication Application { get; }

        /// <summary>
        /// An application unique identifier.
        /// </summary>
        public VostokMultiHostApplicationIdentifier Identifier { get; }

        /// <summary>
        /// A delegate which will be used to configure <see cref="IVostokHostingEnvironment"/>.
        /// </summary>
        public VostokHostingEnvironmentSetup EnvironmentSetup { get; }
    }
}