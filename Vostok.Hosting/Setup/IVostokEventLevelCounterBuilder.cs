using System;
using JetBrains.Annotations;
using Vostok.Hosting.Components.Log;

namespace Vostok.Hosting.Setup
{
    [PublicAPI]
    public interface IVostokEventLevelCounterBuilder
    {
        [NotNull]
        IVostokEventLevelCounterBuilder Customize([NotNull] Action<EventLevelCounterSettings> customization);
    }
}