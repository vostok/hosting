using JetBrains.Annotations;

namespace Vostok.Hosting
{
    [PublicAPI]
    public enum VostokApplicationState
    {
        NotInitialized,
        Initializing,
        Initialized,
        Running,
        Stopping,
        Stopped,
        Exited
    }
}