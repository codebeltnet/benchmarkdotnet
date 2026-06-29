---
uid: Codebelt.Extensions.BenchmarkDotNet.Console.BenchmarkProgram
example:
- *content
---
The following example shows the typical `Program.cs` of a benchmark host project: the entry point forwards the command-line arguments to `BenchmarkProgram.RunAsync` and supplies a `setup` delegate that customizes the resolved `BenchmarkWorkspaceOptions` before the host is built. `BenchmarkProgram` derives from `Codebelt.Bootstrapper.Console.MinimalConsoleProgram<BenchmarkProgram>`, so the `Main` plumbing — host configuration, service registration, lifecycle — is inherited; the only thing the host project has to write is the call site and the workspace configuration.

```csharp
using System.Threading.Tasks;
using Codebelt.Extensions.BenchmarkDotNet;
using Codebelt.Extensions.BenchmarkDotNet.Console;

namespace MyBenchmarks;

public static class Program
{
    // minimal benchmark host: forwards args into the auto-discovered benchmark workspace
    public static Task Main(string[] args) => BenchmarkProgram.RunAsync(args, setup: options =>
    {
        options.BenchmarkProjectSuffix = "MyBench";
        options.AllowDebugBuild = false;
        options.SkipBenchmarksWithReports = true;
    });
}
```
