using BenchmarkDotNet.Running;
using Codebelt.Bootstrapper.Console;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Codebelt.Extensions.BenchmarkDotNet.Console
{
    /// <summary>
    /// Worker responsible for executing benchmarks within the console host.
    /// </summary>
    /// <seealso cref="ConsoleStartup" />
    public class BenchmarkWorker : ConsoleStartup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BenchmarkWorker"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="environment">The host environment.</param>
        public BenchmarkWorker(IConfiguration configuration, IHostEnvironment environment) : base(configuration, environment)
        {
        }

        /// <summary>
        /// Configures services for the benchmark runner. Override this method to customize service registration.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <remarks>
        /// Suppresses console lifetime status messages to keep benchmark output clean.
        /// </remarks>
        public override void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ConsoleLifetimeOptions>(o => o.SuppressStatusMessages = true);
        }

        /// <summary>
        /// Runs the actual benchmarks as envisioned by BenchmarkDotNet.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A completed task when benchmark execution has finished.</returns>
        /// <remarks>
        /// When arguments are provided, they are forwarded to <see cref="BenchmarkSwitcher"/> for selective execution.
        /// After execution completes, the worker performs artifact post-processing.
        /// </remarks>
        public override Task RunAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var options = serviceProvider.GetRequiredService<BenchmarkWorkspaceOptions>();
            var workspace = serviceProvider.GetRequiredService<IBenchmarkWorkspace>();
            var assemblies = workspace.LoadBenchmarkAssemblies();
            var context = serviceProvider.GetRequiredService<BenchmarkContext>();

            try
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
            finally
            {
                workspace.PostProcessArtifacts();
            }

            return Task.CompletedTask;
        }
    }
}
