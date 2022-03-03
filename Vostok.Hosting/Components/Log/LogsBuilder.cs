using System;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using Vostok.Commons.Helpers;
using Vostok.Configuration.Abstractions;
using Vostok.Hosting.Helpers;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Configuration;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Log
{
    internal class LogsBuilder : IVostokCompositeLogBuilder, IBuilder<Logs>
    {
        private readonly List<(string name, ILog log)> userLogs;
        private readonly HerculesLogBuilder herculesLogBuilder;
        private readonly FileLogBuilder fileLogBuilder;
        private readonly ConsoleLogBuilder consoleLogBuilder;
        private readonly LogRulesBuilder rulesBuilder;
        private readonly Customization<ILog> logCustomization;
        private volatile Func<LogLevel> minLevelProvider;
        private int unnamedLogsCounter;

        public LogsBuilder()
        {
            userLogs = new List<(string name, ILog log)>();
            rulesBuilder = new LogRulesBuilder();
            herculesLogBuilder = new HerculesLogBuilder(rulesBuilder);
            fileLogBuilder = new FileLogBuilder(rulesBuilder);
            consoleLogBuilder = new ConsoleLogBuilder(rulesBuilder);
            logCustomization = new Customization<ILog>();
        }

        public bool IsFileLogEnabled => fileLogBuilder.IsEnabled;

        public bool IsConsoleLogEnabled => consoleLogBuilder.IsEnabled;

        public bool IsHerculesLogEnabled => herculesLogBuilder.IsEnabled;

        [NotNull]
        public Logs Build(BuildContext context)
        {
            return new Logs(
                userLogs,
                fileLogBuilder.Build(context),
                consoleLogBuilder.Build(context),
                herculesLogBuilder.Build(context),
                rulesBuilder.Build(context),
                finalLog => logCustomization.Customize(
                    (minLevelProvider != null ? finalLog.WithMinimumLevel(minLevelProvider) : finalLog)
                   .WithEnrichedProperties(context)
                )
            );
        }

        public IVostokCompositeLogBuilder AddLog(ILog log)
            => AddLog($"{log.GetType().Name}-{Interlocked.Increment(ref unnamedLogsCounter)}", log);

        public IVostokCompositeLogBuilder AddLog(string name, ILog log)
        {
            userLogs.Add((name ?? throw new ArgumentNullException(nameof(name)), log ?? throw new ArgumentNullException(nameof(log))));
            return this;
        }

        public IVostokCompositeLogBuilder AddRule(LogConfigurationRule rule)
        {
            rulesBuilder.Add(rule);
            return this;
        }

        public IVostokCompositeLogBuilder AddRules(IConfigurationSource source)
        {
            rulesBuilder.Add(source);
            return this;
        }

        public IVostokCompositeLogBuilder ClearRules()
        {
            rulesBuilder.Clear();
            return this;
        }

        public IVostokCompositeLogBuilder SetupMinimumLevelProvider(Func<LogLevel> minLevelProvider)
        {
            this.minLevelProvider = minLevelProvider ?? throw new ArgumentNullException(nameof(minLevelProvider));
            return this;
        }

        public IVostokCompositeLogBuilder CustomizeLog(Func<ILog, ILog> logCustomization)
        {
            this.logCustomization.AddCustomization(logCustomization ?? throw new ArgumentNullException(nameof(logCustomization)));
            return this;
        }

        public IVostokCompositeLogBuilder SetupFileLog(Action<IVostokFileLogBuilder> fileLogSetup)
        {
            fileLogBuilder.Enable();
            fileLogSetup(fileLogBuilder ?? throw new ArgumentNullException(nameof(fileLogSetup)));
            return this;
        }

        public IVostokCompositeLogBuilder SetupConsoleLog()
        {
            consoleLogBuilder.Enable();
            return this;
        }

        public IVostokCompositeLogBuilder SetupConsoleLog(Action<IVostokConsoleLogBuilder> consoleLogSetup)
        {
            consoleLogBuilder.Enable();
            consoleLogSetup(consoleLogBuilder ?? throw new ArgumentNullException(nameof(consoleLogSetup)));
            return this;
        }

        public IVostokCompositeLogBuilder SetupHerculesLog(Action<IVostokHerculesLogBuilder> herculesLogSetup)
        {
            herculesLogBuilder.Enable();
            herculesLogSetup(herculesLogBuilder ?? throw new ArgumentNullException(nameof(herculesLogSetup)));
            return this;
        }
    }
}