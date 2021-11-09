using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
        private readonly ConcurrentDictionary<VostokMultiHostApplicationIdentifier, VostokMultiHostApplication> applications;
        private readonly AtomicBoolean launchedOnce = false;
        private readonly object launchGate = new object();
        private readonly TaskCompletionSource<bool> initiateShutdown;
        private readonly VostokMultiHostSettings settings;

        private volatile Task<VostokMultiHostRunResult> workerTask;
        private volatile ILog log;
        private volatile VostokHostingEnvironment commonEnvironment;

        public VostokMultiHost(VostokMultiHostSettings settings, params VostokMultiHostApplicationSettings[] apps)
        {
            this.settings = settings;
            applications = new ConcurrentDictionary<VostokMultiHostApplicationIdentifier, VostokMultiHostApplication>();
            initiateShutdown = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            foreach (var app in apps)
                AddApplication(app);
        }

        /// <summary>
        /// Returns current <see cref="VostokMultiHostState"/>.
        /// </summary>
        public VostokMultiHostState MultiHostState { get; private set; } = VostokMultiHostState.NotInitialized;

        /// <summary>
        /// <para>Initializes itself (if it wasn't initialized before) and runs added apps.</para>
        /// <para>May throw an exception if an error occurs during environment creation.</para>
        /// <para>Does not rethrow exceptions from applications, stores them in result's <see cref="VostokMultiHostRunResult.Error"/> property.</para>
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
        /// <para>Initializes itself and launches added apps.</para>
        /// <para>May throw an exception if an error occurs during environment creation.</para>
        /// </summary>
        public Task StartAsync() => !launchedOnce.TrySetTrue()
            ? Task.CompletedTask
            : StartInternalAsync();

        /// <summary>
        /// <para>Stops all running applications and disposes itself.</para>
        /// </summary>
        public Task<VostokMultiHostRunResult> StopAsync()
        {
            Task<VostokMultiHostRunResult> resultTask;

            lock (launchGate)
                resultTask = workerTask;

            if (resultTask == null)
                return Task.FromResult(ReturnResult(VostokMultiHostState.NotInitialized));

            initiateShutdown.TrySetResult(true);

            return resultTask;
        }

        /// <summary>
        /// <para>Returns added application by name or returns null if it doesn't exist.</para>
        /// </summary>
        [CanBeNull]
        public IVostokMultiHostApplication GetApplication(VostokMultiHostApplicationIdentifier identifier)
            => applications.TryGetValue(identifier, out var app) ? app : null;

        /// <summary>
        /// <para>Adds an application and returns created application.</para>
        /// </summary>
        public IVostokMultiHostApplication AddApplication(VostokMultiHostApplicationSettings applicationSettings)
        {
            if (initiateShutdown.Task.IsCompleted)
                throw new InvalidOperationException($"Unable to add application {applicationSettings.Identifier} because VostokMultiHost is shutting down.");

            if (applications.ContainsKey(applicationSettings.Identifier))
                throw new ArgumentException($"{applicationSettings.Identifier} has already been added.");

            var updatedSettings = UpdateAppSettings(applicationSettings);

            return applications[applicationSettings.Identifier] =
                new VostokMultiHostApplication(
                    updatedSettings,
                    () => commonEnvironment != null && !initiateShutdown.Task.IsCompleted);
        }

        /// <summary>
        /// <para>Removes an application (And stops it if necessary).</para>
        /// </summary>
        public Task<VostokApplicationRunResult> RemoveApplicationAsync(VostokMultiHostApplicationIdentifier identifier)
        {
            if (applications.TryRemove(identifier, out var app))
                return app.StopAsync();

            throw new KeyNotFoundException($"VostokMultiHost doesn't contain {identifier}.");
        }

        public IEnumerator<IVostokMultiHostApplication> GetEnumerator()
            => applications.Select(pair => pair.Value).GetEnumerator();

        private Task<VostokMultiHostRunResult> StartInternalAsync()
        {
            // NOTE: Configure thread pool once there if necessary.
            if (settings.ConfigureThreadPool)
                ThreadPoolUtility.Setup(settings.ThreadPoolTuningMultiplier);

            BuildCommonEnvironment()?.EnsureSuccess();

            StartAddedApplications();

            return Task.FromResult(ReturnResult(VostokMultiHostState.Running));
        }

        private void StartAddedApplications()
        {
            log.Info("Starting {ApplicationCount} applications.", applications.Count);

            foreach (var application in applications.Select(x => x.Value))
                application.InternalStartApplication();

            lock (launchGate)
                workerTask = Task.Run(RunAddedApplications);
        }

        private async Task<VostokMultiHostRunResult> RunAddedApplications()
        {
            IEnumerable<Task<VostokApplicationRunResult>> GetLaunchedApplications(IEnumerable<Task<VostokApplicationRunResult>> apps) =>
                apps.Where(x => x != null);

            var appTasks = applications
                .Select(x => x.Value.WorkerTask)
                .ToArray();

            var isInitialized = false;

            while ((appTasks.Any() || !isInitialized) && !initiateShutdown.Task.IsCompleted)
            {
                await Task.WhenAll(
                        Task.WhenAny(Task.WhenAll(GetLaunchedApplications(appTasks)), initiateShutdown.Task),
                        Task.Delay(20)
                    )
                    .ConfigureAwait(false);

                // NOTE: MultiHost becomes initialized when it launches at least one app.
                if (!isInitialized && GetLaunchedApplications(appTasks).Any())
                    isInitialized = true;

                // NOTE: We don't launch added applications. Their start is their owner's responsibility.
                appTasks = applications
                    .Select(x => x.Value)
                    .Where(x => !x.ApplicationState.IsTerminal())
                    .Select(x => x.WorkerTask)
                    .ToArray();
            }

            var applicationRunResults = await StopInternalAsync().ConfigureAwait(false);

            log.Info("Applications have stopped.");

            return DisposeCommonEnvironment() ?? ReturnResult(VostokMultiHostState.Exited, applicationRunResults);
        }

        private async Task<Dictionary<VostokMultiHostApplicationIdentifier, VostokApplicationRunResult>> StopInternalAsync()
        {
            var results = new ConcurrentDictionary<VostokMultiHostApplicationIdentifier, VostokApplicationRunResult>();

            log.Info("Stopping applications..");

            await Task.WhenAll(this.Select(x => StopApplication(x, results))).ConfigureAwait(false);

            return results.ToDictionary(
                x => x.Key,
                y => y.Value);
        }

        private async Task StopApplication(IVostokMultiHostApplication app, ConcurrentDictionary<VostokMultiHostApplicationIdentifier, VostokApplicationRunResult> results)
            => results[app.Identifier] = await app.StopAsync(false).ConfigureAwait(false);

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
                return ReturnResult(VostokMultiHostState.CrashedDuringStopping, error);
            }
        }

        [CanBeNull]
        private VostokMultiHostRunResult BuildCommonEnvironment()
        {
            try
            {
                commonEnvironment = SetupEnvironment();

                log = commonEnvironment.Log;

                return null;
            }
            catch (Exception error)
            {
                return ReturnResult(VostokMultiHostState.CrashedDuringEnvironmentSetup, error);
            }
        }

        private VostokHostingEnvironment SetupEnvironment()
        {
            var environmentFactorySettings = new VostokHostingEnvironmentFactorySettings
            {
                ConfigureStaticProviders = settings.ConfigureStaticProviders,
                DiagnosticMetricsEnabled = settings.DiagnosticMetricsEnabled
            };

            return EnvironmentBuilder.Build(
                builder =>
                {
                    builder.SetupApplicationIdentity(
                        identityBuilder => identityBuilder
                            .SetProject("VostokMultiHost")
                            .SetEnvironment("VostokMultiHost")
                            .SetApplication("VostokMultiHost")
                            .SetInstance("VostokMultiHost"));

                    settings.EnvironmentSetup(builder);

                    builder.SetupLog(logBuilder => logBuilder.CustomizeLog(toCustomize => toCustomize.ForContext<VostokMultiHost>()));

                    builder.DisableServiceBeacon();

                    builder.SetupLog(logBuilder => logBuilder.SetupHerculesLog(herculesLogBuilder => herculesLogBuilder.Disable()));

                    builder.SetupMetrics(metricsBuilder => metricsBuilder.SetupHerculesMetricEventSender(senderBuilder => senderBuilder.Disable()));

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
                applicationSettings.Identifier,
                builder =>
                {
                    settings.EnvironmentSetup(builder);

                    builder.SetupLog(
                        logBuilder =>
                        {
                            logBuilder.CustomizeLog(
                                logCustomization => logCustomization.ForContext($"{applicationSettings.Identifier.ApplicationName}-{applicationSettings.Identifier.InstanceName}"));
                        });

                    builder.SetupApplicationIdentity(
                        identityBuilder =>
                        {
                            identityBuilder
                                .SetApplication(applicationSettings.Identifier.ApplicationName)
                                .SetInstance(applicationSettings.Identifier.InstanceName);
                        });

                    applicationSettings.EnvironmentSetup(builder);

                    builder.SetupClusterConfigClient(clientBuilder => clientBuilder.UseInstance(commonEnvironment.ClusterConfigClient));

                    builder.SetupHerculesSink(sinkBuilder => sinkBuilder.UseInstance(commonEnvironment.HerculesSink));

                    if (commonEnvironment.HostExtensions.TryGet<IZooKeeperClient>(out var zooKeeperClient))
                        builder.SetupZooKeeperClient(clientBuilder => clientBuilder.UseInstance(zooKeeperClient));
                }
            );
        }

        private VostokMultiHostRunResult ReturnResult(VostokMultiHostState newState)
        {
            MultiHostState = newState;

            return new VostokMultiHostRunResult(newState);
        }

        private VostokMultiHostRunResult ReturnResult(VostokMultiHostState newState, Exception error)
        {
            MultiHostState = newState;

            return new VostokMultiHostRunResult(newState, error);
        }

        private VostokMultiHostRunResult ReturnResult(
            VostokMultiHostState newState,
            Dictionary<VostokMultiHostApplicationIdentifier, VostokApplicationRunResult> runResults)
        {
            MultiHostState = newState;

            return new VostokMultiHostRunResult(newState, runResults);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}