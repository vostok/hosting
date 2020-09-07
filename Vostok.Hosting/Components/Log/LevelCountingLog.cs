﻿using System;
using System.Collections.Generic;
using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.Components.Log
{
    internal class LevelCountingLog : ILog
    {
        private readonly Func<IEnumerable<EventLevelCounter>> provider;
        private volatile ILog baseLog;
        
        public LevelCountingLog(ILog baseLog, Func<IEnumerable<EventLevelCounter>> countersProvider)
        {
            this.baseLog = baseLog;
            provider = countersProvider;
        }

        public void Log(LogEvent @event)
        {
            baseLog.Log(@event);

            foreach (var counter in provider())
                counter.HandleEvent(@event);
        }

        public bool IsEnabledFor(LogLevel level) => baseLog.IsEnabledFor(level);

        public ILog ForContext(string context) => new LevelCountingLog(baseLog.ForContext(context), provider);
    }
}