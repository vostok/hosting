using JetBrains.Annotations;

namespace Vostok.Hosting
{
    [PublicAPI]
    public enum VostokApplicationRunStatus
    {
        ApplicationStopped,
        ApplicationExited,
        // CR(iloktionov): Crashed during init vs crashed while running
        ApplicationCrashed
    }
}