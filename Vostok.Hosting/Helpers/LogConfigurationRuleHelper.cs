using System;
using Vostok.Logging.Configuration;

namespace Vostok.Hosting.Helpers
{
    internal static class LogConfigurationRuleHelper
    {
        public static LogConfigurationRule WithLog(this LogConfigurationRule rule, string log)
        {
            if (rule.Log != log && rule.Log != null)
                throw new ArgumentException($"Rule for {rule.Log} log can't be used for {log} log.");

            rule = new LogConfigurationRule
            {
                Enabled = rule.Enabled,
                Log = log,
                Operation = rule.Operation,
                Properties = rule.Properties,
                Source = rule.Source,
                MinimumLevel = rule.MinimumLevel
            };

            return rule;
        }
    }
}