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
    // TODO: Problems: 
    // 1) Logging
    // 2) Configuring thread pool
    // 3) Application identity in VostokMultiHost
    // 4) What to disable in VostokMultiHost common environment?
    // 5) How to shutdown in given time? Is it even possible?

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
        /// <para>Initializes itself (if it wasn't initialized before) and runs added apps.</para>
        /// </summary>
        public async Task<VostokMultiHostRunResult> RunAsync()
        {
            if (launchedOnce.TrySetTrue())
                await StartInternalAsync().ConfigureAwait(false);

            Task<VostokMultiHostRunResult> runTask;

            lock (launchGate)
                runTask = workerTask;

            if (runTask == null)
                return new VostokMultiHostRunResult(VostokMultiHostState.Exited);

            return await runTask.ConfigureAwait(false);
        }

        /// <summary>
        /// <para>Initializes itself and launches added apps.</para>
        /// </summary>
        public async Task StartAsync()
        {
            if (!launchedOnce.TrySetTrue())
                return;

            await StartInternalAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// <para>Stops all running applications and disposes itself.</para>
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
        /// <para>Returns added application by name or returns null if it doesn't exist.</para>
        /// </summary>
        public IVostokMultiHostApplication GetApp(string appName) => applications.TryGetValue(appName, out var app) ? app : null;

        /// <summary>
        /// <para>Adds an application and returns created application.</para>
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
                    Settings.EnvironmentSetup(builder);

                    vostokMultiHostApplicationSettings.EnvironmentSetup(builder);

                    builder.SetupClusterConfigClient(clientBuilder => clientBuilder.UseInstance(CommonEnvironment.ClusterConfigClient));

                    builder.SetupHerculesSink(sinkBuilder => sinkBuilder.UseInstance(CommonEnvironment.HerculesSink));

                    if (CommonEnvironment.HostExtensions.TryGet<IZooKeeperClient>(out var zooKeeperClient))
                        builder.SetupZooKeeperClient(clientBuilder => clientBuilder.UseInstance(zooKeeperClient));
                });

            return applications[vostokMultiHostApplicationSettings.ApplicationName] = new VostokMultiHostApplication(updatedSettings);
        }

        /// <summary>
        /// <para>Removes an application (And stops it if necessary).</para>
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
                workerTask = Task.Run(RunAllApplications);

            return Task.FromResult(new VostokMultiHostRunResult(VostokMultiHostState.Running));
        }

        private async Task<VostokMultiHostRunResult> RunAllApplications()
        {
            var appTasks = Applications
               .Select(x => x.RunAsync())
               .ToArray();

            while (appTasks.Any() && !CommonEnvironment.ShutdownToken.IsCancellationRequested)
            {
                await Task.WhenAny(Task.WhenAll(appTasks), CommonEnvironment.ShutdownTask);

                // NOTE: We don't launch added applications. Their start is their owner's responsibility.
                appTasks = applications.Values
                   .Where(x => !x.ApplicationState.IsTerminal())
                   .Select(x => x.WorkerTask)
                   .ToArray();
            }

            var appDict = await StopInternalAsync().ConfigureAwait(false);

            return DisposeCommonEnvironment() ?? new VostokMultiHostRunResult(VostokMultiHostState.Exited, appDict);
        }

        private async Task<Dictionary<string, VostokApplicationRunResult>> StopInternalAsync()
        {
            var resultDict = new ConcurrentDictionary<string, VostokApplicationRunResult>();

            await Task.WhenAll(Applications.Select(x => StopApplication(x, resultDict)));

            return resultDict.ToDictionary(
                x => x.Key,
                y => y.Value);
        }

        private async Task StopApplication(IVostokMultiHostApplication app, ConcurrentDictionary<string, VostokApplicationRunResult> dict)
        {
            dict[app.Name] = await app.StopAsync(false);
        }

        [CanBeNull]
        private VostokMultiHostRunResult DisposeCommonEnvironment()
        {
            try
            {
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
                CommonEnvironment = SetupEnvironment();

                return null;
            }
            catch (Exception error)
            {
                return new VostokMultiHostRunResult(VostokMultiHostState.CrashedDuringEnvironmentSetup, error);
            }
        }

        private VostokHostingEnvironment SetupEnvironment()
        {
            var environmentFactorySettings = new VostokHostingEnvironmentFactorySettings
            {
                ConfigureStaticProviders = Settings.ConfigureStaticProviders
            };

            return EnvironmentBuilder.Build(
                builder =>
                {
                    Settings.EnvironmentSetup(builder);

                    builder.SetupApplicationIdentity(
                        identityBuilder => identityBuilder
                           .SetProject("Infrastructure")
                           .SetEnvironment("Local")
                           .SetApplication("VostokMultiHost")
                           .SetInstance("1"));

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
                },
                environmentFactorySettings);
        }
    }
}