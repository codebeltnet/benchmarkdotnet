using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;

namespace Codebelt.Extensions.BenchmarkDotNet.Console;

/// <summary>
/// Benchmarks for the <see cref="BenchmarkWorker"/> class.
/// </summary>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class BenchmarkWorkerBenchmark
{
    private IConfiguration _configuration;
    private IHostEnvironment _environment;
    private BenchmarkWorker _worker;
    private IServiceCollection _services;

    [GlobalSetup]
    public void Setup()
    {
        // Deterministic initialization of test data
        var configData = new Dictionary<string, string>
        {
            ["Setting1"] = "Value1",
            ["Setting2"] = "Value2"
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        _environment = new TestHostEnvironment
        {
            EnvironmentName = "Development",
            ApplicationName = "BenchmarkTest"
        };

        _worker = new BenchmarkWorker(_configuration, _environment);
        _services = new ServiceCollection();
    }

    [Benchmark(Baseline = true, Description = "Construct BenchmarkWorker")]
    public BenchmarkWorker ConstructWorker()
    {
        return new BenchmarkWorker(_configuration, _environment);
    }

    [Benchmark(Description = "Configure services")]
    public IServiceCollection ConfigureServices()
    {
        var services = new ServiceCollection();
        _worker.ConfigureServices(services);
        return services;
    }

    [Benchmark(Description = "Configure services with options")]
    public IServiceCollection ConfigureServicesWithOptions()
    {
        var services = new ServiceCollection();
        _worker.ConfigureServices(services);
        var provider = services.BuildServiceProvider();
        return services;
    }

    [Benchmark(Description = "Access configuration")]
    public IConfiguration AccessConfiguration()
    {
        return _configuration;
    }

    [Benchmark(Description = "Access environment")]
    public IHostEnvironment AccessEnvironment()
    {
        return _environment;
    }

    /// <summary>
    /// Test implementation of IHostEnvironment for benchmark purposes.
    /// </summary>
    private class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; }
        public string ContentRootPath { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; }
    }
}
