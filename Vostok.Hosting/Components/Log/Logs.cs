using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;

namespace Vostok.Hosting.Components.Log
{
    internal class Logs : IDisposable
    {
        private readonly List<ILog> userLogs;
        private readonly Func<ILog, ILog> customization;

        private readonly ILog fileLog;
        private readonly ILog consoleLog;
        private readonly ILog herculesLog;

        public Logs(List<ILog> userLogs, ILog fileLog, ILog consoleLog, ILog herculesLog, Func<ILog, ILog> customization)
        {
            this.userLogs = userLogs;
            this.fileLog = fileLog;
            this.consoleLog = consoleLog;
            this.herculesLog = herculesLog;
            this.customization = customization;
        }

        public ILog BuildCompositeLog(bool withoutHercules = false)
        {
            var result = BuildCompositeLogInner(withoutHercules);
            result = customization(result);
            return result;
        }

        private ILog BuildCompositeLogInner(bool withoutHercules)
        {
            var logs = userLogs.ToList();
            logs.Add(fileLog);
            logs.Add(consoleLog);
            if (!withoutHercules)
                logs.Add(herculesLog);

            logs = logs.Where(l => l != null).ToList();

            switch (logs.Count)
            {
                case 0:
                    return new SilentLog();
                case 1:
                    return logs.Single();
                default:
                    return new CompositeLog(logs.ToArray());
            }
        }

        public void Dispose()
        {
            (fileLog as IDisposable)?.Dispose();
            ConsoleLog.Flush();
        }
    }
}