using JetBrains.Annotations;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokApplicationIdentityBuilder
    {
        IVostokApplicationIdentityBuilder SetProject([NotNull] string project);

        IVostokApplicationIdentityBuilder SetSubproject([CanBeNull] string subproject);

        IVostokApplicationIdentityBuilder SetEnvironment([NotNull] string environment);

        IVostokApplicationIdentityBuilder SetApplication([NotNull] string application);

        IVostokApplicationIdentityBuilder SetInstance([NotNull] string instance);
    }
}