using System.Threading.Tasks;
using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting
{
    internal class VostokApplication : IVostokApplication
    {
        public Task InitializeAsync(IVostokHostingEnvironment environment) =>
            throw new System.NotImplementedException();

        public Task RunAsync(IVostokHostingEnvironment environment) =>
            throw new System.NotImplementedException();
    }
}