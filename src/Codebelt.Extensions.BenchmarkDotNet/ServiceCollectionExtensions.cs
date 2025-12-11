using System;
using Cuemon;
using Microsoft.Extensions.DependencyInjection;

namespace Codebelt.Extensions.BenchmarkDotNet
{
    /// <summary>
    /// Extension methods for the <see cref="IServiceCollection"/> interface.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the default benchmark workspace implementation (<see cref="BenchmarkWorkspace"/>) to the specified <see cref="IServiceCollection"/>
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="setup">The <see cref="BenchmarkWorkspaceOptions" /> which may be configured.</param>
        /// <returns>The original <see cref="IServiceCollection"/> instance for chaining.</returns>
        public static IServiceCollection AddBenchmarkWorkspace(this IServiceCollection services, Action<BenchmarkWorkspaceOptions> setup = null)
        {
            return AddBenchmarkWorkspace<BenchmarkWorkspace>(services, setup);
        }

        /// <summary>
        /// Adds a benchmark workspace implementation of type <typeparamref name="TWorkspace"/> to the specified <see cref="IServiceCollection"/>
        /// </summary>
        /// <typeparam name="TWorkspace">The type that implements the <see cref="IBenchmarkWorkspace"/> interface.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="setup">The <see cref="BenchmarkWorkspaceOptions" /> which may be configured.</param>
        /// <returns>The original <see cref="IServiceCollection"/> instance for chaining.</returns>
        /// <remarks>
        /// Validates the <paramref name="services"/> parameter and the provided <paramref name="setup"/> configurator.
        /// Registers <see cref="IBenchmarkWorkspace"/> as a singleton using <typeparamref name="TWorkspace"/>,
        /// applies the provided configuration, and registers the resolved <see cref="BenchmarkWorkspaceOptions"/> as a singleton.
        /// </remarks>
        public static IServiceCollection AddBenchmarkWorkspace<TWorkspace>(this IServiceCollection services, Action<BenchmarkWorkspaceOptions> setup = null) where TWorkspace : class, IBenchmarkWorkspace
        {
            Validator.ThrowIfNull(services);
            Validator.ThrowIfInvalidConfigurator(setup, out var options);
            return services
                .AddSingleton<IBenchmarkWorkspace, TWorkspace>()
                .Configure(setup ?? (_ => {}))
                .AddSingleton(options);
        }
    }
}
