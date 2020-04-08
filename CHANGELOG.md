## 0.1.14 (08-04-2020):

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