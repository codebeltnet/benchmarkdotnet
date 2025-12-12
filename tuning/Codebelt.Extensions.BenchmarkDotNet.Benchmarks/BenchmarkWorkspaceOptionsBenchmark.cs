using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using System;

namespace Codebelt.Extensions.BenchmarkDotNet;

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class BenchmarkWorkspaceOptionsBenchmark
{
    private BenchmarkWorkspaceOptions _options;

    [GlobalSetup]
    public void Setup()
    {
        // Pre-allocate an instance for benchmarks that need existing state
        _options = new BenchmarkWorkspaceOptions();
    }

    [Benchmark(Baseline = true, Description = "Create default BenchmarkWorkspaceOptions")]
    [BenchmarkCategory("Construction")]
    public BenchmarkWorkspaceOptions CreateDefaultOptions()
    {
        return new BenchmarkWorkspaceOptions();
    }

    [Benchmark(Description = "Create and configure BenchmarkWorkspaceOptions")]
    [BenchmarkCategory("Construction")]
    public BenchmarkWorkspaceOptions CreateAndConfigureOptions()
    {
        var options = new BenchmarkWorkspaceOptions
        {
            RepositoryPath = AppContext.BaseDirectory,
            RepositoryTuningFolder = "tuning",
            RepositoryReportsFolder = "reports",
            BenchmarkProjectSuffix = "Benchmarks",
            TargetFrameworkMoniker = "net9.0",
            AllowDebugBuild = false
        };
        return options;
    }

    [Benchmark(Description = "ValidateOptions - valid state")]
    [BenchmarkCategory("Validation")]
    public void ValidateOptions_ValidState()
    {
        _options.ValidateOptions();
    }

    [Benchmark(Description = "PostConfigureOptions - default config")]
    [BenchmarkCategory("Configuration")]
    public void PostConfigureOptions_DefaultConfig()
    {
        var options = new BenchmarkWorkspaceOptions();
        options.PostConfigureOptions();
        GC.KeepAlive(options);
    }

    [Benchmark(Description = "PostConfigureOptions - custom config")]
    [BenchmarkCategory("Configuration")]
    public void PostConfigureOptions_CustomConfig()
    {
        var options = new BenchmarkWorkspaceOptions
        {
            Configuration = ManualConfig.CreateEmpty()
        };
        options.PostConfigureOptions();
        GC.KeepAlive(options);
    }

    [Benchmark(Description = "Property access - RepositoryPath")]
    [BenchmarkCategory("PropertyAccess")]
    public string AccessRepositoryPath()
    {
        return _options.RepositoryPath;
    }

    [Benchmark(Description = "Property access - Configuration")]
    [BenchmarkCategory("PropertyAccess")]
    public IConfig AccessConfiguration()
    {
        return _options.Configuration;
    }

    [Benchmark(Description = "Property modification - set all properties")]
    [BenchmarkCategory("PropertyModification")]
    public void SetAllProperties()
    {
        var options = new BenchmarkWorkspaceOptions
        {
            RepositoryPath = AppContext.BaseDirectory,
            RepositoryTuningFolder = "custom-tuning",
            RepositoryReportsFolder = "custom-reports",
            BenchmarkProjectSuffix = "Perf",
            TargetFrameworkMoniker = "net10.0",
            AllowDebugBuild = true
        };
        GC.KeepAlive(options);
    }

    [Benchmark(Description = "Full lifecycle - create, configure, validate")]
    [BenchmarkCategory("Lifecycle")]
    public void FullLifecycle()
    {
        var options = new BenchmarkWorkspaceOptions
        {
            RepositoryPath = AppContext.BaseDirectory,
            TargetFrameworkMoniker = "net9.0"
        };
        options.PostConfigureOptions();
        options.ValidateOptions();
        GC.KeepAlive(options);
    }
}
