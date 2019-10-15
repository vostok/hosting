using System;
using System.Collections.Generic;
using JetBrains.Annotations;
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

        public LogsBuilder()
        {
            userLogs = new List<ILog>();

            herculesLogBuilder = new HerculesLogBuilder();
            fileLogBuilder = new FileLogBuilder();
            consoleLogBuilder = new ConsoleLogBuilder();
        }

        [NotNull]
        public Logs Build(BuildContext context)
        {
            return new Logs(
                userLogs,
                fileLogBuilder.Build(context),
                consoleLogBuilder.Build(context),
                herculesLogBuilder.Build(context),
                finalLog => finalLog
                    .WithApplicationIdentityProperties(context.ApplicationIdentity)
                    .WithTracingProperties(context.Tracer)
                    .WithOperationContext());
        }

        public IVostokCompositeLogBuilder AddLog(ILog log)
        {
            userLogs.Add(log);
            return this;
        }

        public IVostokCompositeLogBuilder SetupFileLog(Action<IVostokFileLogBuilder> fileLogSetup)
        {
            fileLogSetup(fileLogBuilder);
            return this;
        }

        public IVostokCompositeLogBuilder SetupConsoleLog(Action<IVostokConsoleLogBuilder> consoleLogSetup)
        {
            consoleLogSetup(consoleLogBuilder);
            return this;
        }

        public IVostokCompositeLogBuilder SetupHerculesLog(Action<IVostokHerculesLogBuilder> herculesLogSetup)
        {
            herculesLogSetup(herculesLogBuilder);
            return this;
        }
    }
}