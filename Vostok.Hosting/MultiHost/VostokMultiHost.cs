using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Commons.Threading;
using Vostok.Hosting.Components.Environment;
using Vostok.Hosting.Models;
using Vostok.Hosting.Setup;
using Vostok.ZooKeeper.Client.Abstractions;

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
        private readonly ConcurrentDictionary<string, VostokMultiHostApplication> applications;
        private readonly AtomicBoolean launchedOnce = false;
        private readonly object launchGate = new object();
        private readonly CancellationTokenSource shutdownTokenSource;
        private volatile Task<VostokMultiHostRunResult> workerTask;

        public VostokMultiHost(VostokMultiHostSettings settings, params VostokMultiHostApplicationSettings[] apps)
        {
            Settings = settings;
            applications = new ConcurrentDictionary<string, VostokMultiHostApplication>();
            shutdownTokenSource = new CancellationTokenSource();

            foreach (var app in apps)
                AddApp(app);
        }

        /// <summary>
        /// Returns an enumerable of added <see cref="IVostokMultiHostApplication"/>.
        /// </summary>
        public IEnumerable<IVostokMultiHostApplication> Applications => applications.Values;

        /// <summary>
        /// <para>Initializes itself and launches added apps.</para>
        /// </summary>
        public async Task<VostokMultiHostRunResult> RunAsync()
        {
            if (launchedOnce.TrySetTrue())
                await StartInternalAsync().ConfigureAwait(false);

            Task<VostokMultiHostRunResult> runTask;

            lock (launchGate)
                runTask = workerTask;

            return await runTask.ConfigureAwait(false);
        }

        /// <summary>
        /// <para>Initializes itself and launches added apps, but returns just after the initialization. (Fire-and-forget style)</para>
        /// </summary>
        public async Task StartAsync()
        {
            if (!launchedOnce.TrySetTrue())
                return;

            await StartInternalAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// <para>Stops all running applications and dispose itself.</para>
        /// </summary>
        public Task<VostokMultiHostRunResult> StopAsync()
        {
            Task<VostokMultiHostRunResult> resultTask;

            lock (launchGate)
                resultTask = workerTask;

            if (resultTask == null)
                return Task.FromResult(new VostokMultiHostRunResult(VostokMultiHostState.NotInitialized));

            shutdownTokenSource.Cancel();

            return resultTask;
        }

        /// <summary>
        /// <para>Returns added application by name or null if it doesn't exist.</para>
        /// </summary>
        public IVostokMultiHostApplication GetApp(string appName) => applications.TryGetValue(appName, out var app) ? app : null;

        /// <summary>
        /// <para>Adds an application by providing its settings and returns created application.</para>
        /// </summary>
        public IVostokMultiHostApplication AddApp(VostokMultiHostApplicationSettings vostokMultiHostApplicationSettings)
        {
            if (applications.ContainsKey(vostokMultiHostApplicationSettings.ApplicationName))
                throw new ArgumentException("Application with this name has already been added.");

            var updatedSettings = new VostokMultiHostApplicationSettings(
                vostokMultiHostApplicationSettings.Application,
                vostokMultiHostApplicationSettings.ApplicationName,
                builder =>
                {
                    builder.SetupClusterConfigClient(clientBuilder => clientBuilder.UseInstance(CommonEnvironment.ClusterConfigClient));
                    builder.SetupHerculesSink(sinkBuilder => sinkBuilder.UseInstance(CommonEnvironment.HerculesSink));
                    if(CommonEnvironment.HostExtensions.TryGet<IZooKeeperClient>(out var zooKeeperClient))
                        builder.SetupZooKeeperClient(clientBuilder => clientBuilder.UseInstance(zooKeeperClient));
                    Settings.EnvironmentSetup(builder);
                    vostokMultiHostApplicationSettings.EnvironmentSetup(builder);
                });

            return applications[vostokMultiHostApplicationSettings.ApplicationName] = new VostokMultiHostApplication(updatedSettings);
        }

        /// <summary>
        /// <para>Removes an application (stops it if it's necessary).</para>
        /// </summary>
        public Task<VostokApplicationRunResult> RemoveAppAsync(string appName)
        {
            if (applications.TryRemove(appName, out var app))
                return app.StopAsync();

            throw new InvalidOperationException("VostokMultiHost doesn't contain application with this name.");
        }

        private VostokHostingEnvironment CommonEnvironment { get; set; }
        private VostokMultiHostSettings Settings { get; set; }

        private Task<VostokMultiHostRunResult> StartInternalAsync()
        {
            var environmentBuildResult = BuildCommonEnvironment();

            if (environmentBuildResult != null)
                return Task.FromResult(environmentBuildResult);

            lock (launchGate)
                return workerTask = Task.Run(RunAllApplications);
        }

        private async Task<VostokMultiHostRunResult> RunAllApplications()
        {
            // NOTE: Applications can't be launched before VostokMultiHost, so we don't have to filter them.

            var appTasks = Applications.Select(x => x.RunAsync()).ToArray();

            while (appTasks.Any() && !CommonEnvironment.ShutdownToken.IsCancellationRequested)
            {
                await Task.WhenAny(Task.WhenAll(appTasks), CommonEnvironment.ShutdownTask);

                // NOTE: We don't launch added applications. Their start is their owner's responsibility.
                appTasks = applications.Values.Where(x => !x.ApplicationState.IsTerminal()).Select(x => x.workerTask).ToArray();
            }

            var appDict = await StopInternalAsync().ConfigureAwait(false);

            return DisposeCommonEnvironment() ?? new VostokMultiHostRunResult(VostokMultiHostState.Stopped, appDict);
        }

        private async Task<Dictionary<string, VostokApplicationRunResult>> StopInternalAsync()
        {
            var resultDict = new ConcurrentDictionary<string, VostokApplicationRunResult>();

            await Task.WhenAll(
                Applications.Select(
                    x => Task.Run(
                        async () => resultDict[x.Name] = await x.StopAsync(false)
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
                // TODO: Logging?
                CommonEnvironment.Dispose();

                return null;
            }
            catch (Exception error)
            {
                return new VostokMultiHostRunResult(VostokMultiHostState.CrashedDuringStopping, error);
            }
        }

        [CanBeNull]
        private VostokMultiHostRunResult BuildCommonEnvironment()
        {
            try
            {
                var environmentFactorySettings = new VostokHostingEnvironmentFactorySettings
                {
                    ConfigureStaticProviders = Settings.ConfigureStaticProviders,
                    BeaconShutdownTimeout = Settings.BeaconShutdownTimeout,
                    BeaconShutdownWaitEnabled = Settings.BeaconShutdownWaitEnabled
                };

                // TODO: Move to "SetupEnvironment" method.
                CommonEnvironment = EnvironmentBuilder.Build(
                    builder =>
                    {
                        builder.SetupApplicationIdentity(identityBuilder => identityBuilder.SetApplication("test").SetEnvironment("teststs").SetInstance("etstse").SetProject("project"));
                        builder.SetupZooKeeperClient(clientBuilder => {});
                        Settings.EnvironmentSetup(builder);
                        builder.SetupShutdownToken(shutdownTokenSource.Token);
                        builder.DisableServiceBeacon();
                        builder.SetupSystemMetrics(
                            settings =>
                            {
                                settings.EnableGcEventsLogging = false;
                                settings.EnableGcEventsMetrics = false;
                                settings.EnableProcessMetricsLogging = false;
                                settings.EnableProcessMetricsReporting = false;
                                settings.EnableHostMetricsLogging = false;
                                settings.EnableHostMetricsReporting = false;
                            });
                        // TODO: Disable unnecessary things. 
                    },
                    environmentFactorySettings);

                return null;
            }
            catch (Exception error)
            {
                return new VostokMultiHostRunResult(VostokMultiHostState.CrashedDuringEnvironmentSetup, error);
            }
        }
    }
}