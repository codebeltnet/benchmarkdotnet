---
uid: Codebelt.Extensions.BenchmarkDotNet.BenchmarkWorkspace
example:
- *content
---
The following example builds a default `BenchmarkWorkspace` against the current repository and walks through the two phases the workspace actually performs: discovering every `*.Benchmarks.dll` under the configured `tuning` folder for the current build configuration and target framework moniker, then moving the per-run `results` directory into the long-lived `tuning` directory once the benchmark run finishes.

```csharp
using System;
using System.IO;
using Codebelt.Extensions.BenchmarkDotNet;

namespace MyBenchmarks;

public static class Program
{
    public static void Main()
    {
        var repositoryPath = Directory.GetCurrentDirectory();
        var options = new BenchmarkWorkspaceOptions
        {
            RepositoryPath = repositoryPath,
            TargetFrameworkMoniker = "net10.0"
        };

        var workspace = new BenchmarkWorkspace(options);

        // Discovers and loads every *.Benchmarks.dll that matches
        // <RepositoryPath>/<RepositoryTuningFolder>/bin/<Debug|Release>/<TargetFrameworkMoniker>.
        var assemblies = workspace.LoadBenchmarkAssemblies();
        Console.WriteLine($"Loaded {assemblies.Length} benchmark assemblies from {repositoryPath}.");

        // After a benchmark run, PostProcessArtifacts moves the per-run results directory
        // into the configured tuning folder and removes the now-empty results directory.
        workspace.PostProcessArtifacts();
    }
}
```
