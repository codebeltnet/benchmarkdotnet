using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using System;
using System.IO;
using System.Reflection;
using Cuemon;
using Cuemon.Reflection;

namespace Codebelt.Extensions.BenchmarkDotNet;

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class BenchmarkWorkspaceBenchmark
{
    private BenchmarkWorkspace _workspace;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var isDebugBuild = Decorator.Enclose(GetType().Assembly).IsDebugBuild();
        var options = new BenchmarkWorkspaceOptions()
        {
            RepositoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()),
            AllowDebugBuild = isDebugBuild
        };
        _workspace = new BenchmarkWorkspace(options);

        var buildConfig = isDebugBuild
            ? "Debug" 
            : "Release";
        var tuningDir = Path.Combine(options.RepositoryPath, "tuning");
        
        CreateBenchmarkAssembly(tuningDir, buildConfig, "net10.0", "Valid.Benchmarks.dll");
        CreateBenchmarkAssembly(tuningDir, buildConfig, "net9.0", "Valid.Benchmarks.dll");
    }

    [Benchmark(Baseline = true, Description = "Construct BenchmarkDotNetWorkspace")]
    public void ConstructWorkspaceBenchmark()
    {
        var v = new BenchmarkWorkspace(new BenchmarkWorkspaceOptions());
    }

    [Benchmark(Description = "Load assemblies from tuning folder (no matching assemblies)")]
    public void LoadBenchmarkAssembliesBenchmark()
    {
        // In the prepared environment there are no matching *.Benchmarks.dll files,
        // so LoadBenchmarkAssemblies will exercise the enumeration/path logic without loading assemblies.
        var result = _workspace.LoadBenchmarkAssemblies();
        GC.KeepAlive(result);
    }

    [Benchmark(Description = "PostProcessArtifacts (move results -> tuning folder)")]
    public void PostProcessArtifactsBenchmark()
    {
        _workspace.PostProcessArtifacts();
    }

    /// <summary>
    /// Creates a benchmark assembly by copying the executing assembly to a target framework-specific build directory.
    /// </summary>
    /// <param name="tuningDir">The base tuning directory path.</param>
    /// <param name="buildConfig">The build configuration (e.g., "Debug" or "Release").</param>
    /// <param name="targetFramework">The target framework moniker (e.g., "net9.0", "net10.0").</param>
    /// <param name="dllName">The name of the DLL file to create.</param>
    private void CreateBenchmarkAssembly(string tuningDir, string buildConfig, string targetFramework, string dllName)
    {
        var buildDir = Path.Combine(tuningDir, "bin", buildConfig, targetFramework);
        Directory.CreateDirectory(buildDir);
        var targetDll = Path.Combine(buildDir, dllName);
        File.Copy(Assembly.GetExecutingAssembly().Location, targetDll, true);
    }
}
