using Vostok.Hosting.Abstractions;

namespace Vostok.Hosting.Components.ApplicationIdentity
{
    internal class VostokApplicationIdentity : IVostokApplicationIdentity
    {
        public string Project { get; set; }
        public string Environment { get; set; }
        public string Application { get; set; }
        public string Instance { get; set; }
    }
}