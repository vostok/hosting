using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Commons.Helpers.Observable;
using Vostok.Commons.Threading;
using Vostok.Configuration.Abstractions.Extensions.Observable;
using Vostok.Hosting.Components;
using Vostok.Hosting.Components.Environment;
using Vostok.Hosting.Models;

namespace Vostok.Hosting.VostokMultiHost
{
    public class VostokMultiHost
    {
        private readonly ConcurrentDictionary<string, IVostokMultiHostApplication> applications;
        private readonly ConcurrentDictionary<string, IVostokMultiHostApplication> runningApplications;
        private readonly AtomicBoolean launchedOnce = false;
        private readonly CachingObservable<VostokMultiHostState> onHostStateChanged;
        private readonly CachingObservable<int> onRunningApplicationsCountChanged;

        public VostokMultiHost(VostokMultiHostSettings settings, params VostokApplicationSettings[] apps)
        {
            this.settings = settings;
            applications = new ConcurrentDictionary<string, IVostokMultiHostApplication>();
            runningApplications = new ConcurrentDictionary<string, IVostokMultiHostApplication>();

            foreach (var app in apps)
                AddApp(app);

            onHostStateChanged = new CachingObservable<VostokMultiHostState>();
            onRunningApplicationsCountChanged = new CachingObservable<int>();
        }

        public VostokMultiHostState HostState { get; private set; }

        public IObservable<VostokMultiHostState> OnHostStateChanged => onHostStateChanged;

        // Get added applications. 
        public IEnumerable<(string appName, IVostokMultiHostApplication app)> Applications => applications.Select(x => (x.Key, x.Value));

        // Initialize environment, run ALL added applications, return when apps have stopped. You can't run twice.
        public Task<VostokMultiHostRunResult> RunAsync()
        {
            if (!launchedOnce.TrySetTrue())
                throw new InvalidOperationException("VostokMultiHost can't be launched multiple times.");

            return Task.Run(() => RunInternalAsync());
        }

        // Returns after environment initialization and configuration. You can't start twice (even after being stopped).
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

        // Stop all applications and dispose yourself.
        public async Task<VostokMultiHostRunResult> StopAsync()
        {
            if (HostState != VostokMultiHostState.Running)
                throw new InvalidOperationException("VostokMultiHost can't be stopped in a non-running state.");

            var appDict = await StopInternalAsync();

            return DisposeCommonEnvironment() ?? ReturnResult(VostokMultiHostState.Stopped, appDict);
        }

        // Get app or null.
        public IVostokMultiHostApplication GetApp(string appName) => applications.TryGetValue(appName, out var app) ? app : null;

        public IVostokMultiHostApplication AddApp(VostokApplicationSettings vostokApplicationSettings)
        {
            if (applications.ContainsKey(vostokApplicationSettings.ApplicationName))
                throw new ArgumentException("Application with this name has already been added.");

            return applications[vostokApplicationSettings.ApplicationName] = new VostokMultiHostApplication(vostokApplicationSettings, this);
        }

        public async Task<VostokApplicationRunResult> RemoveAppAsync(string appName)
        {
            if (applications.TryRemove(appName, out var app))
                return await app.StopAsync();
            throw new InvalidOperationException("VostokMultiHost doesn't contain application with this name.");
        }

        protected VostokMultiHostSettings settings { get; set; }
        internal CommonBuildContext CommonContext { get; set; }

        internal void AddRunningApp(string appName, IVostokMultiHostApplication app)
        {
            if(runningApplications.ContainsKey(appName))
                throw new ArgumentException("Application with this name has already been added.");
            
            runningApplications[appName] = app;
            onRunningApplicationsCountChanged.Next(runningApplications.Count);
        }

        internal void RemoveRunningApp(string appName)
        {
            if (!applications.TryRemove(appName, out var app))
                throw new InvalidOperationException("VostokMultiHost doesn't contain application with this name.");

            onRunningApplicationsCountChanged.Next(runningApplications.Count);
        }
        
        // TODO: Also I should add RunSequentially and RunInParallel extension methods.
        private async Task<VostokMultiHostRunResult> RunInternalAsync(bool stopAndDispose = true)
        {
            var contextBuildResult = BuildCommonContext();

            if (contextBuildResult != null)
                return contextBuildResult;

            ChangeStateTo(VostokMultiHostState.Running);

            if (applications.Count > 0)
            {
                await Task.WhenAll(applications.Select(x => x.Value.StartAsync()));

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
                        async () => resultDict[x.appName] = await x.app.StopAsync()
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
                CommonContext.DisposeCommonComponents();
            }
            catch (Exception error)
            {
                return ReturnResult(VostokMultiHostState.CrashedDuringStopping, error);
            }

            return null;
        }

        [CanBeNull]
        private VostokMultiHostRunResult BuildCommonContext()
        {
            ChangeStateTo(VostokMultiHostState.EnvironmentSetup);

            try
            {
                var environmentFactorySettings = new VostokHostingEnvironmentFactorySettings
                {
                    ConfigureStaticProviders = settings.ConfigureStaticProviders,
                    BeaconShutdownTimeout = settings.BeaconShutdownTimeout,
                    BeaconShutdownWaitEnabled = settings.BeaconShutdownWaitEnabled
                };

                CommonContext = EnvironmentBuilder.BuildCommonContext(settings.EnvironmentSetup, environmentFactorySettings);
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