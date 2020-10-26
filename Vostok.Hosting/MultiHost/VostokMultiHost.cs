using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Commons.Helpers.Observable;
using Vostok.Commons.Threading;
using Vostok.Configuration.Abstractions.Extensions.Observable;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Components;
using Vostok.Hosting.Components.Environment;
using Vostok.Hosting.Models;

namespace Vostok.Hosting.MultiHost
{
    // CR(iloktionov): 1. Idea: override Dispose in child environments instead of using CommonBuildContext + utilize UseInstance for CC client, ZK client and HerculesSink
    // CR(iloktionov): 2. Idea: implement Run using Start, stop tracking statuses (Run can just wait for RunAsync tasks of the apps with double-checking), break the cyclic dependency between VostokMultiHost and VostokMultiHostApplication

    /// <summary>
    /// <para>An <see cref="IVostokMultiHostApplication"/> launcher.</para>
    /// <para>It was designed to launch multiple <see cref="IVostokMultiHostApplication"/> at a time.</para>
    /// <para>Shares common environment components between applications.</para>
    /// </summary>
    [PublicAPI]
    public class VostokMultiHost
    {
        public VostokMultiHost(VostokMultiHostSettings settings, params VostokApplicationSettings[] apps)
        {
            this.Settings = settings;
            applications = new ConcurrentDictionary<string, IVostokMultiHostApplication>();
            runningApplications = new ConcurrentDictionary<string, IVostokMultiHostApplication>();

            foreach (var app in apps)
                AddApp(app);

            onHostStateChanged = new CachingObservable<VostokMultiHostState>();
            onRunningApplicationsCountChanged = new CachingObservable<int>();
        }

        /// <summary>
        /// Returns current <see cref="VostokMultiHostState"/>.
        /// </summary>
        public VostokMultiHostState HostState { get; private set; }

        /// <summary>
        /// <para>Returns an observable sequence of host states.</para>
        /// <para>This sequence produces <see cref="IObserver{T}.OnNext"/> notifications every time current <see cref="HostState"/> changes.</para>
        /// <para>This sequence produces <see cref="IObserver{T}.OnError"/> if host crashes.</para>
        /// <para>This sequence produces <see cref="IObserver{T}.OnCompleted"/> notification when host execution completes.</para>
        /// <para>Note that terminal notifications (<see cref="IObserver{T}.OnError"/> and <see cref="IObserver{T}.OnCompleted"/>)
        /// do not guarantee full completion of the task returned by <see cref="RunAsync"/> (the host may run its own cleanup after those).
        /// These notifications merely signify the final, terminal nature of the last reported status.</para>
        /// </summary>
        public IObservable<VostokMultiHostState> OnHostStateChanged => onHostStateChanged;

        // CR(iloktionov): VostokMultiHost: IEnumerable<IVostokMultiHostApplication>
        /// <summary>
        /// Returns an enumerable of added <see cref="IVostokMultiHostApplication"/>.
        /// </summary>
        public IEnumerable<(string appName, IVostokMultiHostApplication app)> Applications => applications.Select(x => (x.Key, x.Value));

        /// <summary>
        /// <para>Initializes itself and launches added apps.</para>
        /// </summary>
        public Task<VostokMultiHostRunResult> RunAsync()
        {
            if (!launchedOnce.TrySetTrue())
                throw new InvalidOperationException("VostokMultiHost can't be launched multiple times.");

            return RunInternalAsync();
        }

        /// <summary>
        /// <para>Initializes itself and launches added apps, but returns just after the initialization. (Fire-and-forget style)</para>
        /// </summary>
        public async Task StartAsync()
        {
            var stateCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            var subscription = OnHostStateChanged.Subscribe(
                state =>
                {
                    if (state == VostokMultiHostState.Running)
                        stateCompletionSource.TrySetResult(true);
                });

            using (subscription)
            {
                var runnerTask = RunInternalAsync(false).ContinueWith(task => task.Result.EnsureSuccess(), TaskContinuationOptions.OnlyOnRanToCompletion);

                var completedTask = Task.WhenAny(runnerTask, stateCompletionSource.Task);

                await completedTask.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// <para>Stops all running applications and dispose itself.</para>
        /// </summary>
        public async Task<VostokMultiHostRunResult> StopAsync()
        {
            if (HostState != VostokMultiHostState.Running)
                throw new InvalidOperationException("VostokMultiHost can't be stopped in a non-running state.");

            var appDict = await StopInternalAsync();

            return DisposeCommonEnvironment() ?? ReturnResult(VostokMultiHostState.Stopped, appDict);
        }

        /// <summary>
        /// <para>Returns added application by name or null if it doesn't exist.</para>
        /// </summary>
        public IVostokMultiHostApplication GetApp(string appName) => applications.TryGetValue(appName, out var app) ? app : null;

        /// <summary>
        /// <para>Adds an application by providing its settings and returns created application.</para>
        /// </summary>
        public IVostokMultiHostApplication AddApp(VostokApplicationSettings vostokApplicationSettings)
        {
            if (applications.ContainsKey(vostokApplicationSettings.ApplicationName))
                throw new ArgumentException("Application with this name has already been added.");

            var previousAppSetup = vostokApplicationSettings.EnvironmentSetup;
            
            // TODO: Setup common things such as HerculesSink, CC, ZK

            vostokApplicationSettings.EnvironmentSetup = builder =>
            {
                Settings.EnvironmentSetup(builder);
                previousAppSetup(builder);
            };

            return applications[vostokApplicationSettings.ApplicationName] = new VostokMultiHostApplication(vostokApplicationSettings);
        }

        /// <summary>
        /// <para>Removes an application (stops it if it's necessary).</para>
        /// </summary>
        public async Task<VostokApplicationRunResult> RemoveAppAsync(string appName)
        {
            if (applications.TryRemove(appName, out var app))
                return await app.StopAsync();
            throw new InvalidOperationException("VostokMultiHost doesn't contain application with this name.");
        }
        
        private VostokHostingEnvironment commonEnvironment { get; set; }
        private VostokMultiHostSettings Settings { get; set; }
        private readonly ConcurrentDictionary<string, IVostokMultiHostApplication> applications;
        private readonly ConcurrentDictionary<string, IVostokMultiHostApplication> runningApplications;
        private readonly AtomicBoolean launchedOnce = false;
        private readonly CachingObservable<VostokMultiHostState> onHostStateChanged;
        private readonly CachingObservable<int> onRunningApplicationsCountChanged;


        internal void AddRunningApp(string appName, IVostokMultiHostApplication app)
        {
            if (runningApplications.ContainsKey(appName))
                throw new ArgumentException("Application with this name has already been added.");

            runningApplications[appName] = app;
            onRunningApplicationsCountChanged.Next(runningApplications.Count);
        }

        internal void RemoveRunningApp(string appName)
        {
            if (!runningApplications.TryRemove(appName, out _))
                throw new InvalidOperationException("VostokMultiHost doesn't contain application with this name.");

            onRunningApplicationsCountChanged.Next(runningApplications.Count);
        }

        private async Task<VostokMultiHostRunResult> RunInternalAsync(bool stopAndDispose = true)
        {
            var contextBuildResult = BuildCommonContext();

            if (contextBuildResult != null)
                return contextBuildResult;

            ChangeStateTo(VostokMultiHostState.Running);

            if (applications.Count > 0)
            {
                await Task.WhenAll(Applications.Select(x => x.app.StartAsync()));

                var stateCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

                var subscription = onRunningApplicationsCountChanged.Subscribe(
                    value =>
                    {
                        if (value == 0)
                            stateCompletionSource.TrySetResult(true);
                    });

                using (subscription)
                    await stateCompletionSource.Task.ConfigureAwait(false);
            }

            if (stopAndDispose)
            {
                var appDict = await StopInternalAsync();
                return DisposeCommonEnvironment() ?? ReturnResult(VostokMultiHostState.Stopped, appDict);
            }

            return new VostokMultiHostRunResult(VostokMultiHostState.Running);
        }

        private async Task<Dictionary<string, VostokApplicationRunResult>> StopInternalAsync()
        {
            ChangeStateTo(VostokMultiHostState.Stopping);

            var resultDict = new ConcurrentDictionary<string, VostokApplicationRunResult>();

            await Task.WhenAll(
                Applications.Select(
                    x => Task.Run(
                        async () => resultDict[x.appName] = await x.app.StopAsync(false)
                    )
                )
            );

            return resultDict.ToDictionary(
                x => x.Key,
                y => y.Value);
        }

        [CanBeNull]
        private VostokMultiHostRunResult DisposeCommonEnvironment()
        {
            try
            {
                commonEnvironment.Dispose();
                
                return null;
            }
            catch (Exception error)
            {
                return ReturnResult(VostokMultiHostState.CrashedDuringStopping, error);
            }
        }

        [CanBeNull]
        private VostokMultiHostRunResult BuildCommonContext()
        {
            ChangeStateTo(VostokMultiHostState.EnvironmentSetup);

            try
            {
                var environmentFactorySettings = new VostokHostingEnvironmentFactorySettings
                {
                    ConfigureStaticProviders = Settings.ConfigureStaticProviders,
                    BeaconShutdownTimeout = Settings.BeaconShutdownTimeout,
                    BeaconShutdownWaitEnabled = Settings.BeaconShutdownWaitEnabled
                };

                // TODO: Remove unnecessary things such as tracer, metrics etc...
                commonEnvironment = EnvironmentBuilder.Build(Settings.EnvironmentSetup, environmentFactorySettings);
                return null;
            }
            catch (Exception error)
            {
                return ReturnResult(VostokMultiHostState.CrashedDuringEnvironmentSetup, error);
            }
        }

        private VostokMultiHostRunResult ReturnResult(VostokMultiHostState newState, Exception error = null) => ReturnResult(newState, null, error);

        private VostokMultiHostRunResult ReturnResult(VostokMultiHostState newState, Dictionary<string, VostokApplicationRunResult> appDict, Exception error = null)
        {
            ChangeStateTo(newState, error);
            return new VostokMultiHostRunResult(newState, appDict, error);
        }

        private void ChangeStateTo(VostokMultiHostState newState, Exception error = null)
        {
            HostState = newState;

            onHostStateChanged.Next(newState);

            if (error != null)
                onHostStateChanged.Error(error);
            else if (newState.IsTerminal())
                onHostStateChanged.Complete();
        }
    }
}