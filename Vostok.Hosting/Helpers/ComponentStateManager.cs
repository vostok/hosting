namespace Vostok.Hosting.Helpers
{
    internal class ComponentStateManager
    {
        private volatile ComponentState state = ComponentState.NotInitialized;

        public bool IsEnabled() => state == ComponentState.Enabled;

        public void Enable(bool isAuto)
        {
            if (!isAuto || state == ComponentState.NotInitialized)
            {
                state = ComponentState.Enabled;
            }
        }

        public void Disable()
        {
            state = ComponentState.Disabled;
        }

        private enum ComponentState
        {
            NotInitialized,
            Enabled,
            Disabled
        }
    }
}