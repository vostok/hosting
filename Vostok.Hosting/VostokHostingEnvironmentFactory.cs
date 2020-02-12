using System;
using JetBrains.Annotations;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Components.Environment;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting
{
    [PublicAPI]
    public static class VostokHostingEnvironmentFactory
    {
        /// <summary>
        /// Creates an instance of <see cref="IVostokHostingEnvironment"/>, using given <paramref name="setup"/>.
        /// </summary>
        [NotNull]
        public static IVostokHostingEnvironment Create([NotNull] VostokHostingEnvironmentSetup setup)
            => Create(setup, new VostokHostingEnvironmentFactorySettings());

        /// <summary>
        /// Creates an instance of <see cref="IVostokHostingEnvironment"/>, using given <paramref name="setup"/>.
        /// </summary>
        [NotNull]
        public static IVostokHostingEnvironment Create([NotNull] VostokHostingEnvironmentSetup setup, [NotNull] VostokHostingEnvironmentFactorySettings settings)
            => EnvironmentBuilder.Build(setup ?? throw new ArgumentNullException(nameof(setup)), settings);
    }
}