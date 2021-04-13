using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Hosting.Abstractions.Helpers;

namespace Vostok.Hosting
{
    /// <inheritdoc cref="IVostokHostShutdown"/>
    [PublicAPI]
    public class VostokHostShutdown : IVostokHostShutdown
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
