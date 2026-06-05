using BenchmarkDotNet.Configs;
using Codebelt.Extensions.BenchmarkDotNet.Console;
using Codebelt.Extensions.Xunit;
using System;
using System.IO;
using Xunit;

namespace Codebelt.Extensions.BenchmarkDotNet;

/// <summary>
/// Functional tests for <see cref="BenchmarkProgram"/> non-generic overloads and end-to-end host lifecycle
/// using the real <see cref="BenchmarkWorkspace"/> against the existing tuning assemblies.
/// </summary>
public class BenchmarkProgramTest : Test
{
    // BenchmarkProgram.IsDebugBuild reflects whether the entry assembly was compiled in Debug mode,
    // which is what BenchmarkWorkspace uses to locate tuning assemblies under bin/Debug or bin/Release.
    private static readonly bool IsDebugBuild = BenchmarkProgram.IsDebugBuild;

    public BenchmarkProgramTest(ITestOutputHelper output) : base(output)
    {
    }
    
    /// <summary>
    /// Exercises the non-generic <c>Run(string[], Action&lt;BenchmarkWorkspaceOptions&gt;)</c> overload (line 54)
    /// and the non-generic <c>Run(string[], Action&lt;IServiceCollection&gt;, Action&lt;BenchmarkWorkspaceOptions&gt;)</c>
    /// overload (line 68) which both ultimately delegate to
    /// <c>Run&lt;BenchmarkWorkspace&gt;(string[], Action&lt;IServiceCollection&gt;, Action&lt;BenchmarkWorkspaceOptions&gt;)</c>.
    ///
    /// Using "--list flat" makes BenchmarkSwitcher enumerate available benchmarks and return immediately
    /// without executing any, which keeps the test fast and deterministic.
    /// </summary>
    [Fact]
    public void Run_NonGeneric_WithListFlatArg_ShouldCompleteWithoutException()
    {
        var exception = Record.Exception(() =>
            BenchmarkProgram.Run(
                new[] { "--list", "flat" },
                options =>
                {
                    options.AllowDebugBuild = IsDebugBuild;
                    options.Configuration = ManualConfig.CreateEmpty()
                        .WithOptions(ConfigOptions.DisableLogFile | ConfigOptions.DisableOptimizationsValidator);
                }));

        Assert.Null(exception);
        TestOutput.WriteLine("Non-generic BenchmarkProgram.Run completed via '--list flat'.");
    }

    /// <summary>
    /// Exercises the non-generic <c>Run(string[], Action&lt;IServiceCollection&gt;, Action&lt;BenchmarkWorkspaceOptions&gt;)</c>
    /// overload with an explicit <paramref name="serviceConfigurator"/> to ensure the three-argument
    /// non-generic overload (line 68) is reached.
    /// </summary>
    [Fact]
    public void Run_NonGenericWithServiceConfigurator_WithListFlatArg_ShouldCompleteWithoutException()
    {
        var configuratorCalled = false;

        var exception = Record.Exception(() =>
            BenchmarkProgram.Run(
                new[] { "--list", "flat" },
                services => { configuratorCalled = true; },
                options =>
                {
                    options.AllowDebugBuild = IsDebugBuild;
                    options.Configuration = ManualConfig.CreateEmpty()
                        .WithOptions(ConfigOptions.DisableLogFile | ConfigOptions.DisableOptimizationsValidator);
                }));

        Assert.Null(exception);
        Assert.True(configuratorCalled);
        TestOutput.WriteLine("Non-generic BenchmarkProgram.Run with service configurator completed.");
    }

    /// <summary>
    /// Verifies that <see cref="BenchmarkWorkspace.LoadBenchmarkAssemblies"/> loads the tuning assemblies
    /// built for the current configuration and TFM, and that the assembly resolver hook is registered.
    /// Running this test with the real workspace may cause the <see cref="AppDomain.AssemblyResolve"/>
    /// event to fire for transitive dependencies discovered only through the tuning-folder DLL scan.
    /// </summary>
    [Fact]
    public void BenchmarkWorkspace_LoadBenchmarkAssemblies_ShouldLoadTuningAssemblies()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions
        {
            AllowDebugBuild = IsDebugBuild,
            TargetFrameworkMoniker = "net10.0"
        };
        var workspace = new BenchmarkWorkspace(options);

        // Act
        var assemblies = workspace.LoadBenchmarkAssemblies();

        // Assert
        Assert.NotNull(assemblies);
        Assert.NotEmpty(assemblies);

        foreach (var assembly in assemblies)
        {
            TestOutput.WriteLine($"Loaded: {assembly.GetName().Name}");
        }
    }

    /// <summary>
    /// Verifies that <see cref="BenchmarkWorkspace.PostProcessArtifacts"/> behaves correctly when the
    /// results directory already has files and then calls delete. This exercises the full cleanup path.
    /// </summary>
    [Fact]
    public void BenchmarkWorkspace_PostProcessArtifacts_ShouldMoveFilesAndDeleteResultsDir()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            Directory.CreateDirectory(tempPath);
            var artifactsPath = Path.Combine(tempPath, "artifacts");
            var resultsDir = Path.Combine(artifactsPath, "results");
            var tuningDir = Path.Combine(artifactsPath, "tuning");
            Directory.CreateDirectory(resultsDir);

            File.WriteAllText(Path.Combine(resultsDir, "SomeBenchmark-report.md"), "# report");

            var config = ManualConfig.CreateEmpty().WithArtifactsPath(artifactsPath);
            var options = new BenchmarkWorkspaceOptions
            {
                RepositoryPath = tempPath,
                Configuration = config,
                RepositoryTuningFolder = "tuning"
            };
            var workspace = new BenchmarkWorkspace(options);

            // Act
            workspace.PostProcessArtifacts();

            // Assert
            Assert.False(Directory.Exists(resultsDir));
            Assert.True(File.Exists(Path.Combine(tuningDir, "SomeBenchmark-report.md")));
        }
        finally
        {
            if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true);
        }
    }
}
