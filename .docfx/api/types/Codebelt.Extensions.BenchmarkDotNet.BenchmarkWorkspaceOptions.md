---
uid: Codebelt.Extensions.BenchmarkDotNet.BenchmarkWorkspaceOptions
example:
- *content
---
The following example configures a `BenchmarkWorkspaceOptions` instance with the repository layout and discovery knobs the workspace will use, then runs the two lifecycle methods that the workspace invokes before the benchmark run (`PostConfigureOptions`) and during construction (`ValidateOptions`). This is the same shape every consumer follows: build an options instance, optionally override one or more defaults, and let the workspace validate the state.

```csharp
using System;
using Codebelt.Extensions.BenchmarkDotNet;

namespace MyBenchmarks;

public static class Program
{
    public static void Main()
    {
        var options = new BenchmarkWorkspaceOptions
        {
            // pin the workspace to a specific repository layout
            RepositoryPath = @"C:\Repos\MyBenchmarkRepo",
            RepositoryTuningFolder = "tuning",
            RepositoryReportsFolder = "reports",
            TargetFrameworkMoniker = "net10.0",
            BenchmarkProjectSuffix = "Benchmarks",
            AllowDebugBuild = false,
            SkipBenchmarksWithReports = true
        };

        // late-bind the BenchmarkDotNet artifacts path against the configured repository
        options.PostConfigureOptions();

        // throws InvalidOperationException if any required property is missing or whitespace
        options.ValidateOptions();

        Console.WriteLine($"ArtifactsPath: {options.Configuration.ArtifactsPath}");
    }
}
```
