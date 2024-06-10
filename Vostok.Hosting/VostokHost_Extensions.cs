using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Vostok.Hosting.Models;

namespace Vostok.Hosting
{
    /// <summary>
    /// A set of extensions for <see cref="VostokHost"/>.
    /// </summary>
    [PublicAPI]
    // Note(kungurtsev): do not rename this class to `VostokHostExtensions` to avoid collision with IVostokHostExtensions implementation.
    public static class VostokHost_Extensions
    {
        /// <inheritdoc cref="VostokHost.RunAsync"/>
        [NotNull]
        public static VostokApplicationRunResult Run([NotNull] this VostokHost vostokHost) =>
            vostokHost.RunAsync().GetAwaiter().GetResult();

        /// <inheritdoc cref="VostokHost.StartAsync"/>
        public static void Start([NotNull] this VostokHost vostokHost) =>
            vostokHost.StartAsync().GetAwaiter().GetResult();

        /// <inheritdoc cref="VostokHost.StartAsync"/>
        public static void Start([NotNull] this VostokHost vostokHost, VostokApplicationState stateToAwait) =>
            vostokHost.StartAsync(stateToAwait).GetAwaiter().GetResult();

        /// <inheritdoc cref="VostokHost.StopAsync"/>
        [NotNull]
        public static VostokApplicationRunResult Stop([NotNull] this VostokHost vostokHost) =>
            vostokHost.StopAsync().GetAwaiter().GetResult();

        /// <inheritdoc cref="VostokHost.StopAsync"/>
        [NotNull]
        public static VostokApplicationRunResult Stop([NotNull] this VostokHost vostokHost, bool ensureSuccess) =>
            vostokHost.StopAsync(ensureSuccess).GetAwaiter().GetResult();

        /// <summary>
        /// Listen <see cref="Console.CancelKeyPress"/> and shutdown VostokHost if called.
        /// </summary>
        public static VostokHost WithConsoleCancellation([NotNull] this VostokHost vostokHost)
        {
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                vostokHost.ShutdownTokenSource.Cancel();
            };

            return vostokHost;
        }

        /// <summary>
        /// Listen <see cref="AppDomain.ProcessExit"/> and shutdown VostokHost if SIGTERM received.
        /// </summary>
        public static VostokHost WithSigtermCancellation([NotNull] this VostokHost vostokHost)
        {
#if NET6_0_OR_GREATER
            PosixSignalRegistration.Create(PosixSignal.SIGTERM, ctx =>
            {
                ctx.Cancel = true;
                vostokHost.Stop(false);
            });
#else
            AppDomain.CurrentDomain.ProcessExit += (_, _) => vostokHost.Stop(false);
#endif
            return vostokHost;
        }
    }
}