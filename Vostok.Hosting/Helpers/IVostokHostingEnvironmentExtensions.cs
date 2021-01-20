using Vostok.Commons.Threading;
using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting.Helpers
{
    public static class IVostokHostingEnvironmentExtensions
    {
        public static void ConfigureThreadPool(this IVostokHostingEnvironment environment, int multiplier)
        {
            if (environment.ApplicationLimits.CpuUnits.HasValue)
                ThreadPoolUtility.Setup(multiplier, environment.ApplicationLimits.CpuUnits.Value);
            else
                ThreadPoolUtility.Setup(multiplier);
        }
    }
}