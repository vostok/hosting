using System.Text;
using JetBrains.Annotations;
using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting.Helpers
{
    internal static class IVostokApplicationIdentityExtensions
    {
        public static string FormatServiceName([CanBeNull] this IVostokApplicationIdentity identity)
        {
            if (identity == null)
                return null;

            var result = new StringBuilder();

            result.Append(identity.Project);

            if (identity.Subproject != null)
            {
                result.Append(".");
                result.Append(identity.Subproject);
            }

            result.Append(".");
            result.Append(identity.Application);

            return result.ToString();
        }
    }
}