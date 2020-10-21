using System.Collections.Generic;

namespace Vostok.Hosting.VostokMultiHost
{
    public static class VostokMultiHostStateExtensions
    {
        private static readonly HashSet<VostokMultiHostState> TerminalStates = new HashSet<VostokMultiHostState>
        {
            VostokMultiHostState.Stopped,
            VostokMultiHostState.CrashedDuringStopping,
            VostokMultiHostState.CrashedDuringEnvironmentSetup
        };

        public static bool IsTerminal(this VostokMultiHostState state)
        {
            return TerminalStates.Contains(state);
        }
    }
}