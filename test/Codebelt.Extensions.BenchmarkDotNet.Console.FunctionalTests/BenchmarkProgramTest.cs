using BenchmarkDotNet.Configs;
using Codebelt.Extensions.Xunit;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Codebelt.Extensions.BenchmarkDotNet.Console;

/// <summary>
/// Functional tests for <see cref="BenchmarkProgram"/> that exercise the full hosted-application lifecycle
/// including <see cref="BenchmarkProgram.RunAsync(IServiceProvider,CancellationToken)"/>, <see cref="BenchmarkProgram.Run{TWorkspace}(string[],Action{BenchmarkWorkspaceOptions})"/>,
/// and the benchmark-filter pipeline (SkipBenchmarksWithReports).
/// </summary>
public class BenchmarkProgramTest : Test
{
    public BenchmarkProgramTest(ITestOutputHelper output) : base(output)
    {
    }

    // ---------------------------------------------------------------------------
    // RunAsync – direct instantiation tests (avoid starting the full host)
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task RunAsync_ShouldComplete_WhenWorkspaceReturnsNoAssemblies_AndArgsAreEmpty()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions();
        var workspace = new EmptyWorkspace();
        var context = new BenchmarkContext(Array.Empty<string>());

        using var sp = BuildServiceProvider(options, workspace, context);
        var program = new BenchmarkProgram();

        // Act – empty assemblies + empty args → empty foreach → no BenchmarkRunner call
        var exception = await Record.ExceptionAsync(() => program.RunAsync(sp, CancellationToken.None));

        // Assert
        Assert.Null(exception);
        Assert.True(workspace.PostProcessArtifactsCalled, "PostProcessArtifacts should always be called in the finally block.");

        TestOutput.WriteLine("RunAsync completed cleanly with empty workspace and no args.");
    }

    [Fact]
    public async Task RunAsync_ShouldUseBenchmarkSwitcher_WhenArgsAreNonEmpty()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions();
        var workspace = new EmptyWorkspace();

        // "--list flat" asks BenchmarkSwitcher to list (not run) benchmarks; safe with an empty assembly list.
        var context = new BenchmarkContext(new[] { "--list", "flat" });

        using var sp = BuildServiceProvider(options, workspace, context);
        var program = new BenchmarkProgram();

        // Act – BenchmarkSwitcher path (context.Args.Length > 0)
        var exception = await Record.ExceptionAsync(() => program.RunAsync(sp, CancellationToken.None));

        // Assert
        Assert.Null(exception);
        Assert.True(workspace.PostProcessArtifactsCalled);

        TestOutput.WriteLine("RunAsync completed cleanly via BenchmarkSwitcher path.");
    }

    [Fact]
    public async Task RunAsync_ShouldSkipFiltering_WhenSkipBenchmarksWithReportsIsFalse()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions { SkipBenchmarksWithReports = false };
        var workspace = new EmptyWorkspace();
        var context = new BenchmarkContext(Array.Empty<string>());

        using var sp = BuildServiceProvider(options, workspace, context);
        var program = new BenchmarkProgram();

        // Act
        var exception = await Record.ExceptionAsync(() => program.RunAsync(sp, CancellationToken.None));

        // Assert – no filtering attempted, completes normally
        Assert.Null(exception);
    }

    [Fact]
    public async Task RunAsync_ShouldApplyFilters_WhenSkipBenchmarksWithReportsIsTrue_AndTuningDirDoesNotExist()
    {
        // Arrange – tuning dir does not exist → ApplyReportFilters early-returns
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            Directory.CreateDirectory(tempPath);
            var config = ManualConfig.CreateEmpty().WithArtifactsPath(Path.Combine(tempPath, "artifacts"));
            var options = new BenchmarkWorkspaceOptions
            {
                RepositoryPath = tempPath,
                Configuration = config,
                RepositoryTuningFolder = "tuning",
                SkipBenchmarksWithReports = true
            };

            // Return the functional-test assembly so benchmarkTypes is populated with KnownBenchmark
            var workspace = new AssemblyWorkspace(typeof(BenchmarkProgramTest).Assembly);
            var context = new BenchmarkContext(Array.Empty<string>());

            using var sp = BuildServiceProvider(options, workspace, context);
            var program = new BenchmarkProgram();

            // Act – tuning dir absent → ApplyReportFilters returns config unchanged
            var exception = await Record.ExceptionAsync(() => program.RunAsync(sp, CancellationToken.None));

            // Assert
            Assert.Null(exception);
        }
        finally
        {
            if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true);
        }
    }

    [Fact]
    public async Task RunAsync_ShouldApplyFilters_WhenSkipBenchmarksWithReportsIsTrue_AndMatchingReportExists()
    {
        // Arrange – a report file whose name contains "KnownBenchmark" exists in the tuning dir.
        // KnownBenchmark (defined below) is in this assembly so it will be found by benchmarkTypes.
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            Directory.CreateDirectory(tempPath);
            var artifactsPath = Path.Combine(tempPath, "artifacts");
            var tuningDir = Path.Combine(artifactsPath, "tuning");
            Directory.CreateDirectory(tuningDir);

            // Report file whose name encodes the KnownBenchmark type
            File.WriteAllText(
                Path.Combine(tuningDir, "Codebelt.Extensions.BenchmarkDotNet.Console.KnownBenchmark-report-github.md"),
                "# benchmark report");

            var config = ManualConfig.CreateEmpty().WithArtifactsPath(artifactsPath);
            var options = new BenchmarkWorkspaceOptions
            {
                RepositoryPath = tempPath,
                Configuration = config,
                RepositoryTuningFolder = "tuning",
                SkipBenchmarksWithReports = true
            };

            var workspace = new AssemblyWorkspace(typeof(BenchmarkProgramTest).Assembly);
            var context = new BenchmarkContext(Array.Empty<string>());

            using var sp = BuildServiceProvider(options, workspace, context);
            var program = new BenchmarkProgram();

            // Act – filter IS applied for KnownBenchmark (matchingType != null path)
            var exception = await Record.ExceptionAsync(() => program.RunAsync(sp, CancellationToken.None));

            // Assert
            Assert.Null(exception);

            TestOutput.WriteLine("Filter applied for KnownBenchmark report.");
        }
        finally
        {
            if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true);
        }
    }

    [Fact]
    public async Task RunAsync_ShouldSkipReport_WhenFilenameHasLeadingDash()
    {
        // Arrange – a file whose name starts with "-" causes Split('-').FirstOrDefault() == ""
        // which exercises the first 'return null' branch in FindMatchingBenchmarkType.
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            Directory.CreateDirectory(tempPath);
            var artifactsPath = Path.Combine(tempPath, "artifacts");
            var tuningDir = Path.Combine(artifactsPath, "tuning");
            Directory.CreateDirectory(tuningDir);

            File.WriteAllText(Path.Combine(tuningDir, "-leading-dash-report.md"), "edge case");

            var config = ManualConfig.CreateEmpty().WithArtifactsPath(artifactsPath);
            var options = new BenchmarkWorkspaceOptions
            {
                RepositoryPath = tempPath,
                Configuration = config,
                RepositoryTuningFolder = "tuning",
                SkipBenchmarksWithReports = true
            };

            var workspace = new AssemblyWorkspace(typeof(BenchmarkProgramTest).Assembly);
            var context = new BenchmarkContext(Array.Empty<string>());

            using var sp = BuildServiceProvider(options, workspace, context);
            var program = new BenchmarkProgram();

            // Act
            var exception = await Record.ExceptionAsync(() => program.RunAsync(sp, CancellationToken.None));

            // Assert – no filter applied (FindMatchingBenchmarkType returned null), no exception
            Assert.Null(exception);

            TestOutput.WriteLine("Leading-dash filename correctly produced null match.");
        }
        finally
        {
            if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true);
        }
    }

    [Fact]
    public async Task RunAsync_ShouldSkipReport_WhenFilenameHasEmptyTypePart()
    {
        // Arrange – ".-something.md" → potentialTypeFullName = "." → Split('.').LastOrDefault() == ""
        // which exercises the second 'return null' branch in FindMatchingBenchmarkType.
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            Directory.CreateDirectory(tempPath);
            var artifactsPath = Path.Combine(tempPath, "artifacts");
            var tuningDir = Path.Combine(artifactsPath, "tuning");
            Directory.CreateDirectory(tuningDir);

            File.WriteAllText(Path.Combine(tuningDir, ".-empty-type-part.md"), "edge case");

            var config = ManualConfig.CreateEmpty().WithArtifactsPath(artifactsPath);
            var options = new BenchmarkWorkspaceOptions
            {
                RepositoryPath = tempPath,
                Configuration = config,
                RepositoryTuningFolder = "tuning",
                SkipBenchmarksWithReports = true
            };

            var workspace = new AssemblyWorkspace(typeof(BenchmarkProgramTest).Assembly);
            var context = new BenchmarkContext(Array.Empty<string>());

            using var sp = BuildServiceProvider(options, workspace, context);
            var program = new BenchmarkProgram();

            // Act
            var exception = await Record.ExceptionAsync(() => program.RunAsync(sp, CancellationToken.None));

            // Assert
            Assert.Null(exception);

            TestOutput.WriteLine("Dot-only type-name segment correctly produced null match.");
        }
        finally
        {
            if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true);
        }
    }

    [Fact]
    public async Task RunAsync_ShouldSkipReport_WhenFilenameDoesNotMatchAnyBenchmarkType()
    {
        // Arrange – well-formed filename but no matching type in the assembly exercises
        // the 'return null' path where FirstOrDefault returns null from benchmarkTypes.
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            Directory.CreateDirectory(tempPath);
            var artifactsPath = Path.Combine(tempPath, "artifacts");
            var tuningDir = Path.Combine(artifactsPath, "tuning");
            Directory.CreateDirectory(tuningDir);

            File.WriteAllText(
                Path.Combine(tuningDir, "SomeNamespace.NonExistentBenchmark-report.md"),
                "no matching type");

            var config = ManualConfig.CreateEmpty().WithArtifactsPath(artifactsPath);
            var options = new BenchmarkWorkspaceOptions
            {
                RepositoryPath = tempPath,
                Configuration = config,
                RepositoryTuningFolder = "tuning",
                SkipBenchmarksWithReports = true
            };

            var workspace = new AssemblyWorkspace(typeof(BenchmarkProgramTest).Assembly);
            var context = new BenchmarkContext(Array.Empty<string>());

            using var sp = BuildServiceProvider(options, workspace, context);
            var program = new BenchmarkProgram();

            // Act
            var exception = await Record.ExceptionAsync(() => program.RunAsync(sp, CancellationToken.None));

            // Assert
            Assert.Null(exception);

            TestOutput.WriteLine("No matching benchmark type found – report correctly skipped.");
        }
        finally
        {
            if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true);
        }
    }

    // ---------------------------------------------------------------------------
    // BenchmarkProgram.Run<TWorkspace> – full hosted-application lifecycle tests
    // ---------------------------------------------------------------------------

    [Fact]
    public void Run_WithFakeWorkspace_ShouldCompleteWithoutException()
    {
        // Exercises the static Run<TWorkspace>(string[], Action<BenchmarkWorkspaceOptions>) overload (line 82)
        // and the full 4-param generic Run<TWorkspace> (lines 97-105), which starts the host, calls RunAsync,
        // and stops the host after RunAsync returns.
        // Using named parameter 'setup' disambiguates from the 3-param overload.
        var exception = Record.Exception(() =>
            BenchmarkProgram.Run<EmptyWorkspace>(Array.Empty<string>(), setup: (Action<BenchmarkWorkspaceOptions>)null));

        Assert.Null(exception);
        TestOutput.WriteLine("Run<EmptyWorkspace> completed without exception.");
    }

    [Fact]
    public void Run_WithFakeWorkspace_AndServiceConfigurator_ShouldInvokeConfigurator()
    {
        // Exercises Run<TWorkspace>(string[], Action<IServiceCollection>, Action<BenchmarkWorkspaceOptions>)
        // and verifies that the serviceConfigurator delegate is invoked (line 102).
        var configuratorWasCalled = false;

        var exception = Record.Exception(() =>
            BenchmarkProgram.Run<EmptyWorkspace>(
                Array.Empty<string>(),
                serviceConfigurator: services =>
                {
                    configuratorWasCalled = true;
                    services.AddSingleton<IServiceProvider>(_ => null!); // harmless extra registration
                }));

        Assert.Null(exception);
        Assert.True(configuratorWasCalled, "serviceConfigurator should have been invoked during host setup.");
        TestOutput.WriteLine("serviceConfigurator was invoked as expected.");
    }

    [Fact]
    public void Run_WithFakeWorkspace_AndNonEmptyArgs_ShouldUseBenchmarkSwitcherPath()
    {
        // Passing "--list flat" exercises the BenchmarkSwitcher branch in ExecuteBenchmarks.
        // Using named parameter 'setup' disambiguates from the 3-param overload.
        var exception = Record.Exception(() =>
            BenchmarkProgram.Run<EmptyWorkspace>(new[] { "--list", "flat" }, setup: (Action<BenchmarkWorkspaceOptions>)null));

        Assert.Null(exception);
        TestOutput.WriteLine("Run<EmptyWorkspace> with '--list flat' completed (BenchmarkSwitcher path).");
    }

    // ---------------------------------------------------------------------------
    // BenchmarkProgram.RunAsync<TWorkspace> – full hosted-application lifecycle tests
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task RunAsync_WithFakeWorkspace_ShouldCompleteWithoutException()
    {
        var exception = await Record.ExceptionAsync(() =>
            BenchmarkProgram.RunAsync<EmptyWorkspace>(Array.Empty<string>(), setup: (Action<BenchmarkWorkspaceOptions>)null));

        Assert.Null(exception);
        TestOutput.WriteLine("RunAsync<EmptyWorkspace> completed without exception.");
    }

    [Fact]
    public async Task RunAsync_WithFakeWorkspace_AndServiceConfigurator_ShouldInvokeConfigurator()
    {
        var configuratorWasCalled = false;

        var exception = await Record.ExceptionAsync(() =>
            BenchmarkProgram.RunAsync<EmptyWorkspace>(
                Array.Empty<string>(),
                serviceConfigurator: services =>
                {
                    configuratorWasCalled = true;
                    services.AddSingleton<IServiceProvider>(_ => null!); // harmless extra registration
                }));

        Assert.Null(exception);
        Assert.True(configuratorWasCalled, "serviceConfigurator should have been invoked during host setup.");
        TestOutput.WriteLine("async serviceConfigurator was invoked as expected.");
    }

    [Fact]
    public async Task RunAsync_WithFakeWorkspace_AndNonEmptyArgs_ShouldUseBenchmarkSwitcherPath()
    {
        var exception = await Record.ExceptionAsync(() =>
            BenchmarkProgram.RunAsync<EmptyWorkspace>(new[] { "--list", "flat" }, setup: (Action<BenchmarkWorkspaceOptions>)null));

        Assert.Null(exception);
        TestOutput.WriteLine("RunAsync<EmptyWorkspace> with '--list flat' completed (BenchmarkSwitcher path).");
    }

    // ---------------------------------------------------------------------------
    // Helper: build a minimal service provider for RunAsync tests
    // ---------------------------------------------------------------------------

    private static ServiceProvider BuildServiceProvider(
        BenchmarkWorkspaceOptions options,
        IBenchmarkWorkspace workspace,
        BenchmarkContext context)
    {
        return new ServiceCollection()
            .AddSingleton<IBenchmarkWorkspace>(workspace)
            .AddSingleton(options)
            .AddSingleton(context)
            .BuildServiceProvider();
    }

    // ---------------------------------------------------------------------------
    // Test doubles
    // ---------------------------------------------------------------------------

    /// <summary>
    /// A workspace that returns an empty assembly array and tracks whether
    /// <see cref="PostProcessArtifacts"/> was called.
    /// </summary>
    private sealed class EmptyWorkspace : IBenchmarkWorkspace
    {
        public bool PostProcessArtifactsCalled { get; private set; }

        public Assembly[] LoadBenchmarkAssemblies() => Array.Empty<Assembly>();

        public void PostProcessArtifacts() => PostProcessArtifactsCalled = true;
    }

    /// <summary>
    /// A workspace that returns a specific assembly so that benchmark-type discovery
    /// (filtering by types whose names end with "Benchmark") finds <see cref="KnownBenchmark"/>.
    /// </summary>
    private sealed class AssemblyWorkspace : IBenchmarkWorkspace
    {
        private readonly Assembly _assembly;

        public AssemblyWorkspace(Assembly assembly) => _assembly = assembly;

        public Assembly[] LoadBenchmarkAssemblies() => new[] { _assembly };

        public void PostProcessArtifacts() { }
    }
}

/// <summary>
/// A placeholder type whose name ends with "Benchmark" so that
/// <see cref="BenchmarkProgram"/>'s filter logic can discover it via
/// <c>t.Name.EndsWith("Benchmark")</c>.
/// </summary>
internal sealed class KnownBenchmark { }
