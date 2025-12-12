using System;
using System.Reflection;
using Codebelt.Extensions.Xunit;
using Microsoft.Extensions.DependencyInjection;
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

        private sealed class FakeWorkspace : IBenchmarkWorkspace
        {
            public Assembly[] LoadBenchmarkAssemblies() => Array.Empty<Assembly>();

            public void PostProcessArtifacts() { }
        }
    }
}
