---
uid: Codebelt.Extensions.BenchmarkDotNet.Console
summary: *content
---
The `Codebelt.Extensions.BenchmarkDotNet.Console` namespace removes the boilerplate of turning a console application into a BenchmarkDotNet host. Use it when you want a single static call from `Main` to wire up the generic host, register a `BenchmarkContext` for the command-line arguments, register the default `BenchmarkWorkspace` (or your own `IBenchmarkWorkspace` implementation) through `AddBenchmarkWorkspace`, run every discovered benchmark assembly, and then post-process the generated artifacts — all without writing the hosting setup by hand.

Start with `BenchmarkProgram.Run` from your `Main` for a synchronous host, or `BenchmarkProgram.RunAsync` when your entry point is async. Both forward the command-line arguments into a `BenchmarkContext`, resolve the registered `IBenchmarkWorkspace` and `BenchmarkWorkspaceOptions` from the service provider, and then hand the loaded assemblies to `BenchmarkRunner` (when no arguments are supplied) or to `BenchmarkSwitcher` (when selective filtering is required). The generic `Run<TWorkspace>` / `RunAsync<TWorkspace>` overloads let you plug in a custom `IBenchmarkWorkspace` without rewriting the host.

If you need to suppress status messages in Release, register additional services, or surface a different `IHost` lifecycle, you can either rely on the `setup` delegate that mutates the resolved `BenchmarkWorkspaceOptions` or the optional `serviceConfigurator` delegate that mutates the `IServiceCollection` before the host is built.

[!INCLUDE [availability-modern](../../includes/availability-modern.md)]
