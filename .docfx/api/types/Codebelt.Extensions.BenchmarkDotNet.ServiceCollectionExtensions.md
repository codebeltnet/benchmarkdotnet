---
uid: Codebelt.Extensions.BenchmarkDotNet.ServiceCollectionExtensions
example:
- *content
---
The following example registers a benchmark workspace through the non-generic `AddBenchmarkWorkspace` overload — which defaults to the built-in `BenchmarkWorkspace` implementation — and the generic `AddBenchmarkWorkspace<TWorkspace>` overload — which lets a consumer plug in a custom `IBenchmarkWorkspace` (here, a `FakeWorkspace` that simply returns an empty assembly set). Both overloads register the workspace and the resolved options as singletons, so any consumer can resolve `IBenchmarkWorkspace` and `BenchmarkWorkspaceOptions` straight from the built `IServiceProvider`.

```csharp
using System;
using System.Reflection;
using Codebelt.Extensions.BenchmarkDotNet;
using Microsoft.Extensions.DependencyInjection;

namespace MyBenchmarks;

// a custom workspace registered through the generic overload
public sealed class FakeWorkspace : IBenchmarkWorkspace
{
    public Assembly[] LoadBenchmarkAssemblies() => Array.Empty<Assembly>();
    public void PostProcessArtifacts() { }
}

public static class Program
{
    public static void Main()
    {
        // default registration: registers BenchmarkWorkspace and the resolved options as singletons
        var services = new ServiceCollection();
        services.AddBenchmarkWorkspace(setup: o => o.BenchmarkProjectSuffix = "Benchmarks");
        using (var provider = services.BuildServiceProvider())
        {
            var defaultWorkspace = provider.GetRequiredService<IBenchmarkWorkspace>();
            var defaultOptions = provider.GetRequiredService<BenchmarkWorkspaceOptions>();
            Console.WriteLine($"default: {defaultWorkspace.GetType().Name} / {defaultOptions.BenchmarkProjectSuffix}");
        }

        // generic registration: any IBenchmarkWorkspace implementation
        var typed = new ServiceCollection();
        typed.AddBenchmarkWorkspace<FakeWorkspace>(setup: o => o.RepositoryPath = @"C:\Repos\MyRepo");
        using (var typedProvider = typed.BuildServiceProvider())
        {
            var customWorkspace = typedProvider.GetRequiredService<IBenchmarkWorkspace>();
            var customOptions = typedProvider.GetRequiredService<BenchmarkWorkspaceOptions>();
            Console.WriteLine($"custom: {customWorkspace.GetType().Name} / {customOptions.RepositoryPath}");
        }
    }
}
```
