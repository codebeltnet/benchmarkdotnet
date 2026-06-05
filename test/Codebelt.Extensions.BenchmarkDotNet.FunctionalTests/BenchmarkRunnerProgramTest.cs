#if NET10_0_OR_GREATER
using BenchmarkDotNet.Environments;
using Codebelt.Extensions.BenchmarkDotNet.Console;
using Codebelt.Extensions.Xunit;
using Codebelt.Extensions.Xunit.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System.Linq;
using Xunit;
using RunnerProgram = Codebelt.Extensions.BenchmarkDotNet.Runner.Program;

namespace Codebelt.Extensions.BenchmarkDotNet;

/// <summary>
/// Functional tests for the benchmark runner entry point.
/// </summary>
public class BenchmarkRunnerProgramTest : Test
{
    public BenchmarkRunnerProgramTest(ITestOutputHelper output) : base(output)
    {
    }

    /// <summary>
    /// Verifies that the real runner entry point configures <see cref="BenchmarkWorkspaceOptions"/>
    /// as expected without executing benchmark workloads.
    /// </summary>
    [Fact]
    public void Program_Main_ShouldConfigureBenchmarkWorkspaceOptions()
    {
        using var host = ApplicationHostFactory.Create<RunnerProgram>(
            builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IHostedService>();
                });
            });

        var options = host.Services.GetRequiredService<BenchmarkWorkspaceOptions>();
        var jobs = options.Configuration.GetJobs().ToArray();

        Assert.Equal(BenchmarkProgram.IsDebugBuild, options.AllowDebugBuild);
        Assert.True(options.SkipBenchmarksWithReports);
        Assert.Contains(jobs, job => job.Environment.Runtime == CoreRuntime.Core90);
        Assert.Contains(jobs, job => job.Environment.Runtime == CoreRuntime.Core10_0);
    }
}
#endif
