using JetBrains.Annotations;
using Vostok.ZooKeeper.Client.Abstractions.Model.Authentication;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public static class IVostokZooKeeperClientBuilderExtensions
    {
        public static IVostokZooKeeperClientBuilder AddAuthenticationInfo(this IVostokZooKeeperClientBuilder builder, [NotNull] string login, [NotNull] string password)
            => builder.AddAuthenticationInfo(AuthenticationInfo.Digest(login, password));
    }
}