namespace Vostok.Hosting.Helpers
{
    internal class ComponentState
    {
        private volatile ComponentStateFlag flag = ComponentStateFlag.NotInitialized;

        public bool IsEnabled() => flag == ComponentStateFlag.Enabled;

        public void AutoEnable()
        {
            if (flag == ComponentStateFlag.NotInitialized)
            {
                flag = ComponentStateFlag.Enabled;
            }
        }

        public void Enable()
        {
            flag = ComponentStateFlag.Enabled;
        }

        public void Disable()
        {
            flag = ComponentStateFlag.Disabled;
        }

        private enum ComponentStateFlag
        {
            NotInitialized,
            Enabled,
            Disabled
        }
    }
}