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

        public int Count(bool withoutHercules = false)
        {
            return ToArray(withoutHercules).Length;
        }

        public ILog BuildCompositeLog(bool withoutHercules = false)
        {
            var result = BuildCompositeLogInner(withoutHercules);
            result = customization(result);
            return result;
        }

        public void Dispose()
        {
            (fileLog as IDisposable)?.Dispose();
            ConsoleLog.Flush();
        }

        private ILog BuildCompositeLogInner(bool withoutHercules)
        {
            var logs = ToArray(withoutHercules);

            switch (logs.Length)
            {
                case 0:
                    return new SilentLog();
                case 1:
                    return logs.Single();
                default:
                    return new CompositeLog(logs.ToArray());
            }
        }

        private ILog[] ToArray(bool withoutHercules)
        {
            var logs = userLogs.ToList();
            logs.Add(fileLog);
            logs.Add(consoleLog);
            if (!withoutHercules)
                logs.Add(herculesLog);

            return logs.Where(l => l != null).ToArray();
        }
    }
}