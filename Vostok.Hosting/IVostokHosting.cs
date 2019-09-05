using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting
{
    [PublicAPI]
    public interface IVostokHosting
    {
        IObservable<VostokApplicationState> OnApplicationStateChanged { get; }

        ILog Log { get; }

        Task RunAsync();
    }
}