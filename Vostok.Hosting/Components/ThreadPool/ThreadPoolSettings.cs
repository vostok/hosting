using JetBrains.Annotations;

namespace Vostok.Hosting.Components.ThreadPool
{
    [PublicAPI]
    public class ThreadPoolSettings
    {
        public int ThreadPoolMultiplier { get; set; }
        
        internal float? CpuUnits { get; set; }
    }
}