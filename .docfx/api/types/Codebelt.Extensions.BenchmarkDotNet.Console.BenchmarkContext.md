---
uid: Codebelt.Extensions.BenchmarkDotNet.Console.BenchmarkContext
example:
- *content
---
The following example constructs a `BenchmarkContext` from the command-line arguments passed to the entry point. When the host resolves `BenchmarkContext` from the service provider, it inspects `Args.Length` to decide whether to run every benchmark in every loaded assembly (`BenchmarkRunner.Run`) or to forward the args to `BenchmarkSwitcher` for selective execution. A `null` array is normalized to an empty array so downstream code never has to guard against `null`.

```csharp
using System;
using Codebelt.Extensions.BenchmarkDotNet.Console;

namespace MyBenchmarks;

public static class Program
{
    public static void Main(string[] args)
    {
        var context = new BenchmarkContext(args);
        // context.Args is the same array passed to Main, or an empty array when args is null
        Console.WriteLine($"BenchmarkContext received {context.Args.Length} argument(s).");
        foreach (var arg in context.Args)
        {
            Console.WriteLine($"  - {arg}");
        }
    }
}
```
