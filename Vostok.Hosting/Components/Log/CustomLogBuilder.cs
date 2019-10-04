using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.Components.Log
{
    internal class CustomLogBuilder : LogBuilderBase
    {
        private readonly ILog log;

        public CustomLogBuilder(ILog log)
        {
            this.log = log;
        }

        protected override ILog BuildInner(BuildContext context) => log;
    }
}