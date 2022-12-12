using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Vostok.Commons.Environment;
using Vostok.Commons.Threading;
using Vostok.Configuration.Abstractions.SettingsTree;
using Vostok.Configuration.Extensions;
using Vostok.Configuration.Primitives;
using Vostok.Datacenters;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Components.HostExtensions;
using Vostok.Hosting.Models;
using Vostok.Logging.Abstractions;
using Vostok.ZooKeeper.Client.Abstractions;

namespace Vostok.Hosting.Helpers;

[PublicAPI]
public static class IVostokHostingEnvironmentExtensions
{
    public static void Warmup(this IVostokHostingEnvironment environment, VostokHostingEnvironmentWarmupSettings settings)
    {
        var log = environment.Log.ForContext<VostokHostingEnvironment>();

        log.LogEnvironmentInfo();
        log.LogDotnetEnvironmentVariables(settings);
        log.LogApplicationIdentity(environment.ApplicationIdentity);
        log.LogPort(environment.Port);
        log.LogLocalDatacenter(environment.Datacenters);
        log.LogApplicationLimits(environment.ApplicationLimits);
        log.LogApplicationReplication(environment.ApplicationReplicationInfo);
        log.LogHostExtensions(environment.HostExtensions);
        log.LogThreadPoolSettings();

        environment.WarmupConfiguration(log, settings);
        environment.WarmupZooKeeper(log, settings);
    }

    private static void LogEnvironmentInfo(this ILog log)
    {
        log.Info("Application user = '{User}'.", Environment.UserName);
        log.Info("Application host = '{Host}'.", EnvironmentInfo.Host);
        log.Info("Application host FQDN = '{HostFQDN}'.", EnvironmentInfo.FQDN);
        log.Info("Application process id = '{ProcessId}'.", EnvironmentInfo.ProcessId);
        log.Info("Application process name = '{ProcessName}'.", EnvironmentInfo.ProcessName);
        log.Info("Application base directory = '{BaseDirectory}'.", EnvironmentInfo.BaseDirectory);
        log.Info("Application current directory = '{CurrentDirectory}'.", Environment.CurrentDirectory);
        log.Info("Application OS = '{OperatingSystem}'.", RuntimeInformation.OSDescription);
        log.Info("Application bitness = '{Bitness}'.", Environment.Is64BitProcess ? "x64" : "x86");
        log.Info("Application framework = '{Framework}'.", RuntimeInformation.FrameworkDescription);
        log.Info("Application GC type = '{GCType}'.", GCSettings.IsServerGC ? "Server" : "Workstation");
    }

    private static void LogDotnetEnvironmentVariables(this ILog log, VostokHostingEnvironmentWarmupSettings settings)
    {
        if (!settings.LogDotnetEnvironmentVariables)
            return;

        try
        {
            var dotnetPrefixes = new[] {"DOTNET_", "COMPlus_", "ASPNETCORE_"};
            var dotnetVariables = new Dictionary<string, string>();

            foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
            {
                if (entry.Key is string stringKey &&
                    entry.Value is string stringValue &&
                    dotnetPrefixes.Any(p => stringKey.StartsWith(p, StringComparison.InvariantCultureIgnoreCase)))
                    dotnetVariables[stringKey] = stringValue;
            }

            log.Info("Application dotnet environment variables = '{EnvironmentVariables}'.", dotnetVariables);
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private static void LogApplicationIdentity(this ILog log, IVostokApplicationIdentity applicationIdentity)
    {
        var messageTemplate = applicationIdentity.Subproject == null
            ? "Application identity: project: '{Project}', environment: '{Environment}', application: '{Application}', instance: '{Instance}'."
            : "Application identity: project: '{Project}', subproject: '{Subproject}', environment: '{Environment}', application: '{Application}', instance: '{Instance}'.";

        var messageParameters = applicationIdentity.Subproject == null
            ? new object[] {applicationIdentity.Project, applicationIdentity.Environment, applicationIdentity.Application, applicationIdentity.Instance}
            : new object[] {applicationIdentity.Project, applicationIdentity.Subproject, applicationIdentity.Environment, applicationIdentity.Application, applicationIdentity.Instance};

        log.Info(messageTemplate, messageParameters);
    }

    private static void LogPort(this ILog log, int? port)
    {
        if (port.HasValue)
            log.Info("Application port: {Port}.", port);
    }

    private static void LogLocalDatacenter(this ILog log, IDatacenters datacenters) =>
        log.Info("Application datacenter: {DatacenterName}.", datacenters.GetLocalDatacenter() ?? "unknown");

    private static void LogApplicationLimits(this ILog log, IVostokApplicationLimits limits) =>
        log.Info(
            "Application limits: {CpuLimit} CPU, {MemoryLimit} memory.",
            limits.CpuUnits?.ToString("F2") ?? "unlimited",
            limits.MemoryBytes.HasValue ? new DataSize(limits.MemoryBytes.Value).ToString() : "unlimited");

    private static void LogApplicationReplication(this ILog log, IVostokApplicationReplicationInfo info) =>
        log.Info("Application replication: instance {InstanceIndex} of {InstanceCount}.", info.InstanceIndex, info.InstancesCount);

    private static void LogApplicationConfiguration(this ILog log, ISettingsNode configuration)
    {
        try
        {
            log.Info($"Application configuration: {Environment.NewLine}{{ApplicationConfiguration}}.", configuration);
        }
        catch
        {
            log.Warn("Application configuration is unknown.");
        }
    }

    private static void LogHostExtensions(this ILog log, IVostokHostExtensions extensions)
        => log.Info("Registered host extensions: {HostExtensions}.",
            extensions.GetAll()
                .Select(pair => pair.Item1.Name)
                .Concat(extensions is HostExtensions hostExtensions ? hostExtensions.GetAllKeyed().Select(pair => $"{pair.Item1}({pair.Item2.Name})") : Array.Empty<string>())
                .ToArray());

    private static void LogThreadPoolSettings(this ILog log)
    {
        var state = ThreadPoolUtility.GetPoolState();

        log.Info("Thread pool configuration: {MinWorkerThreads} min workers, {MinIOCPThreads} min IOCP.", state.MinWorkerThreads, state.MinIocpThreads);
    }

    private static void WarmupConfiguration(this IVostokHostingEnvironment environment, ILog log, VostokHostingEnvironmentWarmupSettings settings)
    {
        if (!settings.WarmupConfiguration)
            return;

        log.Info("Warming up application configuration..");

        environment.ClusterConfigClient.Get(Guid.NewGuid().ToString());

        var ordinarySettings = environment.ConfigurationSource.Get();

        environment.SecretConfigurationSource.Get();

        if (settings.LogApplicationConfiguration)
            log.LogApplicationConfiguration(ordinarySettings);
    }

    private static void WarmupZooKeeper(this IVostokHostingEnvironment environment, ILog log, VostokHostingEnvironmentWarmupSettings settings)
    {
        if (!settings.WarmupZooKeeper || !environment.HostExtensions.TryGet<IZooKeeperClient>(out var zooKeeperClient))
            return;

        log.Info("Warming up ZooKeeper connection..");

        zooKeeperClient.Exists("/");
    }
}