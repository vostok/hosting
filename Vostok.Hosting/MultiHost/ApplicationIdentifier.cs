using System;
using JetBrains.Annotations;

namespace Vostok.Hosting.MultiHost
{
    [PublicAPI]
    // TODO: Make doc
    public class ApplicationIdentifier
    {
        // TODO: Check if empty?
        public ApplicationIdentifier([NotNull] string applicationName, [NotNull] string instanceName)
        {
            ApplicationName = applicationName ?? throw new ArgumentNullException(nameof(applicationName));
            InstanceName = instanceName ?? throw new ArgumentNullException(nameof(instanceName));
        }

        public string ApplicationName { get; }
        public string InstanceName { get; }

        public override bool Equals(object obj)
        {
            return obj is ApplicationIdentifier identifier &&
                   ApplicationName == identifier.ApplicationName &&
                   InstanceName == identifier.InstanceName;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (ApplicationName.GetHashCode() * 397) ^ InstanceName.GetHashCode();
            }
        }

        public override string ToString() => $"{ApplicationName}.{InstanceName}";
    }
}