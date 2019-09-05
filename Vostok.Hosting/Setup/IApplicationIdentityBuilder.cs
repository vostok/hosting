using JetBrains.Annotations;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IApplicationIdentityBuilder
    {
        IApplicationIdentityBuilder SetProject([NotNull] string project);

        IApplicationIdentityBuilder SetEnvironment([NotNull] string environment);

        IApplicationIdentityBuilder SetApplication([NotNull] string application);

        IApplicationIdentityBuilder SetInstance([NotNull] string instance);
    }
}