using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Hosting.Models;

namespace Vostok.Hosting.MultiHost
{
    [PublicAPI]
    public interface IVostokMultiHostApplication
    {
        string Name { get; }

        VostokApplicationState ApplicationState { get; }

        Task<VostokApplicationRunResult> RunAsync();

        Task StartAsync(VostokApplicationState? stateToAwait = VostokApplicationState.Running);

        Task<VostokApplicationRunResult> StopAsync(bool ensureSuccess = true);
    }
}