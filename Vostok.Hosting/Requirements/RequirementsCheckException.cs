using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace Vostok.Hosting.Requirements
{
    [PublicAPI]
    public class RequirementsCheckException : Exception
    {
        public RequirementsCheckException(Type applicationType, IReadOnlyList<string> errors)
            : base(FormatMessage(applicationType, errors))
        {
        }

        private static string FormatMessage(Type applicationType, IReadOnlyList<string> errors)
        {
            var builder = new StringBuilder();

            builder.Append($"Configured hosting environment has failed to satisfy some of the application's ('{applicationType.Name}') requirements:");

            foreach (var error in errors)
            {
                builder
                    .AppendLine()
                    .Append('\t')
                    .Append(error);
            }

            return builder.ToString();
        }
    }
}