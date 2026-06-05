---
uid: Codebelt.Extensions.BenchmarkDotNet.Console
summary: *content
---
The `Codebelt.Extensions.BenchmarkDotNet.Console` namespace contains types that provide a structured and opinionated console-hosted execution model for `BenchmarkDotNet`.

Use `BenchmarkProgram.Run` for synchronous benchmark hosts and `BenchmarkProgram.RunAsync` for asynchronous benchmark hosts; both entry points support the default `BenchmarkWorkspace` and custom `IBenchmarkWorkspace` implementations.

[!INCLUDE [availability-modern](../../includes/availability-modern.md)]
