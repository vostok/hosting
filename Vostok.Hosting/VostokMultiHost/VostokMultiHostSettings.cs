using Vostok.Hosting.Setup;

namespace Vostok.Hosting.VostokMultiHost
{
    public class VostokMultiHostSettings : VostokHostSettings
    {
        // ReSharper disable once AssignNullToNotNullAttribute
        public VostokMultiHostSettings(VostokHostingEnvironmentSetup builder)
            : base(null, builder)
        {
        }
    }
}