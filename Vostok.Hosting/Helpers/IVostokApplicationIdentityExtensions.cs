using System.Text;
using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting.Helpers
{
    internal static class IVostokApplicationIdentityExtensions
    {
        public static string FormatServiceName(this IVostokApplicationIdentity identity)
        {
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