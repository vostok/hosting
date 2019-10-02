﻿using JetBrains.Annotations;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IApplicationIdentityBuilder
    {
        IApplicationIdentityBuilder SetProject([NotNull] string project);

        IApplicationIdentityBuilder SetSubproject([CanBeNull] string subproject);

        IApplicationIdentityBuilder SetEnvironment([NotNull] string environment);
        IApplicationIdentityBuilder SetEnvironmentFromClusterConfig([NotNull] string path);

        IApplicationIdentityBuilder SetApplication([NotNull] string application);

        IApplicationIdentityBuilder SetInstance([NotNull] string instance);
    }
}