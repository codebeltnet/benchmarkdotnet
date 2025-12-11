# Codebelt.Extensions.BenchmarkDotNet  

A unified, opinionated foundation for building robust BenchmarkDotNet workflows in .NET.

## About

**Codebelt.Extensions.BenchmarkDotNet** is part of a modern, MIT-licensed ecosystem designed to bring clarity, structure, and consistency to BenchmarkDotNet projects.

If you value predictable conventions, clean separation of responsibilities, and benchmarks that scale gracefully across `.NET 9` and `.NET 10`, this library is your agile companion.

It removes unnecessary ceremony while embracing best practices from other consumers of BenchmarkDotNet, so you can focus on performance insights, not plumbing.

At its heart, the package is **free, flexible, and crafted to extend and empower your agile codebelt**.

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

## Related Packages

* [Codebelt.Extensions.BenchmarkDotNet](https://www.nuget.org/packages/Codebelt.Extensions.BenchmarkDotNet/) 📦
* [Codebelt.Extensions.BenchmarkDotNet.Console](https://www.nuget.org/packages/Codebelt.Extensions.BenchmarkDotNet.Console/) 📦
