using System;
using System.Collections;
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
    public class VostokMultiHost : IEnumerable<IVostokMultiHostApplication>
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
                AddApplication(app);
        }

        /// <summary>
        /// <para>Initializes itself (if it wasn't initialized before) and runs added apps.</para>
        /// <para>May throw an exception if an error occurs during environment creation.</para>
        /// <para>Does not rethrow exceptions, stores them in result's <see cref="VostokMultiHostRunResult.Error"/> property.</para>
        /// </summary>
        public async Task<VostokMultiHostRunResult> RunAsync()
        {
            if (launchedOnce.TrySetTrue())
                await StartInternalAsync()
                   .ContinueWith(task => task.Result.EnsureSuccess(), TaskContinuationOptions.OnlyOnRanToCompletion)
                   .ConfigureAwait(false);

            Task<VostokMultiHostRunResult> runTask;

            lock (launchGate)
                runTask = workerTask;

            if (runTask == null)
                return new VostokMultiHostRunResult(VostokMultiHostState.Exited);

            return await runTask.ConfigureAwait(false);
        }

        /// <summary>
        /// <para>Initializes itself and launches added apps.</para>
        /// <para>May throw an exception if an error occurs during environment creation.</para>
        /// </summary>
        public Task StartAsync() => !launchedOnce.TrySetTrue()
            ? Task.CompletedTask
            : StartInternalAsync().ContinueWith(task => task.Result.EnsureSuccess());

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

            Task.Run(() => shutdownTokenSource.Cancel());

            return resultTask;
        }

        /// <summary>
        /// <para>Returns added application by name or returns null if it doesn't exist.</para>
        /// </summary>
        [CanBeNull]
        public IVostokMultiHostApplication GetApplication(string appName)
            => applications.TryGetValue(appName, out var app) ? app : null;

        /// <summary>
        /// <para>Adds an application and returns created application.</para>
        /// </summary>
        public IVostokMultiHostApplication AddApplication(VostokMultiHostApplicationSettings applicationSettings)
        {
            if (shutdownTokenSource.IsCancellationRequested)
                throw new InvalidOperationException($"Unable to add application {applicationSettings.ApplicationName} because VostokMultiHost is shutting down.");

            if (applications.ContainsKey(applicationSettings.ApplicationName))
                throw new ArgumentException($"Application {applicationSettings.ApplicationName} has already been added.");

            var updatedSettings = UpdateAppSettings(applicationSettings);

            return applications[applicationSettings.ApplicationName] =
                new VostokMultiHostApplication(updatedSettings, () => commonEnvironment != null);
        }

        /// <summary>
        /// <para>Removes an application (And stops it if necessary).</para>
        /// </summary>
        public Task<VostokApplicationRunResult> RemoveApplicationAsync(string appName)
        {
            if (applications.TryRemove(appName, out var app))
                return app.StopAsync();

            throw new KeyNotFoundException($"VostokMultiHost doesn't contain application {appName}.");
        }

        public IEnumerator<IVostokMultiHostApplication> GetEnumerator() => applications.Values.GetEnumerator();

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
            var appTasks = this
               .Select(x => x.RunAsync())
               .ToArray();

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

            await Task.WhenAll(this.Select(x => StopApplication(x, results))).ConfigureAwait(false);

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

                    builder.SetupApplicationIdentity(
                        identityBuilder => identityBuilder
                           .SetProject("VostokMultiHost")
                           .SetEnvironment("Common")
                           .SetApplication("Anything")
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

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}