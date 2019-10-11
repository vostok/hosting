﻿using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Vostok.Hosting.Helpers;
using Vostok.Hosting.Setup;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Context;
using Vostok.Logging.Tracing;

// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Log
{
    internal class CompositeLogBuilder : IVostokCompositeLogBuilder, IBuilder<ILog>
    {
        private readonly List<IBuilder<ILog>> logBuilders;

        private readonly HerculesLogBuilder herculesLogBuilder;

        public CompositeLogBuilder()
        {
            herculesLogBuilder = new HerculesLogBuilder();

            logBuilders = new List<IBuilder<ILog>> {herculesLogBuilder};
        }

        [NotNull]
        public ILog Build(BuildContext context)
        {
            return BuildCompositeLog(context)
                .WithApplicationIdentityProperties(context.ApplicationIdentity)
                .WithTracingProperties(context.Tracer)
                .WithOperationContext();
        }

        public IVostokCompositeLogBuilder AddLog(ILog log)
        {
            logBuilders.Add(new CustomLogBuilder(log));
            return this;
        }

        public IVostokCompositeLogBuilder SetupHerculesLog(Action<IVostokHerculesLogBuilder> herculesLogSetup)
        {
            herculesLogSetup(herculesLogBuilder);
            return this;
        }

        private ILog BuildCompositeLog(BuildContext context)
        {
            var logs = logBuilders
                .Select(b => b.Build(context))
                .Where(l => l != null)
                .ToArray();

            switch (logs.Length)
            {
                case 0:
                    return new SilentLog();
                case 1:
                    return logs.Single();
                default:
                    return new CompositeLog(logs);
            }
        }
    }
}