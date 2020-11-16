using System;
using JetBrains.Annotations;
using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting.MultiHost
{
    /// <summary>
    /// A pair of properties that allow to uniquely identify <see cref="IVostokApplication"/> applications inside <see cref="VostokMultiHost"/>.
    /// </summary>
    [PublicAPI]
    public class VostokMultiHostApplicationIdentifier
    {
        public VostokMultiHostApplicationIdentifier([NotNull] string applicationName, [NotNull] string instanceName)
        {
            if (string.IsNullOrWhiteSpace(applicationName))
                throw new ArgumentException("Application name have not been specified.");

            if (string.IsNullOrWhiteSpace(instanceName))
                throw new ArgumentException("Instance name have not been specified.");

            ApplicationName = applicationName;
            InstanceName = instanceName;
        }

        /// <summary>
        /// Application name that is used as default for <see cref="IVostokApplicationIdentity.Application"/>.
        /// Default can be overriden in application builder.
        /// This value can't be null or whitespace.
        /// </summary>
        public string ApplicationName { get; }

        /// <summary>
        /// Instance name that is used as default for <see cref="IVostokApplicationIdentity.Instance"/>.
        /// Default can be overriden in application builder.
        /// This value can't be null or whitespace.
        /// </summary>
        public string InstanceName { get; }

        public override bool Equals(object obj)
        {
            return obj is VostokMultiHostApplicationIdentifier identifier &&
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