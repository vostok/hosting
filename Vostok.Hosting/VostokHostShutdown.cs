using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Vostok.Hosting
{
    /// <summary>
    /// <see cref="VostokHostShutdown"/> extension allows the application to initiate graceful shutdown.
    /// </summary>
    [PublicAPI]
    public class VostokHostShutdown
    {
        public VostokHostShutdown([NotNull] CancellationTokenSource source)
            => Source = source;

        [NotNull]
        public CancellationTokenSource Source { get; }

        public bool IsInitiated => Source.Token.IsCancellationRequested;

        public void Initiate()
            => Task.Run(() => Source.Cancel());
    }
}
