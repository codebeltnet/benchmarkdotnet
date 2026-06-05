using System;
using System.Reflection;
using Codebelt.Extensions.Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Codebelt.Extensions.BenchmarkDotNet
{
    public class ServiceCollectionExtensionsTest : Test
    {
        public ServiceCollectionExtensionsTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void AddBenchmarkWorkspace_ShouldThrowWhenServicesIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => ServiceCollectionExtensions.AddBenchmarkWorkspace((IServiceCollection)null));
        }

        [Fact]
        public void AddBenchmarkWorkspace_ShouldRegisterDefaultWorkspaceAndOptions_WhenCalledWithoutGeneric()
        {
            var services = new ServiceCollection();
            services.AddBenchmarkWorkspace(setup: options => options.BenchmarkProjectSuffix = "MySuffix");
            using var sp = services.BuildServiceProvider();

            var workspace = sp.GetRequiredService<IBenchmarkWorkspace>();
            Assert.IsType<BenchmarkWorkspace>(workspace);

            var options = sp.GetRequiredService<BenchmarkWorkspaceOptions>();
            Assert.Equal("MySuffix", options.BenchmarkProjectSuffix);
        }

        [Fact]
        public void AddBenchmarkWorkspace_GenericOverload_ShouldRegisterCustomImplementationAndOptions()
        {
            var services = new ServiceCollection();
            services.AddBenchmarkWorkspace<FakeWorkspace>(setup: options => options.RepositoryPath = "repo-path");
            using var sp = services.BuildServiceProvider();

            var workspace = sp.GetRequiredService<IBenchmarkWorkspace>();
            Assert.IsType<FakeWorkspace>(workspace);

            var options = sp.GetRequiredService<BenchmarkWorkspaceOptions>();
            Assert.Equal("repo-path", options.RepositoryPath);
        }

        [Fact]
        public void AddBenchmarkWorkspace_GenericOverload_ShouldUseNullCoalescingLambda_WhenSetupIsNull()
        {
            // When setup is null, the Configure call registers a no-op lambda (_ => {}).
            // Resolving IOptions<BenchmarkWorkspaceOptions>.Value causes the Options framework to
            // invoke all registered configure-actions, which executes the null-coalescing lambda body.
            var services = new ServiceCollection();
            services.AddBenchmarkWorkspace<FakeWorkspace>(setup: null);
            using var sp = services.BuildServiceProvider();

            // Resolving via IOptions<T> triggers the registered configure action (the no-op lambda)
            var optionsAccessor = sp.GetRequiredService<IOptions<BenchmarkWorkspaceOptions>>();
            var options = optionsAccessor.Value;

            Assert.NotNull(options);
            Assert.IsType<FakeWorkspace>(sp.GetRequiredService<IBenchmarkWorkspace>());

            TestOutput.WriteLine($"BenchmarkProjectSuffix: {options.BenchmarkProjectSuffix}");
        }

        private sealed class FakeWorkspace : IBenchmarkWorkspace
        {
            public Assembly[] LoadBenchmarkAssemblies() => Array.Empty<Assembly>();

            public void PostProcessArtifacts() { }
        }
    }
}
