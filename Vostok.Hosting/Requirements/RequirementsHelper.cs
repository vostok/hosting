﻿using System;
using System.Linq;
using JetBrains.Annotations;
using Vostok.Commons.Helpers.Network;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Sources;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Requirements;
using Vostok.Hosting.Helpers;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.Requirements
{
    /// <summary>
    /// A collection of helper methods to aid in satisfying application requirements when configuring a <see cref="IVostokHostingEnvironment"/>.
    /// </summary>
    [PublicAPI]
    public static class RequirementsHelper
    {
        public static void EnsurePort([NotNull] Type applicationType, [NotNull] IVostokHostingEnvironmentBuilder builder)
        {
            if (RequirementDetector.RequiresPort(applicationType))
                builder.SetPort(FreeTcpPortFinder.GetFreePort());
        }

        public static void EnsureConfigurations([NotNull] Type applicationType, [NotNull] IVostokHostingEnvironmentBuilder builder)
        {
            void SetupSource(IConfigurationProvider provider, IConfigurationSource source, string[] scope, Type type)
            {
                if (scope.Any())
                    source = source.ScopeTo(scope);

                provider.SetupSourceFor(source, type);
            }

            builder.SetupConfiguration(
                b => b.CustomizeConfigurationContext(
                    context =>
                    {
                        foreach (var requirement in RequirementDetector.GetRequiredConfigurations(applicationType))
                            SetupSource(context.ConfigurationProvider, context.ConfigurationSource, requirement.Scope, requirement.Type);

                        foreach (var requirement in RequirementDetector.GetRequiredSecretConfigurations(applicationType))
                            SetupSource(context.SecretConfigurationProvider, context.SecretConfigurationSource, requirement.Scope, requirement.Type);
                    }));
        }
    }
}