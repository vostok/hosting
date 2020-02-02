using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Configuration.Abstractions;
using Vostok.Configuration.Abstractions.Merging;
using Vostok.Configuration.Sources.Combined;
using Vostok.Configuration.Sources.Constant;
using Vostok.Configuration.Sources.Object;
using Vostok.Logging.Configuration;

namespace Vostok.Hosting.Components.Log
{
    internal class LogRulesBuilder : IBuilder<IObservable<LogConfigurationRule[]>>
    {
        private readonly List<LogConfigurationRule> userRules = new List<LogConfigurationRule>();
        private readonly List<IConfigurationSource> userSources = new List<IConfigurationSource>();

        public IObservable<LogConfigurationRule[]> Build(BuildContext context)
        {
            var rulesSources = new List<IConfigurationSource>();

            rulesSources.AddRange(userSources);
            rulesSources.Add(new ObjectSource(userRules));

            var mergeOptions = new SettingsMergeOptions { ArrayMergeStyle = ArrayMergeStyle.Union };
            var mergedSource = new CombinedSource(rulesSources, mergeOptions);

            return context.ConfigurationProvider.Observe<LogConfigurationRule[]>(mergedSource);
        }

        public void Add([NotNull] LogConfigurationRule rule)
            => userRules.Add(rule ?? throw new ArgumentNullException(nameof(rule)));

        public void Add([NotNull] IConfigurationSource source)
            => userSources.Add(source ?? throw new ArgumentNullException(nameof(source)));

        public void Clear()
        {
            userRules.Clear();
            userSources.Clear();
        }
    }
}
