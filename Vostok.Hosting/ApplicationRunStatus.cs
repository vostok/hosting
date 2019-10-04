using JetBrains.Annotations;

namespace Vostok.Hosting
{
    [PublicAPI]
    public enum ApplicationRunStatus
    {
        ApplicationStopped,
        ApplicationExited,
        // CR(iloktionov): Crashed during init vs crashed while running
        ApplicationCrashed
    }
}