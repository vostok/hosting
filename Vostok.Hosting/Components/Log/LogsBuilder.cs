using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Commons.Helpers;
using Vostok.Hosting.Helpers;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Context;
using Vostok.Logging.Tracing;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Log
{
    internal class LogsBuilder : IVostokCompositeLogBuilder, IBuilder<Logs>
    {
        private readonly List<ILog> userLogs;
        private readonly HerculesLogBuilder herculesLogBuilder;
        private readonly FileLogBuilder fileLogBuilder;
        private readonly ConsoleLogBuilder consoleLogBuilder;
        private readonly Customization<ILog> logCustomization;

        public LogsBuilder()
        {
            userLogs = new List<ILog>();
            herculesLogBuilder = new HerculesLogBuilder();
            fileLogBuilder = new FileLogBuilder();
            consoleLogBuilder = new ConsoleLogBuilder();
            logCustomization = new Customization<ILog>();
        }

        [NotNull]
        public Logs Build(BuildContext context)
        {
            return new Logs(
                userLogs,
                fileLogBuilder.Build(context),
                consoleLogBuilder.Build(context),
                herculesLogBuilder.Build(context),
                finalLog => logCustomization.Customize(
                    finalLog
                        .WithApplicationIdentityProperties(context.ApplicationIdentity)
                        .WithTracingProperties(context.Tracer)
                        .WithOperationContext()));
        }

        public IVostokCompositeLogBuilder AddLog(ILog log)
        {
            userLogs.Add(log ?? throw new ArgumentNullException(nameof(log)));
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