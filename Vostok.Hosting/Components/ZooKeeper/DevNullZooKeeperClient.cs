using System;
using System.Threading.Tasks;
using Vostok.Commons.Helpers.Observable;
using Vostok.ZooKeeper.Client.Abstractions;
using Vostok.ZooKeeper.Client.Abstractions.Model;
using Vostok.ZooKeeper.Client.Abstractions.Model.Authentication;
using Vostok.ZooKeeper.Client.Abstractions.Model.Request;
using Vostok.ZooKeeper.Client.Abstractions.Model.Result;

namespace Vostok.Hosting.Components.ZooKeeper
{
    internal class DevNullZooKeeperClient : IZooKeeperClient, IAuthZooKeeperClient
    {
        public IObservable<ConnectionState> OnConnectionStateChanged =>
            new CachingObservable<ConnectionState>(ConnectionState.Disconnected);

        public ConnectionState ConnectionState =>
            ConnectionState.Disconnected;

        public long SessionId => 0L;

        public Task<CreateResult> CreateAsync(CreateRequest request) =>
            throw new NotSupportedException();

        public Task<DeleteResult> DeleteAsync(DeleteRequest request) =>
            throw new NotSupportedException();

        public Task<SetDataResult> SetDataAsync(SetDataRequest request) =>
            throw new NotSupportedException();

        public Task<ExistsResult> ExistsAsync(ExistsRequest request) =>
            throw new NotSupportedException();

        public Task<GetChildrenResult> GetChildrenAsync(GetChildrenRequest request) =>
            throw new NotSupportedException();

        public Task<GetDataResult> GetDataAsync(GetDataRequest request) =>
            throw new NotSupportedException();

        public Task<GetAclResult> GetAclAsync(GetAclRequest request) =>
            throw new NotSupportedException();

        public Task<SetAclResult> SetAclAsync(SetAclRequest request) =>
            throw new NotSupportedException();

        public void AddAuthenticationInfo(AuthenticationInfo authenticationInfo) =>
            throw new NotSupportedException();
    }
}