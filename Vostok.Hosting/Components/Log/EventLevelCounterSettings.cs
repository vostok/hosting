using JetBrains.Annotations;

namespace Vostok.Hosting.Components.Log
{
    [PublicAPI]
    public class EventLevelCounterSettings
    {
        // Min event level can be added later.

        public bool EnableEventLevelCounter { get; set; } = true;
    }
}