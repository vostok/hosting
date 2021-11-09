using System.Collections.Generic;
using JetBrains.Annotations;

namespace Vostok.Hosting.Models
{
    [PublicAPI]
    public static class VostokApplicationStateExtensions
    {
        private static readonly HashSet<VostokApplicationState> TerminalStates = new HashSet<VostokApplicationState>
        {
            VostokApplicationState.Exited,
            VostokApplicationState.Stopped,
            VostokApplicationState.StoppedForcibly,
            VostokApplicationState.CrashedDuringEnvironmentSetup,
            VostokApplicationState.CrashedDuringEnvironmentWarmup,
            VostokApplicationState.CrashedDuringInitialization,
            VostokApplicationState.CrashedDuringRunning,
            VostokApplicationState.CrashedDuringStopping
        };

        /// <summary>
        /// Returns whether given <paramref name="state"/> is terminal (a final state of the <see cref="VostokHost"/>.
        /// </summary>
        public static bool IsTerminal(this VostokApplicationState state) => TerminalStates.Contains(state);
    }
}