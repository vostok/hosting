using System.Threading.Tasks;
using Vostok.Hosting.Models;

namespace Vostok.Hosting.VostokMultiHost
{
    public interface IVostokMultiHostApplication
    {
        string Name { get; }

        VostokApplicationState ApplicationState { get; }

        Task<VostokApplicationRunResult> RunAsync();

        Task StartAsync(VostokApplicationState? stateToAwait = VostokApplicationState.Running);

        Task<VostokApplicationRunResult> StopAsync(bool ensureSuccess = true);
    }
}