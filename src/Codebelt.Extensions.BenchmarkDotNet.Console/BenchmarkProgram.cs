using Codebelt.Bootstrapper.Console;
using Cuemon;
using Cuemon.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Reflection;

namespace Codebelt.Extensions.BenchmarkDotNet.Console
{
    /// <summary>
    /// Entry point helper for hosting and running benchmarks using BenchmarkDotNet.
    /// </summary>
    /// <seealso cref="ConsoleProgram{TStartup}"/>
    public class BenchmarkProgram : ConsoleProgram<BenchmarkWorker>
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
            Run<BenchmarkWorkspace>(args, setup);
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
            var hostBuilder = CreateHostBuilder(args);
            hostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton(new BenchmarkContext(args));
                services.AddBenchmarkWorkspace<TWorkspace>(setup);
            });
            var host = hostBuilder.Build();
            host.Run();
        }
    }
}
