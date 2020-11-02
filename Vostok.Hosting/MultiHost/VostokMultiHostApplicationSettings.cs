using System;
using JetBrains.Annotations;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.MultiHost
{
    [PublicAPI]
    public class VostokMultiHostApplicationSettings
    {
        public VostokMultiHostApplicationSettings([NotNull] IVostokApplication application, [NotNull] string applicationName, [NotNull] VostokHostingEnvironmentSetup environmentSetup)
        {
            Application = application ?? throw new ArgumentNullException(nameof(application));
            ApplicationName = applicationName ?? throw new ArgumentNullException(nameof(applicationName));
            EnvironmentSetup = environmentSetup ?? throw new ArgumentNullException(nameof(environmentSetup));
        }
        
        /// <summary>
        /// An application which will be run.
        /// </summary>
        public IVostokApplication Application { get; }

        /// <summary>
        /// An application unique identifier.
        /// </summary>
        public string ApplicationName { get; }

        /// <summary>
        /// A delegate which will be used to configure <see cref="IVostokHostingEnvironment"/>.
        /// </summary>
        public VostokHostingEnvironmentSetup EnvironmentSetup { get; }
    }
}