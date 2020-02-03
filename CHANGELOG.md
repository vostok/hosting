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