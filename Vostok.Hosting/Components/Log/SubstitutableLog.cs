﻿using System;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Abstractions.Wrappers;

namespace Vostok.Hosting.Components.Log
{
    internal class SubstitutableLog : ILog
    {
        private volatile ILog baseLog = new SilentLog();

        public void Log(LogEvent @event) => baseLog.Log(@event);

        public bool IsEnabledFor(LogLevel level) => baseLog.IsEnabledFor(level);

        public ILog ForContext(string context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return new SourceContextWrapper(this, context);
        }

        public void SubstituteWith(ILog newLog) =>
            baseLog = newLog;
    }
}