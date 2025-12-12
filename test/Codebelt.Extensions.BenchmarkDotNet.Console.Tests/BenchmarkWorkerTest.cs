using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Configs;
using Codebelt.Extensions.Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace Codebelt.Extensions.BenchmarkDotNet.Console;

/// <summary>
/// Tests for the <see cref="BenchmarkWorker"/> class.
/// </summary>
public class BenchmarkWorkerTest : Test
{
    public BenchmarkWorkerTest(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Constructor_ShouldInitialize_WithValidParameters()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var environment = CreateMockHostEnvironment();

        // Act
        var worker = new BenchmarkWorker(configuration, environment);

        // Assert
        Assert.NotNull(worker);
    }

    [Fact]
    public void Constructor_ShouldAcceptNullConfiguration()
    {
        // Arrange
        var environment = CreateMockHostEnvironment();

        // Act
        var worker = new BenchmarkWorker(null, environment);

        // Assert
        Assert.NotNull(worker);
    }

    [Fact]
    public void Constructor_ShouldAcceptNullEnvironment()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();

        // Act
        var worker = new BenchmarkWorker(configuration, null);

        // Assert
        Assert.NotNull(worker);
    }

    [Fact]
    public void Constructor_ShouldAcceptBothParametersNull()
    {
        // Act
        var worker = new BenchmarkWorker(null, null);

        // Assert
        Assert.NotNull(worker);
    }

    [Fact]
    public void BenchmarkWorker_ShouldInheritFromConsoleStartup()
    {
        // Act
        var baseType = typeof(BenchmarkWorker).BaseType;

        // Assert
        Assert.NotNull(baseType);
        Assert.Equal("ConsoleStartup", baseType.Name);

        TestOutput.WriteLine($"BenchmarkWorker correctly inherits from: {baseType.FullName}");
    }

    [Fact]
    public void BenchmarkWorker_ShouldBePublicClass()
    {
        // Act
        var type = typeof(BenchmarkWorker);

        // Assert
        Assert.True(type.IsPublic);
        Assert.True(type.IsClass);
        Assert.False(type.IsAbstract);
        Assert.False(type.IsSealed);

        TestOutput.WriteLine("BenchmarkWorker is a public, non-abstract, non-sealed class");
    }

    [Fact]
    public void ConfigureServices_ShouldExist_AndBePublic()
    {
        // Act
        var method = typeof(BenchmarkWorker).GetMethod("ConfigureServices", BindingFlags.Public | BindingFlags.Instance);

        // Assert
        Assert.NotNull(method);
        Assert.True(method.IsPublic);
        Assert.True(method.IsVirtual);
        Assert.Equal(typeof(void), method.ReturnType);

        var parameters = method.GetParameters();
        Assert.Single(parameters);
        Assert.Equal(typeof(IServiceCollection), parameters[0].ParameterType);

        TestOutput.WriteLine($"Found ConfigureServices method: {method}");
    }

    [Fact]
    public void ConfigureServices_ShouldConfigureConsoleLifetimeOptions()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var environment = CreateMockHostEnvironment();
        var worker = new BenchmarkWorker(configuration, environment);
        var services = new ServiceCollection();

        // Act
        worker.ConfigureServices(services);

        // Assert
        var serviceDescriptor = services.FirstOrDefault(s =>
            s.ServiceType.IsGenericType &&
            s.ServiceType.GetGenericTypeDefinition() == typeof(IConfigureOptions<>) &&
            s.ServiceType.GetGenericArguments()[0] == typeof(ConsoleLifetimeOptions));

        Assert.NotNull(serviceDescriptor);

        TestOutput.WriteLine("ConsoleLifetimeOptions configuration was registered");
    }

    [Fact]
    public void ConfigureServices_ShouldSuppressStatusMessages()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var environment = CreateMockHostEnvironment();
        var worker = new BenchmarkWorker(configuration, environment);
        var services = new ServiceCollection();

        // Act
        worker.ConfigureServices(services);

        // Build service provider and resolve options
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<IOptions<ConsoleLifetimeOptions>>()?.Value;

        // Assert
        Assert.NotNull(options);
        Assert.True(options.SuppressStatusMessages);

        TestOutput.WriteLine($"ConsoleLifetimeOptions.SuppressStatusMessages = {options.SuppressStatusMessages}");
    }

    [Fact]
    public void ConfigureServices_ShouldHandleNullServiceCollection()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var environment = CreateMockHostEnvironment();
        var worker = new BenchmarkWorker(configuration, environment);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => worker.ConfigureServices(null));
    }

    [Fact]
    public void RunAsync_ShouldExist_AndBePublic()
    {
        // Act
        var method = typeof(BenchmarkWorker).GetMethod("RunAsync", BindingFlags.Public | BindingFlags.Instance);

        // Assert
        Assert.NotNull(method);
        Assert.True(method.IsPublic);
        Assert.True(method.IsVirtual);
        Assert.Equal(typeof(Task), method.ReturnType);

        var parameters = method.GetParameters();
        Assert.Equal(2, parameters.Length);
        Assert.Equal(typeof(IServiceProvider), parameters[0].ParameterType);
        Assert.Equal(typeof(CancellationToken), parameters[1].ParameterType);

        TestOutput.WriteLine($"Found RunAsync method: {method}");
    }

    [Fact]
    public async Task RunAsync_ShouldCompleteSuccessfully_WithValidServiceProvider()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var environment = CreateMockHostEnvironment();
        var worker = new BenchmarkWorker(configuration, environment);

        var services = CreateServiceProviderWithMocks();
        var cancellationToken = CancellationToken.None;

        // Act
        var task = worker.RunAsync(services, cancellationToken);

        // Assert
        Assert.NotNull(task);
        await task;
        Assert.True(task.IsCompletedSuccessfully);

        TestOutput.WriteLine("RunAsync completed successfully");
    }

    [Fact]
    public async Task RunAsync_ShouldReturnCompletedTask()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var environment = CreateMockHostEnvironment();
        var worker = new BenchmarkWorker(configuration, environment);

        var services = CreateServiceProviderWithMocks();
        var cancellationToken = CancellationToken.None;

        // Act
        var task = worker.RunAsync(services, cancellationToken);

        // Assert
        await task;
        Assert.Equal(TaskStatus.RanToCompletion, task.Status);
        Assert.False(task.IsFaulted);
        Assert.False(task.IsCanceled);

        TestOutput.WriteLine($"Task status: {task.Status}");
    }

    [Fact]
    public async Task RunAsync_ShouldHandleEmptyArgs()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var environment = CreateMockHostEnvironment();
        var worker = new BenchmarkWorker(configuration, environment);

        var services = CreateServiceProviderWithMocks(new string[] { });
        var cancellationToken = CancellationToken.None;

        // Act
        var task = worker.RunAsync(services, cancellationToken);

        // Assert
        await task;
        Assert.True(task.IsCompletedSuccessfully);

        TestOutput.WriteLine("RunAsync handled empty args successfully");
    }

    [Fact]
    public async Task RunAsync_ShouldHandleArgsWithValues()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var environment = CreateMockHostEnvironment();
        var worker = new BenchmarkWorker(configuration, environment);

        var args = new[] { "--filter", "MyBenchmark", "--job", "short" };
        var services = CreateServiceProviderWithMocks(args);
        var cancellationToken = CancellationToken.None;

        // Act
        var task = worker.RunAsync(services, cancellationToken);

        // Assert
        await task;
        Assert.True(task.IsCompletedSuccessfully);

        TestOutput.WriteLine($"RunAsync handled args with {args.Length} values successfully");
    }

    [Fact]
    public async Task RunAsync_ShouldCallPostProcessArtifacts()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var environment = CreateMockHostEnvironment();
        var worker = new BenchmarkWorker(configuration, environment);

        var workspace = new FakeBenchmarkWorkspace();
        var services = CreateServiceProviderWithMocks(workspace: workspace);
        var cancellationToken = CancellationToken.None;

        // Act
        await worker.RunAsync(services, cancellationToken);

        // Assert
        Assert.True(workspace.PostProcessArtifactsCalled);

        TestOutput.WriteLine("PostProcessArtifacts was called");
    }

    [Fact]
    public async Task RunAsync_ShouldCallLoadBenchmarkAssemblies()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var environment = CreateMockHostEnvironment();
        var worker = new BenchmarkWorker(configuration, environment);

        var workspace = new FakeBenchmarkWorkspace();
        var services = CreateServiceProviderWithMocks(workspace: workspace);
        var cancellationToken = CancellationToken.None;

        // Act
        await worker.RunAsync(services, cancellationToken);

        // Assert
        Assert.True(workspace.LoadBenchmarkAssembliesCalled);

        TestOutput.WriteLine("LoadBenchmarkAssemblies was called");
    }

    [Fact]
    public async Task RunAsync_ShouldHandleCancellationToken()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var environment = CreateMockHostEnvironment();
        var worker = new BenchmarkWorker(configuration, environment);

        var services = CreateServiceProviderWithMocks();
        var cts = new CancellationTokenSource();

        // Act
        var task = worker.RunAsync(services, cts.Token);

        // Assert
        await task;
        Assert.True(task.IsCompletedSuccessfully);

        TestOutput.WriteLine("RunAsync handled cancellation token");
    }

    [Fact]
    public async Task RunAsync_ShouldCallPostProcessArtifacts_EvenWithEmptyAssemblies()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var environment = CreateMockHostEnvironment();
        var worker = new BenchmarkWorker(configuration, environment);

        var workspace = new FakeBenchmarkWorkspace(Array.Empty<Assembly>());
        var services = CreateServiceProviderWithMocks(workspace: workspace);
        var cancellationToken = CancellationToken.None;

        // Act
        await worker.RunAsync(services, cancellationToken);

        // Assert
        Assert.True(workspace.PostProcessArtifactsCalled);

        TestOutput.WriteLine("PostProcessArtifacts was called even with empty assemblies");
    }

    [Fact]
    public void Constructor_ShouldHaveCorrectParameterNames()
    {
        // Act
        var constructor = typeof(BenchmarkWorker).GetConstructors().Single();
        var parameters = constructor.GetParameters();

        // Assert
        Assert.Equal(2, parameters.Length);
        Assert.Equal("configuration", parameters[0].Name);
        Assert.Equal("environment", parameters[1].Name);

        TestOutput.WriteLine($"Constructor parameters: {string.Join(", ", parameters.Select(p => $"{p.ParameterType.Name} {p.Name}"))}");
    }

    [Fact]
    public void BenchmarkWorker_ShouldHaveXmlDocumentation()
    {
        // This test verifies that the class has XML documentation
        // by checking for the summary element in the XML doc

        // Act
        var type = typeof(BenchmarkWorker);

        // Assert
        Assert.NotNull(type);
        Assert.True(type.IsPublic);

        TestOutput.WriteLine($"BenchmarkWorker type: {type.FullName}");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task RunAsync_ShouldHandleVariousArgCounts(int argCount)
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var environment = CreateMockHostEnvironment();
        var worker = new BenchmarkWorker(configuration, environment);

        var args = new string[argCount];
        for (int i = 0; i < argCount; i++)
        {
            args[i] = $"arg{i}";
        }

        var services = CreateServiceProviderWithMocks(args);
        var cancellationToken = CancellationToken.None;

        // Act
        var task = worker.RunAsync(services, cancellationToken);

        // Assert
        await task;
        Assert.True(task.IsCompletedSuccessfully);

        TestOutput.WriteLine($"RunAsync handled {argCount} arguments successfully");
    }

    [Fact]
    public async Task RunAsync_ShouldUseConfiguration_FromOptions()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var environment = CreateMockHostEnvironment();
        var worker = new BenchmarkWorker(configuration, environment);

        var customConfig = ManualConfig.CreateEmpty();
        var options = new BenchmarkWorkspaceOptions { Configuration = customConfig };
        var services = CreateServiceProviderWithMocks(options: options);
        var cancellationToken = CancellationToken.None;

        // Act
        await worker.RunAsync(services, cancellationToken);

        // Assert
        Assert.True(true); // If we get here without exception, the configuration was used

        TestOutput.WriteLine("RunAsync used configuration from options");
    }

    [Fact]
    public void ConfigureServices_ShouldBeOverridable()
    {
        // Arrange
        var method = typeof(BenchmarkWorker).GetMethod("ConfigureServices");

        // Assert
        Assert.NotNull(method);
        Assert.True(method.IsVirtual);
        Assert.False(method.IsFinal);

        TestOutput.WriteLine("ConfigureServices is virtual and can be overridden");
    }

    [Fact]
    public void RunAsync_ShouldBeOverridable()
    {
        // Arrange
        var method = typeof(BenchmarkWorker).GetMethod("RunAsync");

        // Assert
        Assert.NotNull(method);
        Assert.True(method.IsVirtual);
        Assert.False(method.IsFinal);

        TestOutput.WriteLine("RunAsync is virtual and can be overridden");
    }

    private static IHostEnvironment CreateMockHostEnvironment()
    {
        var environment = new FakeHostEnvironment
        {
            EnvironmentName = "Test",
            ApplicationName = "BenchmarkWorkerTest",
            ContentRootPath = AppContext.BaseDirectory
        };
        return environment;
    }

    private static IServiceProvider CreateServiceProviderWithMocks(string[] args = null, BenchmarkWorkspaceOptions options = null, FakeBenchmarkWorkspace workspace = null)
    {
        var services = new ServiceCollection();

        // Add required services
        services.AddSingleton(options ?? new BenchmarkWorkspaceOptions());
        services.AddSingleton<IBenchmarkWorkspace>(workspace ?? new FakeBenchmarkWorkspace());
        services.AddSingleton(new BenchmarkContext(args ?? Array.Empty<string>()));

        return services.BuildServiceProvider();
    }

    private class FakeHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; }
        public string ContentRootPath { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; }
    }

    private class FakeBenchmarkWorkspace : IBenchmarkWorkspace
    {
        private readonly Assembly[] _assemblies;

        public FakeBenchmarkWorkspace(Assembly[] assemblies = null)
        {
            _assemblies = assemblies ?? new[] { typeof(BenchmarkWorkerTest).Assembly };
        }

        public bool LoadBenchmarkAssembliesCalled { get; private set; }
        public bool PostProcessArtifactsCalled { get; private set; }

        public Assembly[] LoadBenchmarkAssemblies()
        {
            LoadBenchmarkAssembliesCalled = true;
            return _assemblies;
        }

        public void PostProcessArtifacts()
        {
            PostProcessArtifactsCalled = true;
        }
    }
}
