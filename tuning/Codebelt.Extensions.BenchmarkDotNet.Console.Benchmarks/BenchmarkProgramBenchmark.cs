using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Cuemon;
using Cuemon.Reflection;
using System;
using System.Reflection;

namespace Codebelt.Extensions.BenchmarkDotNet.Console;

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class BenchmarkProgramBenchmark
{
    private Assembly _testAssembly;
    private Assembly _entryAssembly;

    [GlobalSetup]
    public void Setup()
    {
        // Deterministic initialization of test data
        _testAssembly = Assembly.GetExecutingAssembly();
        _entryAssembly = Assembly.GetEntryAssembly() ?? _testAssembly;
    }

    [Benchmark(Baseline = true, Description = "Access BuildConfiguration property")]
    public string AccessBuildConfiguration()
    {
        return BenchmarkProgram.BuildConfiguration;
    }

    [Benchmark(Description = "Access IsDebugBuild property")]
    public bool AccessIsDebugBuild()
    {
        return BenchmarkProgram.IsDebugBuild;
    }

    [Benchmark(Description = "Check assembly debug build status")]
    public bool CheckAssemblyDebugBuild()
    {
        return Decorator.Enclose(_testAssembly).IsDebugBuild();
    }

    [Benchmark(Description = "Resolve build configuration from assembly")]
    public string ResolveBuildConfiguration()
    {
        var isDebugBuild = Decorator.Enclose(_testAssembly).IsDebugBuild();
        return isDebugBuild ? "Debug" : "Release";
    }

    [Benchmark(Description = "Check entry assembly debug build status")]
    public bool CheckEntryAssemblyDebugBuild()
    {
        return Decorator.Enclose(_entryAssembly).IsDebugBuild();
    }

    [Benchmark(Description = "Static property access pattern")]
    public (string, bool) AccessStaticProperties()
    {
        return (BenchmarkProgram.BuildConfiguration, BenchmarkProgram.IsDebugBuild);
    }
}
