## 0.2.2 (14-09-2020):

- Fix `Console.Flush()` bug when `ConsoleLog` hasn't actually been configured. 

## 0.2.1 (09-07-2020):

- New host extensions: GarbageCollectionMonitor (only on .NET Core 3.0+), CurrentProcessMonitor.
- Garbage collection logging enabled by default (for collections longer than 500 ms).
- Garbage collection metrics enabled by default (total duration, longest duration).
- Current process system metrics logging enabled default (CPU, memory, GC, threadpool, ...).
- Current process system metrics reporting can be enabled manually.

## 0.2.0 (27-06-2020):

- Implemented diagnostic info providers
- Implemented health checks
- Now passing app instance instead of type to requirement checker/detector

## 0.1.27 (16-06-2020)

- Fill `MetricContextProvider`.
- Fill `ConfigurationProviderSettings.InitialIndent` setting.

## 0.1.26 (16-06-2020)

Fixed https://github.com/vostok/hosting/issues/10

## 0.1.25 (28-05-2020):

- SubstitutableLog is public now.
- SubstitutableLog: added IsBuffering property.
- IVostokHostExtensionsBuilder: added non-generic overloads with type to Add method.

## 0.1.23 (14-05-2020):

VostokHost now picks up shutdown timeout specified via environment builder.

## 0.1.22 (12-05-2020):

Fixed config placeholder substitutions.

## 0.1.21 (05-05-2020):

- Added extensions on `IVostokHostingEnvironmentBuilder` to quickly setup env/app for ServiceBeacon.
- Added an option to provide custom external instances of IClusterConfigClient, IHerculesSink and IZooKeeperClient.
- Added `IsEnabled` property to all builders that are capable of being disabled so that one can apply post-tuning only if respective components are enabled.
- Added getters for identity components so that one can set missing values without risking to overwrite previous configuration.
- Added default config placeholder substitutions (identity values, SD env and app, CC zone, local datacenter). 
- It's now possible to customize config sources with arbitrary wrappers.

## 0.1.20 (27-04-2020):

Fix `ClusterClientDefaults.ClientApplicationName` filling.

## 0.1.19 (24-04-2020):

Log application dispose.

## 0.1.18 (17-04-2020):

VostokHost received two new methods for lifecycle management: Start and Stop. These are useful for testing.

## 0.1.17 (15-04-2020):

- VostokHost is now suitable to serve as a base class for other hosts.
- VostokHostSettings: increased default shutdown timeout to 10 seconds.
- VostokApplicationState received four new values: EnvironmentSetup, EnvironmentWarmup, CrashedDuringEnvironmentSetup, CrashedDuringEnvironmentWarmup.

## 0.1.16 (12-04-2020)

Environment variable names are public now.

## 0.1.15 (08-04-2020):

- Statically configured parts of application identity can now be used during configuration setup.
- Identity members can now be specified via environment variables.
- Clear messages for errors related to incomplete identities.

## 0.1.13 (08-04-2020):

AddAppSettingsJson extension now recognizes DOTNET_ENVIRONMENT env variable.

## 0.1.12 (07-08-2020):

Update lz4 and hercules.client.

## 0.1.11 (07-04-2020):

Local hostname can be configured using 'VOSTOK_LOCAL_HOSTNAME' environment variable.

## 0.1.10 (04-04-2020):

VostokHost: log environment info before startup.

## 0.1.9 (03-04-2020):

Log datacenters, init them before ApplicationIdentity.

## 0.1.8 (13-03-2020):

Fix lazy setup for configuration.

## 0.1.7 (08-03-2020):

Added extensions to quickly set ServiceBeacon's url scheme.

## 0.1.6 (15-02-2020):

- Implemented https://github.com/vostok/hosting/issues/3.
- Now warming up configuration and ZK connection before app initialization by default.
- ThreadPool tuning multiplier is now configurable.
- Additional environment configuration extensions (DisableZooKeeper, EnableClusterConfig).

## 0.1.5 (04-02-2020):

Minor fixes.

## 0.1.4 (03-02-2020):

- Added an option to configure HerculesSink topology from an external URL.
- Fixed backward compatibility for chained calls in IVostokConfigurationBuilder.

## 0.1.3 (03-02-2020):

- Added configuration extensions to use settings from common sources.
- Added support for configuration sources nesting.
- Added support for log filtering and enrichment rules configured from hot settings.
- Added an option to fully disable ClusterConfig and Hercules.
- Added a log-based metric event sender for debugging.
- It's now possible to specify a port with disabled ServiceBeacon.
- VostokHost now handles OperationCanceledException from applications correctly.
- VostokHost now logs more details about the hosting environment, including app configuration.
- HerculesSink is now disposed before ServiceLocator as it relies on service discovery.
- Default FileLog configuration now includes day-by-day rolling strategy.
- Default log level for ZooKeeper client is now 'Warn'.

## 0.1.2 (28-01-2020):

Update ServiceDiscovery nuget dependencies.

## 0.1.1 (27-01-2020):

Fill `ClusterClientDefaults.ClientApplicationName` if `ConfigureStaticProviders` specified.

## 0.1.0 (16-01-2020): 

Initial prerelease.