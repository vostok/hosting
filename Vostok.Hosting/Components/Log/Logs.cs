using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.Commons.Time;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Configuration;
using Vostok.Logging.Console;

namespace Vostok.Hosting.Components.Log
{
    internal class Logs : IDisposable
    {
        private readonly List<(string name, ILog log)> userLogs;
        private readonly Func<ILog, ILog> customization;
        private readonly IObservable<LogConfigurationRule[]> rules;

        private readonly ILog fileLog;
        private readonly ILog consoleLog;
        private readonly ILog herculesLog;
        
        public LogEventLevelCounterFactory LogEventLevelCounterFactory { get; }

        public Logs(
            List<(string name, ILog log)> userLogs, 
            ILog fileLog, 
            ILog consoleLog, 
            ILog herculesLog,
            IObservable<LogConfigurationRule[]> rules,
            Func<ILog, ILog> customization)
        {
            this.userLogs = userLogs;
            this.fileLog = fileLog;
            this.consoleLog = consoleLog;
            this.herculesLog = herculesLog;
            this.rules = rules;
            this.customization = customization;
            LogEventLevelCounterFactory = new LogEventLevelCounterFactory();
        }

        public int Count(bool withoutHercules = false)
            => SelectLogs(withoutHercules).Count();

        public ILog BuildCompositeLog(bool withoutHercules = false)
        {
            var builder = new ConfigurableLogBuilder();

            foreach (var (name, log) in SelectLogs(withoutHercules))
                builder.AddLog(name, log);

            builder.SetRules(rules);

            var configurableLog = builder.Build();

            configurableLog.WaitForRulesInitialization(1.Seconds());

            LogConfiguredLoggers(configurableLog);

            return customization(WrapAndAttachCounters(configurableLog));
        }

        public void Dispose()
        {
            (fileLog as IDisposable)?.Dispose();
            
            if(consoleLog != null)
                ConsoleLog.Flush();
        }

        private ILog WrapAndAttachCounters(ILog baseLog) => new LevelCountingLog(baseLog, LogEventLevelCounterFactory.GetCounters);

        private IEnumerable<(string name, ILog log)> SelectLogs(bool withoutHercules)
        {
            foreach (var pair in userLogs)
                if (pair.log != null)
                    yield return pair;

            if (fileLog != null)
                yield return ("File", fileLog);

            if (consoleLog != null)
                yield return ("Console", consoleLog);

            if (herculesLog != null && !withoutHercules)
                yield return ("Hercules", herculesLog);
        }

        private void LogConfiguredLoggers(ConfigurableLog log)
            => log.ForContext<ConfigurableLog>().Info("Configured loggers: {ConfiguredLoggers}.", log.BaseLogs.Keys.ToArray());
    }
}