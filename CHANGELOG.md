# Changelog

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

For more details, please refer to `PackageReleaseNotes.txt` on a per assembly basis in the `.nuget` folder.

## [1.2.5] - 2026-03-23

This is a patch release focused on keeping package dependencies current.

### Changed

- Upgraded `Codebelt.Bootstrapper.Console` from 5.0.4 to 5.0.5,
- Upgraded `Codebelt.Extensions.Xunit.App` from 11.0.7 to 11.0.8,
- Upgraded `Cuemon.Core` from 10.4.0 to 10.5.0.

## [1.2.4] - 2026-02-28

This is a service update that focuses on package dependencies.

## [1.2.3] - 2026-02-20

This is a service update that focuses on package dependencies.

## [1.2.2] - 2026-02-15

This is a service update that focuses on package dependencies.

## [1.2.1] - 2026-01-20

This is a service update that focuses on package dependencies.

## [1.2.0] - 2025-12-19

Technically, this is a major release due to the removal of BenchmarkWorker class in the Codebelt.Extensions.BenchmarkDotNet.Console namespace. However, since this library is new and the external API remains unchanged, we have decided to label this release as a minor update.

### Removed

- `BenchmarkWorker` class in the Codebelt.Extensions.BenchmarkDotNet.Console namespace has been removed. Its functionality has been merged into the `BenchmarkProgram` class to streamline the hosting and execution of benchmarks.

## [1.1.0] - 2025-12-14

This release introduces several enhancements and fixes to improve the functionality and usability of the `Codebelt.Extensions.BenchmarkDotNet` and `Codebelt.Extensions.BenchmarkDotNet.Console` packages.

### Changed

- `BenchmarkWorkspace` class in the Codebelt.Extensions.BenchmarkDotNet namespace was extended with two new static methods; `GetReportsResultsPath` and `GetReportsTuningPath` for retrieving the paths to the reports results and tuning directories respectively,
- `BenchmarkWorkspaceOptions` class in the Codebelt.Extensions.BenchmarkDotNet namespace was extended with an additional new property; `SkipBenchmarksWithReports` that indicates whether benchmarks that already have generated reports should be skipped during execution,
- `BenchmarkProgram` class in the Codebelt.Extensions.BenchmarkDotNet.Console namespace was extended to support an optional service configurator delegate for customizing the `IServiceCollection` during host building,
- `BenchmarkWorker` class in the Codebelt.Extensions.BenchmarkDotNet.Console namespace was extended to support skipping benchmarks that already have generated reports based on the `BenchmarkWorkspaceOptions.SkipBenchmarksWithReports` property,
- `BenchmarkWorker` class in the Codebelt.Extensions.BenchmarkDotNet.Console namespace was changed to conditionally suppress console status messages in services based on whether you are in a debugging session or not.

### Fixed

- `BenchmarkWorkspaceOptions` class in the Codebelt.Extensions.BenchmarkDotNet namespace was fixed so it no longer relies on Danish culture.

## [1.0.0] - 2025-12-12

This is the initial stable release of the `Codebelt.Extensions.BenchmarkDotNet` and `Codebelt.Extensions.BenchmarkDotNet.Console` packages.

### Added

- ADDED `BenchmarkWorkspace` class in the Codebelt.Extensions.BenchmarkDotNet namespace that provides a default implementation of `IBenchmarkWorkspace` for discovering and handling assemblies and their generated artifacts in BenchmarkDotNet,
- ADDED `BenchmarkWorkspaceOptions` class in the Codebelt.Extensions.BenchmarkDotNet namespace that specifies configuration options that is related to the `BenchmarkWorkspace` class,
- ADDED `BenchmarkWorkspaceOptionsExtensions` class in the Codebelt.Extensions.BenchmarkDotNet namespace that consist of extension methods for the `BenchmarkWorkspaceOptions` class: `ConfigureBenchmarkDotNet`,
- ADDED `IBenchmarkWorkspace` interface in the Codebelt.Extensions.BenchmarkDotNet namespace that defines a way for discovering and handling assemblies and their generated artifacts in BenchmarkDotNet,
- ADDED `ServiceCollectionExtensions` class in the Codebelt.Extensions.BenchmarkDotNet namespace that consist of extension methods for the IServiceCollection interface: `AddBenchmarkWorkspace` and `AddBenchmarkWorkspace{TWorkspace}`,
- ADDED `BenchmarkContext` class in the Codebelt.Extensions.BenchmarkDotNet.Console namespace that represents the command-line context for a benchmark run,
- ADDED `BenchmarkProgram` class in the Codebelt.Extensions.BenchmarkDotNet.Console namespace that provides the main entry point for hosting and running benchmarks using BenchmarkDotNet,
- ADDED `BenchmarkWorker` class in the Codebelt.Extensions.BenchmarkDotNet.Console namespace that is responsible for executing benchmarks within the console host.

[Unreleased]: https://github.com/codebeltnet/benchmarkdotnet/compare/v1.2.5...HEAD
[1.2.5]: https://github.com/codebeltnet/benchmarkdotnet/compare/v1.2.4...v1.2.5
[1.2.4]: https://github.com/codebeltnet/benchmarkdotnet/compare/v1.2.3...v1.2.4
[1.2.3]: https://github.com/codebeltnet/benchmarkdotnet/compare/v1.2.2...v1.2.3
[1.2.2]: https://github.com/codebeltnet/benchmarkdotnet/compare/v1.2.1...v1.2.2
[1.2.1]: https://github.com/codebeltnet/benchmarkdotnet/compare/v1.2.0...v1.2.1
[1.2.0]: https://github.com/codebeltnet/benchmarkdotnet/compare/v1.1.0...v1.2.0
[1.1.0]: https://github.com/codebeltnet/benchmarkdotnet/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/codebeltnet/benchmarkdotnet/releases/tag/v1.0.0
