using JetBrains.Annotations;

namespace Vostok.Hosting.Models
{
    [PublicAPI]
    public enum VostokApplicationRunStatus
    {
        Stopped,
        Exited,
        CrashedDuringInitialization,
        CrashedDuringRunning
    }
}