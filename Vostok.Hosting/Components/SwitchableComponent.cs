using Vostok.Hosting.Helpers;

namespace Vostok.Hosting.Components
{
    internal abstract class SwitchableComponent<T>
        where T : class
    {
        private readonly ComponentState state = new();

        public bool IsEnabled => state.IsEnabled();

        public T Enable()
        {
            state.Enable();
            return this as T;
        }

        public T AutoEnable()
        {
            state.AutoEnable();
            return this as T;
        }

        public T Disable()
        {
            state.Disable();
            return this as T;
        }
    }
}