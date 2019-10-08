using JetBrains.Annotations;

namespace Vostok.Hosting
{
    [PublicAPI]
    public enum VostokApplicationRunStatus
    {
        Stopped,
        Exited,
        CrashedDuringInitialization,
        CrashedDuringRunning,
    }
}