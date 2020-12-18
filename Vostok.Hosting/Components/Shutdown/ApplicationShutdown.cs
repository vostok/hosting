using System;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Commons.Time;
using Vostok.Hosting.Abstractions;
using Vostok.Logging.Abstractions;

// ReSharper disable MethodSupportsCancellation

namespace Vostok.Hosting.Components.Shutdown
{
    internal class ApplicationShutdown
    {
        private readonly ILog log;
        private readonly TimeSpan initialTimeout;

        private readonly CancellationTokenSource tokenSource;
        private readonly TaskCompletionSource<bool> taskSource;
        private volatile TimeBudget budget;

        public ApplicationShutdown(ILog log, TimeSpan initialTimeout)
        {
            this.log = log.ForContext<ApplicationShutdown>();
            this.initialTimeout = initialTimeout;

            tokenSource = new CancellationTokenSource();
            taskSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        /// <summary>
        /// <para>Returns the shutdown token to be exposed to the application via <see cref="IVostokHostingEnvironment.ShutdownToken"/> property.</para>
        /// <para>May be signaled after the hosting shutdown token with a significant delay.</para>
        /// </summary>
        public CancellationToken ShutdownToken => tokenSource.Token;

        /// <summary>
        /// <para>Returns the shutdown timeout to be exposed to the application via <see cref="IVostokHostingEnvironment.ShutdownTimeout"/> property.</para>
        /// <para>Initially returns an best-guess lower bound estimate on how much time the application will have.</para>
        /// <para>After shutdown has been initiated, returns true remaining time.</para>
        /// </summary>
        public TimeSpan ShutdownTimeout => budget?.Remaining ?? initialTimeout;

        /// <summary>
        /// <para>Returns a task that completes right before <see cref="ShutdownToken"/> gets signaled.</para>
        /// <para>It's preferable to use this for waiting instead of direct <see cref="ShutdownToken"/> subscription as such callbacks may experience arbitrarily high delays due to user code.</para>
        /// </summary>
        public Task ShutdownTask => taskSource.Task;

        public void Initiate(TimeSpan remainingTotalBudget)
        {
            log.Info("Application shutdown has been initiated. Timeout = {ApplicationShutdownTimeout}.", remainingTotalBudget.ToPrettyString());

            // (iloktionov): Start the shutdown budget so that ShutdownTimeout property will return actual remaining time from here on.
            Interlocked.Exchange(ref budget, TimeBudget.StartNew(remainingTotalBudget));

            // (iloktionov): Complete the task to notify VostokHost without relying on ShutdownToken callbacks.
            taskSource.TrySetResult(true);

            // (iloktionov): Protect against synchronous execution of arbitrary user callbacks.
            Task.Run(() => tokenSource.Cancel());
        }
    }
}
