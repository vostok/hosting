using JetBrains.Annotations;

namespace Vostok.Hosting
{
    [PublicAPI]
    public enum ApplicationState
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