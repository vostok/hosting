using System;
using Vostok.Hosting.Setup;

namespace Vostok.Hosting.Components.ThreadPool
{
    internal class DynamicThreadPoolBuilder : IVostokDynamicThreadPoolBuilder, IBuilder<DynamicThreadPoolTracker>
    {
        public DynamicThreadPoolTracker Build(BuildContext context)
        {
            throw new NotImplementedException();
        }
    }
}