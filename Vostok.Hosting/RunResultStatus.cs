using JetBrains.Annotations;

namespace Vostok.Hosting
{
    [PublicAPI]
    public enum RunResultStatus
    {
        ApplicationStopped,
        ApplicationExited,
        ApplicationCrashed
    }
}