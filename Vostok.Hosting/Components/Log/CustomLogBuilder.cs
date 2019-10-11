using Vostok.Logging.Abstractions;

namespace Vostok.Hosting.Components.Log
{
    internal class CustomLogBuilder : IBuilder<ILog>
    {
        private readonly ILog log;

        public CustomLogBuilder(ILog log)
        {
            this.log = log;
        }

        public ILog Build(BuildContext context) => log;
    }
}