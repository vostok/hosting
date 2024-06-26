## 0.3.70 (25-0-2024):

Fixed `WithSigtermCancellation` to use `PosixSignalRegistration` for properly stop application on `SIGTERM` with .net 6.

## 0.3.68 (02-05-2024):

Updated depenedencies.

## 0.3.66 (28-03-2024):

Updated depenedencies.

## 0.3.65 (06-12-2023):

Fixed tests with `DevNullZooKeeperClient`.

## 0.3.64 (05-12-2023):

Fixed `IZooKeeperClient` null reference error during `VostokHostingEnvironment` building.

## 0.3.63 (19-11-2023):

Added default value for `HostMetricsReportingPeriod` setting.

## 0.3.62 (28-03-2023):

Added `UseActivitySourceTracer` setting and `ActivitySourceTracer` setup.

## 0.3.61 (22-02-2023):

Reverted made `IVostokApplicationIdentityExtensions.FormatServiceName` public.

## 0.3.60 (22-02-2023):

Made `IVostokApplicationIdentityExtensions.FormatServiceName` public.

## 0.3.59 (09-01-2023):

Added `SetRandomFreePort` environment builder extensions.

## 0.3.58 (30-12-2022):

Added `SetupSourceFor` configuration builder extensions.

## 0.3.57 (12-12-2022):

Made some internals parts public for use in `hosting.aspnetcore`.

## 0.3.56 (05-12-2022):

Added `ConfigureStaticProvider` callbacks for `IServiceLocator` and `IZooKeeperClient`.

## 0.3.55 (29-11-2022):

- Added `CustomizeAnnotationEventSender` method to `IVostokMetricsBuilder`
- Added `EnrichInstanceAnnotationTags` extension method to `IVostokMetricsBuilderExtensions`. It allows to enrich tags
  for annotations are written by `Instance` metrics context.

## 0.3.54 (06-10-2022):

HostExtensions : IVostokHostExtensionsForKeyed

## 0.3.53 (05-10-2022):

Filled hercules client tracing annotations.

## 0.3.52 (03-10-2022):

Fixed [#88](https://github.com/vostok/hosting/issues/88) - Don't automatically enable a manually disabled components

## 0.3.51 (27-09-2022):

Added `Enable` and `IsEnabled` method to `IVostokFileLogBuilder`, `IVostokConsoleLogBuilder`
, `IVostokHerculesLogBuilder`.

## 0.3.50 (05-09-2022):

Fixed small bug with `EnsureSuccess` in `WithSigtermCancellation`.

## 0.3.49 (26-06-2022):

Moved health checks launch to after warmup.

## 0.3.48 (23-06-2022):

Added configuration health check.

## 0.3.47 (07-04-2022):

Added `ServiceDiscoveryEventsContextProvider` configure.

## 0.3.46 (07-04-2022):

- Added `CustomizeOutputTemplate` extension that customizes file and console log templates. Use it with conduction
  of `WithPropertyAfter` extension for `OutputTemplate`.
- Log dotnet environment variables on host start.

## 0.3.45 (03-03-2022):

Append host name from `EnvironmentInfo.Host` to LogProperties.

## 0.3.44 (01-03-2022):

Bump ClusterConfig dependency version

## 0.3.43 (14-02-2022):

Bump ClusterConfig dependency version

## 0.3.42 (07-02-2022):

- Combine configuration sources in correct order
- Added `CustomizeMergedSettingsMerging` and `CustomizeMergedConfigurationSource` methods for setting
  up `МergedConfigurationSource`
- Added `МergedConfigurationSource` to `IVostokConfigurationContext`, `IVostokHostingEnvironmentSetupContext`
- Added `МergedConfigurationSource` to host extensions (with `MergedConfigurationSource` key)

## 0.3.41 (26-01-2022):

Added `AddRule` methods for Hercules/File/Console log builders.

## 0.3.40 (26-01-2022):

Set up reporting period for GC monitor and DNS monitor.

## 0.3.39 (11-01-2022):

- Added `IServiceDiscoveryEventsContext` setting
- Added sending `ServiceBeacon` events

## 0.3.38 (21-12-2021):

Fixed misuse of structured log.

## 0.3.37 (16-12-2021):

Fixed lambda closure in `BuildContext` related to `HerculesSink`.

## 0.3.36 (10-12-2021):

Log secret settings updates.

## 0.3.35 (06-12-2021):

Added `net6.0` target.

## 0.3.33 (29-11-2021):

Added `OnApplicationStateChanged` property to `VostokMultiHostApplication`.

## 0.3.32 (16-11-2021):

Fixed [#61](https://github.com/vostok/hosting/issues/61) again.

## 0.3.31 (09-11-2021):

- Fixed [#64](https://github.com/vostok/hosting/issues/64)
- Fixed [#61](https://github.com/vostok/hosting/issues/61)

## 0.3.30 (14-10-2021):

- Added DNS health check
- Now scraping log and hercules metrics on dispose. (Revert of 0.3.22)

## 0.3.29 (14-10-2021):

- Fixed a couple of small dispose leaks
- Enabled DNS latency metric scraping

## 0.3.28 (12-10-2021):

Update dependencies.

## 0.3.27 (01-10-2021):

- Added `DiagnosticMetricsEnabled` settings
- Fixed #60

## 0.3.26 (16-09-2021):

Added possibility to target setup of host metrics.

## 0.3.25 (15-09-2021):

Made DevNull implementations public for testing purposes.

## 0.3.24 (13-09-2021):

Added `IDatacenters` to setup context.

## 0.3.23 (31-08-2021):

Fixed possible deadlock when using configuration for logging setup.

## 0.3.22 (26-08-2021):

Revert log scraping

## 0.3.21 (26-08-2021):

Now scraping log and hercules metrics on dispose.

## 0.3.20 (19-08-2021):

Added public name constants for diagnostic components names (health checks and info providers).

## 0.3.19 (18-08-2021):

Enabled ClusterConfig json remote settings merge with local by default.

## 0.3.18 (22-07-2021):

Updated default system metrics scraping period

## 0.3.17 (28-05-2021):

Cast host name to lower for metrics paths.

## 0.3.16 (14-04-2021):

Added `IVostokHostShutdown` interface to HostExtensions.

## 0.3.15 (30-03-2021):

Added `CustomizeTracer` method to `IVostokTracerBuilder`

## 0.3.13 (09-03-2021):

Fixed https://github.com/vostok/hosting/issues/41

## 0.3.12 (08-03-2021):

Added `Dynamic thread pool` feature.

## 0.3.11 (03-03-2021):

IVostokConfigurationBuilder now allows to obtain intermediate configuration models between adding sources. See methods
with `GetIntermediate` prefix.

## 0.3.10 (02-03-2021):

Fixed https://github.com/vostok/hosting/issues/46

## 0.3.9 (28-02-2021):

Added `SetupMinimumLevelProvider` method to log builders.

## 0.3.8 (01-02-2021):

Added `ProcessMetricsReportingPeriod` and `HostMetricsReportingPeriod` system metrics setting.

## 0.3.7 (22-12-2020):

Added `Enable` method to components builders.

## 0.3.6 (04-12-2020):

VostokHostSettings: added settings to make sure the service beacon has successfully registered.
Moved beacon start to initialization.

## 0.3.5 (02-12-2020):

Added async suffix to `VostokMultiHostExtensions`.

## 0.3.4 (01-12-2020):

Changed child log behaviour in `VostokMultiHost`.

## 0.3.3 (01-12-2020):

Fixed bug with log level metrics.

## 0.3.2 (27-11-2020):

Setup sources for `RequiresMergedConfiguration`.

## 0.3.1 (26-11-2020):

IZooKeeperCientBuilder: setup ZooKeeperClient logins and passwords

## 0.3.0 (25-11-2020):

Added `VostokMultiHost`.

## 0.2.13 (13-11-2020):

Fixed configured loggers log timestamp and context.

## 0.2.12 (30-10-2020):

- Do not dispose external components.
- Not log dispose on non-disposable components.

## 0.2.11 (23-10-2020):

Implemented https://github.com/vostok/hosting/issues/32.

## 0.2.10 (13-10-2020):

Added annotations for app lifecycle events: launching, initialized, stopping.

## 0.2.8 (12-10-2020):

Support for annotation event senders.

## 0.2.7 (05-10-2020):

- Enabled reporting of process metrics by default;
- Added limit-based process system metrics;
- Added optional logging/reporting of host system metrics;

## 0.2.6 (28-09-2020):

IVostokHostingEnvironmentBuilder: added an overload of SetupSystemMetrics with IVostokHostingEnvironment parameter.

## 0.2.5 (24-09-2020):

- Added log level metrics (Amount of log events by level).
- Added health check metrics.

## 0.2.4 (21-09-2020):

- Added Sigterm handling via `WithSigtermCancellation` extension method.

## 0.2.3 (17-09-2020):

- Added `logsDirectory` property to `ServiceBeacon`.

## 0.2.2 (14-09-2020):

- Do not call `Console.Flush()` when `ConsoleLog` hasn't actually been configured.

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
- Added `IsEnabled` property to all builders that are capable of being disabled so that one can apply post-tuning only
  if respective components are enabled.
- Added getters for identity components so that one can set missing values without risking to overwrite previous
  configuration.
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
- VostokApplicationState received four new values: EnvironmentSetup, EnvironmentWarmup, CrashedDuringEnvironmentSetup,
  CrashedDuringEnvironmentWarmup.

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
