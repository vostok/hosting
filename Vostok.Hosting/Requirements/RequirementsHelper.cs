using System;
using JetBrains.Annotations;
using Vostok.Commons.Helpers.Network;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Requirements;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.Requirements
{
    /// <summary>
    /// A collection of helper methods to aid in satisfying application requirements when building <see cref="IVostokHostingEnvironment"/>.
    /// </summary>
    [PublicAPI]
    public static class RequirementsHelper
    {
        public static void EnsurePort([NotNull] Type applicationType, [NotNull] IVostokHostingEnvironmentBuilder builder)
        {
            if (RequirementDetector.RequiresPort(applicationType))
                builder.SetPort(FreeTcpPortFinder.GetFreePort());
        }
    }
}
