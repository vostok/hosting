using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Logging.Abstractions;
// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.Components.Log
{
    internal abstract class LogBuilderBase : IBuilder<ILog>
    {
        protected readonly List<Func<ILog, ILog>> LogCustomizations = new List<Func<ILog, ILog>>();
        
        [CanBeNull]
        public ILog Build(BuildContext context)
        {
            var log = BuildInner(context);
            if (log == null)
                return null;

            foreach (var customization in LogCustomizations)
            {
                log = customization(log);
            }

            return log;
        }

        [CanBeNull]
        protected abstract ILog BuildInner(BuildContext context);
    }
}