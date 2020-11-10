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
using Vostok.Logging.Abstractions;
using Vostok.ZooKeeper.Client.Abstractions;

namespace Vostok.Hosting.MultiHost
{
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
        private readonly VostokMultiHostSettings settings;

        private volatile Task<VostokMultiHostRunResult> workerTask;
        private volatile ILog log;
        private volatile VostokHostingEnvironment commonEnvironment;

        public VostokMultiHost(VostokMultiHostSettings settings, params VostokMultiHostApplicationSettings[] apps)
        {
            this.settings = settings;
            applications = new ConcurrentDictionary<string, VostokMultiHostApplication>();
            shutdownTokenSource = new CancellationTokenSource();

            foreach (var app in apps)
                AddApp(app);
        }

        // TODO: Implement IEnumerable
        // CR(iloktionov): Why doesn't VostokMultiHost implement IEnumerable<IVostokMultiHostApplication>?
        /// <summary>
        /// Returns an enumerable of added <see cref="IVostokMultiHostApplication"/>.
        /// </summary>
        public IEnumerable<IVostokMultiHostApplication> Applications => applications.Values;

        /// <summary>
        /// <para>Initializes itself (if it wasn't initialized before) and runs added apps.</para>
        /// </summary>
        public async Task<VostokMultiHostRunResult> RunAsync()
        {
            // TODO: Throw if failed.
            // CR(iloktionov): Why do we ignore the result of StartInternalAsync here?
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

            // TODO: The same as in run.
            // CR(iloktionov): 1. Useless await?
            // CR(iloktionov): 2. Shouldn't this throw if StartInternalAsync returns a failed result?
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

            // TODO: To Task.Run
            // CR(iloktionov): This may execute callbacks synchronously, so it's best to offload this call with Task.Run.
            shutdownTokenSource.Cancel();

            return resultTask;
        }

        // CR(iloktionov): App --> Application? These names are short enough to avoid abbreviations :)
        // CR(iloktionov): [CanBeNull]?
        /// <summary>
        /// <para>Returns added application by name or returns null if it doesn't exist.</para>
        /// </summary>
        public IVostokMultiHostApplication GetApp(string appName)
            => applications.TryGetValue(appName, out var app) ? app : null;

        /// <summary>
        /// <para>Adds an application and returns created application.</para>
        /// </summary>
        public IVostokMultiHostApplication AddApp(VostokMultiHostApplicationSettings applicationSettings)
        {
            // TODO: Check if cancellation has been called.
            // CR(iloktionov): Should this still work once the multihost has stopped and common environment has been disposed? Or, rather, when the shutdown has been initiated.
            // CR(iloktionov): The same question stands for starting the applications directly.

            // CR(iloktionov): Please include the name in the error message :)
            if (applications.ContainsKey(applicationSettings.ApplicationName))
                throw new ArgumentException("Application with this name has already been added.");

            var updatedSettings = UpdateAppSettings(applicationSettings);

            return applications[applicationSettings.ApplicationName] = 
                new VostokMultiHostApplication(updatedSettings, () => commonEnvironment != null);
        }

        /// <summary>
        /// <para>Removes an application (And stops it if necessary).</para>
        /// </summary>
        public Task<VostokApplicationRunResult> RemoveAppAsync(string appName)
        {
            if (applications.TryRemove(appName, out var app))
                return app.StopAsync();

            // CR(iloktionov): KeyNotFoundException? Also please include the name in the error message :)
            throw new InvalidOperationException("VostokMultiHost doesn't contain application with this name.");
        }

        private Task<VostokMultiHostRunResult> StartInternalAsync()
        {
            // NOTE: Configure thread pool once there if necessary.
            if (settings.ConfigureThreadPool)
                ThreadPoolUtility.Setup(settings.ThreadPoolTuningMultiplier);

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

            // CR(iloktionov): Better log this before they actually get started :)
            log.Info("Starting {ApplicationCount} applications.", appTasks.Length);

            while (appTasks.Any() && !commonEnvironment.ShutdownToken.IsCancellationRequested)
            {
                await Task.WhenAny(Task.WhenAll(appTasks), commonEnvironment.ShutdownTask).ConfigureAwait(false);

                // NOTE: We don't launch added applications. Their start is their owner's responsibility.
                appTasks = applications.Values
                   .Where(x => !x.ApplicationState.IsTerminal())
                   .Select(x => x.WorkerTask)
                   .ToArray();
            }

            var applicationRunResults = await StopInternalAsync().ConfigureAwait(false);
            
            log.Info("Applications have stopped.");

            return DisposeCommonEnvironment() ?? new VostokMultiHostRunResult(VostokMultiHostState.Exited, applicationRunResults);
        }

        private async Task<Dictionary<string, VostokApplicationRunResult>> StopInternalAsync()
        {
            var results = new ConcurrentDictionary<string, VostokApplicationRunResult>();
            
            log.Info("Stopping applications..");

            await Task.WhenAll(Applications.Select(x => StopApplication(x, results))).ConfigureAwait(false);

            return results.ToDictionary(
                x => x.Key,
                y => y.Value);
        }

        private async Task StopApplication(IVostokMultiHostApplication app, ConcurrentDictionary<string, VostokApplicationRunResult> results)
            => results[app.Name] = await app.StopAsync(false).ConfigureAwait(false);

        [CanBeNull]
        private VostokMultiHostRunResult DisposeCommonEnvironment()
        {
            try
            {
                log.Info("Disposing common environment..");

                commonEnvironment.Dispose();
                
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
                commonEnvironment = SetupEnvironment();

                log = commonEnvironment.Log.ForContext<VostokMultiHost>();

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
                ConfigureStaticProviders = settings.ConfigureStaticProviders
            };

            return EnvironmentBuilder.Build(
                builder =>
                {
                    settings.EnvironmentSetup(builder);

                    // TODO: Fill with some trash
                    // CR(iloktionov): Project = Infrastructure would likely cause our Sentry to be overrun with test junk :)
                    builder.SetupApplicationIdentity(
                        identityBuilder => identityBuilder
                           .SetProject("Infrastructure")
                           .SetEnvironment("Local")
                           .SetApplication("VostokMultiHost")
                           .SetInstance("1"));

                    builder.DisableServiceBeacon();

                    builder.SetupSystemMetrics(
                        systemMetricsSettings =>
                        {
                            systemMetricsSettings.EnableGcEventsLogging = false;
                            systemMetricsSettings.EnableGcEventsMetrics = false;
                            systemMetricsSettings.EnableProcessMetricsLogging = false;
                            systemMetricsSettings.EnableProcessMetricsReporting = false;
                            systemMetricsSettings.EnableHostMetricsLogging = false;
                            systemMetricsSettings.EnableHostMetricsReporting = false;
                        });
                },
                environmentFactorySettings
            );
        }

        private VostokMultiHostApplicationSettings UpdateAppSettings(VostokMultiHostApplicationSettings applicationSettings)
        {
            return new VostokMultiHostApplicationSettings(
                applicationSettings.Application,
                applicationSettings.ApplicationName,
                builder =>
                {
                    settings.EnvironmentSetup(builder);

                    // NOTE: Disable logging so users will be forced to setup logging. This was made to avoid complete mess when multiple apps write to the same place.
                    // NOTE: Another reason for this is that we use log from Common Environment to log VostokMultiHost events.
                    builder.SetupLog(
                        logBuilder =>
                        {
                            logBuilder.SetupConsoleLog(consoleLogBuilder => consoleLogBuilder.Disable());
                            logBuilder.SetupFileLog(fileLogBuilder => fileLogBuilder.Disable());
                            logBuilder.SetupHerculesLog(herculesLogBuilder => herculesLogBuilder.Disable());
                        });

                    // TODO: ApplicationName should probably be a union of Application and Instance from ApplicationIdentity.
                    // CR(iloktionov): Why do we use app name as instance name instead of, well, application?

                    // NOTE: We use AppName as default instance name.
                    builder.SetupApplicationIdentity(identityBuilder => identityBuilder.SetInstance(applicationSettings.ApplicationName));

                    applicationSettings.EnvironmentSetup(builder);

                    builder.SetupClusterConfigClient(clientBuilder => clientBuilder.UseInstance(commonEnvironment.ClusterConfigClient));

                    builder.SetupHerculesSink(sinkBuilder => sinkBuilder.UseInstance(commonEnvironment.HerculesSink));

                    if (commonEnvironment.HostExtensions.TryGet<IZooKeeperClient>(out var zooKeeperClient))
                        builder.SetupZooKeeperClient(clientBuilder => clientBuilder.UseInstance(zooKeeperClient));
                }
            );
        }
    }
}