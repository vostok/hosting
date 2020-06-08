using System.Threading;

namespace Vostok.Hosting.Helpers
{
    internal class DelayedCancellationTokenSource : CancellationTokenSource
    {
        // TODO(kungurtsev): 
        public CancellationToken DelayedToken;
    }
}