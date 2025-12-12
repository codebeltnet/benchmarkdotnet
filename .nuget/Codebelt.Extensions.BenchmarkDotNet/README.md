# Codebelt.Extensions.BenchmarkDotNet  

A unified, opinionated foundation for building robust BenchmarkDotNet workflows in .NET.

## About

**Codebelt.Extensions.BenchmarkDotNet** is part of a modern, MIT-licensed ecosystem designed to bring clarity, structure, and consistency to BenchmarkDotNet projects.

If you value predictable conventions, clean separation of responsibilities, and benchmarks that scale gracefully across `.NET 9` and `.NET 10`, this library is your agile companion.

It removes unnecessary ceremony while embracing best practices from other consumers of BenchmarkDotNet, so you can focus on performance insights, not plumbing.

At its heart, the package is **free, flexible, and crafted to extend and empower your agile codebelt**.

## Folder Structure

The folder structure promoted by **Codebelt.Extensions.BenchmarkDotNet** follows the same architectural principles commonly used for test projects—while remaining purpose-built for benchmarking.

At the solution level, benchmarks are treated as a first-class concern, clearly separated from tooling and output artifacts.

- **tuning** contains all benchmark projects (e.g. `*.Benchmarks`), in the same way that a `test` folder typically contains `*.Tests` projects,
- **tooling** hosts the executable console application responsible for discovering and running benchmarks,
- **reports** captures benchmark results and generated artifacts, separated from source code and tooling concerns.

This separation enforces a clean boundary between benchmark definition, execution, and output, making benchmark suites easier to scale, automate, and reason about.

### Example Layout

```text
Repository Root
│
├─ reports
│  └─ tuning
│     └─ github
│        └─ MyLibrary.ExampleBenchmarks-report-github.md
│
├─ src
│  └─ MyLibrary
│
├─ test
│  └─ MyLibrary.Tests
│     └─ ExampleTest.cs
│
├─ tooling
│  └─ benchmark-runner
│     └─ Program.cs
│
└─ tuning
   └─ MyLibrary.Benchmarks
      └─ ExampleBenchmark.cs
```


## CSharp Example

Benchmarks are executed using a Generic Host–based bootstrap model, allowing BenchmarkDotNet to participate in a fully managed application lifecycle with dependency injection, configuration, and logging.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

var hostBuilder = Host.CreateDefaultBuilder(args);
hostBuilder.ConfigureServices(services =>
{
    services.AddSingleton(new BenchmarkContext(args));
    services.AddBenchmarkWorkspace(setup);
});
var host = hostBuilder.Build();
host.Run();
```

The folder structure is based o

## Related Packages

* [Codebelt.Extensions.BenchmarkDotNet](https://www.nuget.org/packages/Codebelt.Extensions.BenchmarkDotNet/) 📦
* [Codebelt.Extensions.BenchmarkDotNet.Console](https://www.nuget.org/packages/Codebelt.Extensions.BenchmarkDotNet.Console/) 📦
