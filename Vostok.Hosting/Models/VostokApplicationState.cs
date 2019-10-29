using JetBrains.Annotations;

namespace Vostok.Hosting.Models
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
        StoppedForcibly,
        Exited,
        CrashedDuringInitialization,
        CrashedDuringRunning,
        CrashedDuringStopping
    }
}