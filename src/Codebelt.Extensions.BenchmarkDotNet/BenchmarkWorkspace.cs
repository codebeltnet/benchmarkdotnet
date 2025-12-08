using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using Cuemon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Codebelt.Extensions.BenchmarkDotNet;

/// <summary>
/// Provides a default implementation of <see cref="IBenchmarkWorkspace"/> for discovering and handling assemblies and their generated artifacts in BenchmarkDotNet.
/// </summary>
public sealed class BenchmarkWorkspace : IBenchmarkWorkspace
{
    private readonly BenchmarkWorkspaceOptions _options;
    private static bool _assemblyResolverRegistered;

    /// <summary>
    /// Initializes a new instance of the <see cref="BenchmarkWorkspace"/> class with the specified options.
    /// </summary>
    /// <param name="options">The <see cref="BenchmarkWorkspaceOptions"/> which configures repository paths, build modes and BenchmarkDotNet configuration.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="options"/> cannot be null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="options"/> are not in a valid state.
    /// </exception>
    public BenchmarkWorkspace(BenchmarkWorkspaceOptions options)
    {
        Validator.ThrowIfInvalidOptions(options);
        if (options.AllowDebugBuild && options.Configuration is ManualConfig mc)
        {
            mc.Options |= ConfigOptions.DisableOptimizationsValidator;
            options.Configuration = mc.AddJob(Job.Default.WithToolchain(new InProcessEmitToolchain(TimeSpan.FromHours(1), true)));
        }
        _options = options;
    }

    /// <summary>
    /// Loads benchmark assemblies discovered recursively in the tuning folder.
    /// </summary>
    /// <returns>An array of <see cref="Assembly"/> instances representing the loaded benchmark assemblies.</returns>
    /// <remarks>
    /// Assemblies are selected by matching the configured <see cref="BenchmarkWorkspaceOptions.BenchmarkProjectSuffix"/>,
    /// the build configuration (Debug/Release based on <see cref="BenchmarkWorkspaceOptions.AllowDebugBuild"/>),
    /// and the target framework moniker (<see cref="BenchmarkWorkspaceOptions.TargetFrameworkMoniker"/>).
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no matching assemblies could be loaded from the tuning folder. Ensure the tuning folder contains
    /// built benchmark assemblies for the configured build configuration and TFM.
    /// </exception>
    public Assembly[] LoadBenchmarkAssemblies()
    {
        var useDebugBuild = _options.AllowDebugBuild;
        return LoadAssemblies(
            _options.RepositoryPath,
            _options.TargetFrameworkMoniker,
            _options.BenchmarkProjectSuffix,
            _options.RepositoryTuningFolder,
            useDebugBuild).ToArray();
    }

    /// <summary>
    /// Performs post-processing of artifacts produced by BenchmarkDotNet.
    /// </summary>
    /// <remarks>
    /// This method moves files found in the BenchmarkDotNet artifacts "results" directory into the tuning folder under the configured artifacts path and then deletes the now-empty "results" directory.
    /// </remarks>
    public void PostProcessArtifacts()
    {
        var reportsResultsPath = Path.Combine(_options.Configuration.ArtifactsPath, "results");
        var reportsTuningPath = Path.Combine(_options.Configuration.ArtifactsPath, _options.RepositoryTuningFolder);
        CleanupResults(reportsResultsPath, reportsTuningPath);
    }

    private static IEnumerable<Assembly> LoadAssemblies(string repositoryPath, string targetFrameworkMoniker, string benchmarkProjectSuffix, string repositoryTuningFolder, bool useDebugBuild)
    {
        var tuningDir = Path.Combine(repositoryPath, repositoryTuningFolder);

        if (!Directory.Exists(tuningDir))
        {
            Directory.CreateDirectory(tuningDir);
        }

        if (!_assemblyResolverRegistered)
        {
            AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
            {
                try
                {
                    var requestedName = new AssemblyName(args.Name).Name;
                    if (string.IsNullOrEmpty(requestedName)) { return null; }
                    var fileName = requestedName + ".dll";

                    var match = Directory.EnumerateFiles(tuningDir, fileName, SearchOption.AllDirectories).FirstOrDefault();
                    if (match != null)
                    {
                        var asmName = AssemblyName.GetAssemblyName(match);
                        var already = AppDomain.CurrentDomain.GetAssemblies()
                            .FirstOrDefault(a => AssemblyName.ReferenceMatchesDefinition(a.GetName(), asmName));
                        if (already != null) { return already; }
                        return Assembly.LoadFrom(match);
                    }
                }
                catch
                {
                    // swallow and allow default resolution to continue
                }
                return null;
            };
            _assemblyResolverRegistered = true;
        }

        var alreadyLoaded = AppDomain.CurrentDomain.GetAssemblies();

        var assemblies = new List<Assembly>();
        var filteredAssemblies = Directory.EnumerateFiles(tuningDir, $"*.{benchmarkProjectSuffix}.dll", SearchOption.AllDirectories);
        var debugOrRelease = useDebugBuild
            ? "Debug"
            : "Release";

        foreach (var path in filteredAssemblies.Where(path => path.Contains($"bin{Path.DirectorySeparatorChar}{debugOrRelease}{Path.DirectorySeparatorChar}{targetFrameworkMoniker}", StringComparison.OrdinalIgnoreCase)))
        {
            try
            {
                var candidateName = AssemblyName.GetAssemblyName(path);
                var existing = alreadyLoaded.FirstOrDefault(a => AssemblyName.ReferenceMatchesDefinition(a.GetName(), candidateName));
                if (existing != null)
                {
                    assemblies.Add(existing);
                    continue;
                }

                if (assemblies.Any(a => AssemblyName.ReferenceMatchesDefinition(a.GetName(), candidateName)))
                {
                    continue;
                }

                assemblies.Add(Assembly.LoadFrom(path));
            }
            catch
            {
                // intentionally swallow to allow other assemblies to load
            }
        }

        if (assemblies.Count == 0)
        {
            throw new InvalidOperationException($"No assemblies were loaded. Ensure that '{tuningDir}' contains assemblies for '{debugOrRelease}' build and '{targetFrameworkMoniker}' moniker.");
        }

        return assemblies;
    }

    private static void CleanupResults(string reportsResultsPath, string reportsTuningPath)
    {
        if (!Directory.Exists(reportsResultsPath)) { return; }

        Directory.CreateDirectory(reportsTuningPath);

        foreach (var file in Directory.GetFiles(reportsResultsPath).Where(s => !s.EndsWith(".lock")))
        {
            var targetFile = Path.Combine(reportsTuningPath, Path.GetFileName(file));
            File.Delete(targetFile);
            File.Move(file, targetFile);
        }

        Directory.Delete(reportsResultsPath, recursive: true);
    }
}
