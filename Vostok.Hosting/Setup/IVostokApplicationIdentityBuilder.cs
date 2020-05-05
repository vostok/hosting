using JetBrains.Annotations;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokApplicationIdentityBuilder
    {
        [CanBeNull]
        string Project { get; }

        [CanBeNull]
        string Subproject { get; }

        [CanBeNull]
        string Environment { get; }

        [CanBeNull]
        string Application { get; }

        [CanBeNull]
        string Instance { get; }

        IVostokApplicationIdentityBuilder SetProject([NotNull] string project);

        IVostokApplicationIdentityBuilder SetSubproject([CanBeNull] string subproject);

        IVostokApplicationIdentityBuilder SetEnvironment([NotNull] string environment);

        IVostokApplicationIdentityBuilder SetApplication([NotNull] string application);

        IVostokApplicationIdentityBuilder SetInstance([NotNull] string instance);
    }
}