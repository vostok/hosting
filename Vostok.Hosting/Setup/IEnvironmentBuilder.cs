using System.Threading;
using JetBrains.Annotations;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IEnvironmentBuilder
    {
        IEnvironmentBuilder SetupLog([NotNull] LogSetup logSetup);

        IEnvironmentBuilder SetupApplicationIdentity([NotNull] ApplicationIdentitySetup applicationIdentitySetup);
    }
}