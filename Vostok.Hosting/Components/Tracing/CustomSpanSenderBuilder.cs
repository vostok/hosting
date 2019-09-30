using Vostok.Tracing.Abstractions;

namespace Vostok.Hosting.Components.Tracing
{
    internal class CustomSpanSenderBuilder : IBuilder<ISpanSender>
    {
        private readonly ISpanSender spanSender;

        public CustomSpanSenderBuilder(ISpanSender spanSender)
        {
            this.spanSender = spanSender;
        }

        public ISpanSender Build(BuildContext context) => spanSender;
    }
}