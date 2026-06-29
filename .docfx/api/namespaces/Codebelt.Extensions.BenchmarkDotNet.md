---
uid: Codebelt.Extensions.BenchmarkDotNet
summary: *content
---
The `Codebelt.Extensions.BenchmarkDotNet` namespace solves the recurring friction of running BenchmarkDotNet from a host application: discovery of the benchmark assemblies, the per-TFM and per-build-configuration filtering that comes with that, and the post-run cleanup of the generated report artifacts. Use it when you want a workspace that scans a `tuning` folder, loads every `*.Benchmarks.dll` for the current `Debug|Release` build and target framework moniker, hands the assemblies to a `BenchmarkRunner` / `BenchmarkSwitcher`, and then moves the per-run output out of the per-run `results` directory and into the long-lived `tuning` directory of your repository.

Start with the default registration: add `AddBenchmarkWorkspace` to an `IServiceCollection`, build a service provider, and resolve `IBenchmarkWorkspace`. That returns the built-in `BenchmarkWorkspace` implementation, which already wires up a sensible `ManualConfig` derived from `BenchmarkWorkspaceOptions.Slim` and BenchmarkDotNet's recommended settings. If you need to extend or replace the configuration — adding a job, attaching a custom exporter, tightening the iteration count — call `ConfigureBenchmarkDotNet` on a `BenchmarkWorkspaceOptions` instance to do it fluently. Reach for `AddBenchmarkWorkspace<TWorkspace>` only when you implement `IBenchmarkWorkspace` yourself to override assembly discovery or post-processing.

The two extension surfaces map directly to the two configuration moments you care about. `BenchmarkWorkspaceOptionsExtensions.ConfigureBenchmarkDotNet` lets you mutate the BenchmarkDotNet `IConfig` on an options instance while the fluent `IConfig` API normally forces a manual reassignment. `ServiceCollectionExtensions.AddBenchmarkWorkspace` and `AddBenchmarkWorkspace<TWorkspace>` register the workspace and the resolved options into the DI container so that any consumer — including a console host — can resolve `IBenchmarkWorkspace` and `BenchmarkWorkspaceOptions` straight from the service provider.

[!INCLUDE [availability-modern](../../includes/availability-modern.md)]

### Extension Members

|Type|Ext|Methods|
|--:|:-:|---|
|BenchmarkWorkspaceOptions|⬇️|`ConfigureBenchmarkDotNet`|
|IServiceCollection|⬇️|`AddBenchmarkWorkspace`, `AddBenchmarkWorkspace<TWorkspace>`|
