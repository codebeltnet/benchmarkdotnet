---
uid: Codebelt.Extensions.BenchmarkDotNet.BenchmarkWorkspaceOptionsExtensions
example:
- *content
---
The following example shows `ConfigureBenchmarkDotNet` being used to add a second BenchmarkDotNet job to the default `IConfig` carried by `BenchmarkWorkspaceOptions`. The helper takes care of forcing the default configuration, passing the current `IConfig` to the delegate, and assigning the returned configuration back onto the options instance — which is otherwise awkward because BenchmarkDotNet's `AddJob` / `AddColumn` / `AddDiagnoser` methods return a new configuration object rather than mutating the receiver.

```csharp
using System;
using System.Linq;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using Codebelt.Extensions.BenchmarkDotNet;
using Perfolizer.Horology;

namespace MyBenchmarks;

public static class Program
{
    public static void Main()
    {
        var options = new BenchmarkWorkspaceOptions();

        // fluent IConfig mutations normally require explicit reassignment; this helper does it for you
        options.ConfigureBenchmarkDotNet(c => c.AddJob(
            Job.Default
                .WithWarmupCount(2)
                .WithIterationTime(TimeInterval.FromMilliseconds(500))
                .WithMaxIterationCount(25)
                .WithId("LongRunning")));

        Console.WriteLine($"Jobs: {string.Join(", ", options.Configuration.GetJobs().Select(j => j.Id))}");
    }
}
```
