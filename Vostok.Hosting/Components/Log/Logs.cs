using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.Commons.Helpers.Disposable;
using Vostok.Commons.Time;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Configuration;
using Vostok.Logging.Console;

namespace Vostok.Hosting.Components.Log
{
    internal class Logs
    {
        private readonly List<(string name, ILog log)> userLogs;
        private readonly Func<ILog, ILog> customization;
        private readonly IObservable<LogConfigurationRule[]> rules;

        private readonly Action disposeFileLog;
        private volatile ILog fileLog;
        private volatile ILog consoleLog;
        private volatile ILog herculesLog;
        
        public const string HerculesLogName = "Hercules";
        public const string ConsoleLogName = "Console";
        public const string FileLogName = "File";

        public Logs(
            List<(string name, ILog log)> userLogs,
            (ILog fileLog, Action disposeFileLog) fileLog,
            ILog consoleLog,
            ILog herculesLog,
            IObservable<LogConfigurationRule[]> rules,
            Func<ILog, ILog> customization)
        {
            this.userLogs = userLogs;
            (this.fileLog, disposeFileLog) = fileLog;
            this.consoleLog = consoleLog;
            this.herculesLog = herculesLog;
            this.rules = rules;
            this.customization = customization;
            LogEventLevelCounterFactory = new LogEventLevelCounterFactory();
        }

        public LogEventLevelCounterFactory LogEventLevelCounterFactory { get; }

        public int Count() => SelectLogs().Count();

        public ILog BuildCompositeLog(out string[] configuredLoggers)
        {
            var builder = new ConfigurableLogBuilder();

            foreach (var (name, log) in SelectLogs())
                builder.AddLog(name, log);

            builder.AddLog("_CounterLog", WrapAndAttachCounters(new SilentLog()));

            builder.SetRules(rules);

            var configurableLog = builder.Build();

            configurableLog.WaitForRulesInitialization(1.Seconds());

            configuredLoggers = configurableLog.BaseLogs.Keys.Where(x => x != "_CounterLog").ToArray();

            return customization(configurableLog);
        }

        public void DisposeHerculesLog(BuildContext context)
        {
            if (herculesLog != null)
            {
                context.LogDisposing("HerculesLog");
                herculesLog = null;
                context.Log = BuildCompositeLog(out _);
            }
        }

        public void DisposeFileLog(BuildContext context)
        {
            if (disposeFileLog != null && fileLog != null)
            {
                context.LogDisposing("FileLog");
                fileLog = null;
                context.Log = BuildCompositeLog(out _);
                context.TryDispose(new ActionDisposable(disposeFileLog), "FileLog", shouldLog: false);
            }
        }

        public void DisposeConsoleLog(BuildContext context)
        {
            if (consoleLog != null)
            {
                context.LogDisposing("ConsoleLog");
                consoleLog = null;
                context.Log = BuildCompositeLog(out _);
                context.TryDispose(new ActionDisposable(ConsoleLog.Flush), "ConsoleLog", shouldLog: false);
            }
        }

        private ILog WrapAndAttachCounters(ILog baseLog) => new LevelCountingLog(baseLog, LogEventLevelCounterFactory.GetCounters);

        private IEnumerable<(string name, ILog log)> SelectLogs()
        {
            foreach (var pair in userLogs.Where(pair => pair.log != null))
                yield return pair;

            if (fileLog != null)
                yield return (FileLogName, fileLog);

            if (consoleLog != null)
                yield return (ConsoleLogName, consoleLog);

            if (herculesLog != null)
                yield return (HerculesLogName, herculesLog);
        }
    }
}