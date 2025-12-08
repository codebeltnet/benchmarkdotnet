using System.Reflection;

namespace Codebelt.Extensions.BenchmarkDotNet;

/// <summary>
/// Defines a way for discovering and handling assemblies and their generated artifacts in BenchmarkDotNet.
/// </summary>
public interface IBenchmarkWorkspace
{
    /// <summary>
    /// Loads assemblies that contain BenchmarkDotNet benchmarks.
    /// </summary>
    /// <returns>An array of <see cref="Assembly"/> instances representing the loaded benchmark assemblies.</returns>
    Assembly[] LoadBenchmarkAssemblies();

    /// <summary>
    /// Performs post-processing on artifacts produced by BenchmarkDotNet.
    /// </summary>
    void PostProcessArtifacts();
}
