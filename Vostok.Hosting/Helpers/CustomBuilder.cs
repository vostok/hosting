using Vostok.Hosting.Components;

namespace Vostok.Hosting.Helpers
{
    internal class CustomBuilder<T> : IBuilder<T>
    {
        private readonly T value;

        public CustomBuilder(T value)
        {
            this.value = value;
        }

        public T Build(BuildContext context) => value;
    }
}