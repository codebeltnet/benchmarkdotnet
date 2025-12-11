# Codebelt.Extensions.BenchmarkDotNet.Console

A structured, host-based execution model for running BenchmarkDotNet benchmarks in console applications.

## About

**Codebelt.Extensions.BenchmarkDotNet.Console** extends the **Codebelt.Extensions.BenchmarkDotNet** package with a dedicated console runner built on the Microsoft Generic Host.

It provides a predictable startup model, consistent dependency injection, and a managed application lifecycle for benchmark execution, aligning benchmarks with the same hosting principles used in modern .NET applications.

By embracing `Microsoft.Extensions.Hosting`, this package enables clean separation between benchmark definition, configuration, and execution - making benchmarks easier to compose, test, and evolve over time.

At its core, the package favors convention over configuration and promotes benchmarks as first-class, host-managed workloads rather than ad-hoc console routines.

## CSharp Example

Benchmarks are executed through a console-hosted Generic Host, allowing full participation in dependency injection, configuration, logging and more.

```csharp
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using Codebelt.Extensions.BenchmarkDotNet.Console;

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkProgram.Run(args, o =>
        {
            o.AllowDebugBuild = BenchmarkProgram.IsDebugBuild;
            o.ConfigureBenchmarkDotNet(c =>
            {
                var slimJob = BenchmarkWorkspaceOptions.Slim;
                return c.AddJob(slimJob.WithRuntime(CoreRuntime.Core90))
                    .AddJob(slimJob.WithRuntime(CoreRuntime.Core10_0));
            });
        });
    }
}
```

## Related Packages

* [Codebelt.Extensions.BenchmarkDotNet](https://www.nuget.org/packages/Codebelt.Extensions.BenchmarkDotNet/) 📦
* [Codebelt.Extensions.BenchmarkDotNet.Console](https://www.nuget.org/packages/Codebelt.Extensions.BenchmarkDotNet.Console/) 📦
