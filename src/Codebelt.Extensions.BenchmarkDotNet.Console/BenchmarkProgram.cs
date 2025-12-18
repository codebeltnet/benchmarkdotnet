using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Filters;
using BenchmarkDotNet.Running;
using Codebelt.Bootstrapper.Console;
using Cuemon;
using Cuemon.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Codebelt.Extensions.BenchmarkDotNet.Console
{
    /// <summary>
    /// Entry point helper for hosting and running benchmarks using BenchmarkDotNet.
    /// </summary>
    /// <seealso cref="ConsoleProgram{TStartup}"/>
    public class BenchmarkProgram : MinimalConsoleProgram<BenchmarkProgram>
    {
        static BenchmarkProgram()
        {
            var isDebugBuild = Decorator.Enclose(Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).IsDebugBuild();
            BuildConfiguration = isDebugBuild ? "Debug" : "Release";
            IsDebugBuild = isDebugBuild;
        }

        /// <summary>
        /// Gets the build configuration of the entry assembly.
        /// </summary>
        /// <value>The value is either <c>Debug</c> or <c>Release</c>.</value>
        public static string BuildConfiguration { get; }

        /// <summary>
        /// Gets a value indicating whether the entry assembly was built in Debug configuration.
        /// </summary>
        /// <value><c>true</c> if the entry assembly was compiled with debugging information; otherwise, <c>false</c>.</value>
        public static bool IsDebugBuild { get; }

        /// <summary>
        /// Runs benchmarks using the default <see cref="BenchmarkWorkspace"/> implementation.
        /// </summary>
        /// <param name="args">The command-line arguments passed to the application.</param>
        /// <param name="setup">The <see cref="BenchmarkWorkspaceOptions"/> which may be configured.</param>
        /// <remarks>
        /// This method configures the host builder with the necessary services, builds the host, and runs it to execute benchmarks.
        /// </remarks>
        public static void Run(string[] args, Action<BenchmarkWorkspaceOptions> setup = null)
        {
            Run(args, null, setup);
        }

        /// <summary>
        /// Runs benchmarks using the default <see cref="BenchmarkWorkspace"/> implementation.
        /// </summary>
        /// <param name="args">The command-line arguments passed to the application.</param>
        /// <param name="serviceConfigurator">The delegate that will be invoked to configure additional services in the <see cref="IServiceCollection"/>.</param>
        /// <param name="setup">The <see cref="BenchmarkWorkspaceOptions"/> which may be configured.</param>
        /// <remarks>
        /// This method configures the host builder with the necessary services, builds the host, and runs it to execute benchmarks.
        /// </remarks>
        public static void Run(string[] args, Action<IServiceCollection> serviceConfigurator = null, Action<BenchmarkWorkspaceOptions> setup = null)
        {
            Run<BenchmarkWorkspace>(args, serviceConfigurator, setup);
        }

        /// <summary>
        /// Runs benchmarks using a custom implementation of <see cref="IBenchmarkWorkspace"/>.
        /// </summary>
        /// <typeparam name="TWorkspace">The type of the workspace that implements <see cref="IBenchmarkWorkspace"/>.</typeparam>
        /// <param name="args">The command-line arguments passed to the application.</param>
        /// <param name="setup">The <see cref="BenchmarkWorkspaceOptions"/> which may be configured.</param>
        /// <remarks>
        /// This method configures the host builder with the necessary services, builds the host, and runs it to execute benchmarks.
        /// </remarks>
        public static void Run<TWorkspace>(string[] args, Action<BenchmarkWorkspaceOptions> setup = null) where TWorkspace : class, IBenchmarkWorkspace
        {
            Run<TWorkspace>(args, null, setup);
        }

        /// <summary>
        /// Runs benchmarks using a custom implementation of <see cref="IBenchmarkWorkspace"/>.
        /// </summary>
        /// <typeparam name="TWorkspace">The type of the workspace that implements <see cref="IBenchmarkWorkspace"/>.</typeparam>
        /// <param name="args">The command-line arguments passed to the application.</param>
        /// <param name="serviceConfigurator">The delegate that will be invoked to configure additional services in the <see cref="IServiceCollection"/>.</param>
        /// <param name="setup">The <see cref="BenchmarkWorkspaceOptions"/> which may be configured.</param>
        /// <remarks>
        /// This method configures the host builder with the necessary services, builds the host, and runs it to execute benchmarks.
        /// </remarks>
        public static void Run<TWorkspace>(string[] args, Action<IServiceCollection> serviceConfigurator = null, Action<BenchmarkWorkspaceOptions> setup = null) where TWorkspace : class, IBenchmarkWorkspace
        {
            var builder = CreateHostBuilder(args);
            
            builder.Services.Configure<ConsoleLifetimeOptions>(o => o.SuppressStatusMessages = !IsDebugBuild);
            builder.Services.AddSingleton(new BenchmarkContext(args));
            builder.Services.AddBenchmarkWorkspace<TWorkspace>(setup);
            serviceConfigurator?.Invoke(builder.Services);
            
            using var host = builder.Build();
            host.Run();
        }

        /// <summary>
        /// Runs the actual benchmarks as envisioned by BenchmarkDotNet.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A completed task when benchmark execution has finished.</returns>
        /// <remarks>
        /// When arguments are provided, they are forwarded to <see cref="BenchmarkSwitcher"/> for selective execution.
        /// After execution completes, artifact post-processing is performed.
        /// </remarks>
        public override Task RunAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var options = serviceProvider.GetRequiredService<BenchmarkWorkspaceOptions>();
            var workspace = serviceProvider.GetRequiredService<IBenchmarkWorkspace>();
            var assemblies = workspace.LoadBenchmarkAssemblies();
            var context = serviceProvider.GetRequiredService<BenchmarkContext>();

            if (options.SkipBenchmarksWithReports) { ConfigureBenchmarkDotNetFiltersForExistingReports(options, assemblies); }

            try
            {
                ExecuteBenchmarks(assemblies, context, options);
            }
            finally
            {
                workspace.PostProcessArtifacts();
            }

            return Task.CompletedTask;
        }

        private static void ConfigureBenchmarkDotNetFiltersForExistingReports(BenchmarkWorkspaceOptions options, Assembly[] assemblies)
        {
            var benchmarkTypes = assemblies
                .SelectMany(a => a.GetTypes().Where(t => t.Name.EndsWith("Benchmark", StringComparison.Ordinal)))
                .ToList();

            options.ConfigureBenchmarkDotNet(c => ApplyReportFilters(c, options, benchmarkTypes));
        }

        private static IConfig ApplyReportFilters(IConfig config, BenchmarkWorkspaceOptions options, List<Type> benchmarkTypes)
        {
            var tuningPath = BenchmarkWorkspace.GetReportsTuningPath(options);
            if (!Directory.Exists(tuningPath)) { return config; }

            foreach (var report in Directory.EnumerateFiles(tuningPath))
            {
                var matchingType = FindMatchingBenchmarkType(report, benchmarkTypes);
                if (matchingType != null)
                {
                    config = config.AddFilter(new SimpleFilter(bc => bc.Descriptor.Type != matchingType));
                }
            }

            return config;
        }

        private static Type FindMatchingBenchmarkType(string reportPath, List<Type> benchmarkTypes)
        {
            var filename = Path.GetFileNameWithoutExtension(reportPath);
            var potentialTypeFullName = filename.Split('-').FirstOrDefault();
            if (string.IsNullOrWhiteSpace(potentialTypeFullName)) { return null; }

            var potentialTypeName = potentialTypeFullName.Split('.').LastOrDefault();
            if (string.IsNullOrWhiteSpace(potentialTypeName)) { return null; }

            return benchmarkTypes.FirstOrDefault(t => t.Name.Equals(potentialTypeName, StringComparison.OrdinalIgnoreCase));
        }

        private static void ExecuteBenchmarks(Assembly[] assemblies, BenchmarkContext context, BenchmarkWorkspaceOptions options)
        {
            if (context.Args.Length == 0)
            {
                foreach (var assembly in assemblies)
                {
                    BenchmarkRunner.Run(assembly, options.Configuration);
                }
            }
            else
            {
                BenchmarkSwitcher
                    .FromAssemblies(assemblies)
                    .Run(context.Args, options.Configuration);
            }
        }
    }
}
