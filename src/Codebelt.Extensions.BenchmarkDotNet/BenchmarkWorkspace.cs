using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using Cuemon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Codebelt.Extensions.BenchmarkDotNet;

/// <summary>
/// Provides a default implementation of <see cref="IBenchmarkWorkspace"/> for discovering and handling assemblies and their generated artifacts in BenchmarkDotNet.
/// </summary>
public sealed class BenchmarkWorkspace : IBenchmarkWorkspace
{
    private readonly BenchmarkWorkspaceOptions _options;
    private static bool _assemblyResolverRegistered;
    private static readonly Lock AssemblyResolverLock = new();
    private static Dictionary<string, string> _assemblyLookup = new(StringComparer.OrdinalIgnoreCase);

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
        Directory.CreateDirectory(tuningDir);

        UpdateAssemblyLookup(tuningDir);
        EnsureAssemblyResolverRegistered();

        var debugOrRelease = useDebugBuild ? "Debug" : "Release";
        var buildSegment = Path.Combine("bin", debugOrRelease, targetFrameworkMoniker);

        var alreadyLoaded = AppDomain.CurrentDomain.GetAssemblies();
        var assemblies = new List<Assembly>();

        var candidatePaths = Directory
            .EnumerateFiles(tuningDir, $"*.{benchmarkProjectSuffix}.dll", SearchOption.AllDirectories)
            .Where(path => path.IndexOf(buildSegment, StringComparison.OrdinalIgnoreCase) >= 0);

        foreach (var path in candidatePaths)
        {
            try
            {
                var candidateName = AssemblyName.GetAssemblyName(path);
                var existing = alreadyLoaded.FirstOrDefault(a =>
                    AssemblyName.ReferenceMatchesDefinition(a.GetName(), candidateName));

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
                // swallow and continue
            }
        }

        if (assemblies.Count == 0)
        {
            throw new InvalidOperationException($"No assemblies were loaded. Ensure that '{tuningDir}' contains assemblies for '{debugOrRelease}' build and '{targetFrameworkMoniker}' moniker.");
        }

        return assemblies;
    }

    private static void UpdateAssemblyLookup(string tuningDir)
    {
        var allDlls = Directory.EnumerateFiles(tuningDir, "*.dll", SearchOption.AllDirectories);
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in allDlls)
        {
            var simpleName = Path.GetFileNameWithoutExtension(path);
            if (string.IsNullOrEmpty(simpleName))
            {
                continue;
            }
            map[simpleName] = path;
        }
        _assemblyLookup = map;
    }

    private static void EnsureAssemblyResolverRegistered()
    {
        lock (AssemblyResolverLock)
        {
            if (_assemblyResolverRegistered) { return; }

            AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
            {
                try
                {
                    var requestedName = new AssemblyName(args.Name).Name;
                    if (string.IsNullOrEmpty(requestedName)) { return null; }

                    if (!_assemblyLookup.TryGetValue(requestedName, out var path) || !File.Exists(path))
                    {
                        return null;
                    }

                    var asmName = AssemblyName.GetAssemblyName(path);
                    var already = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => AssemblyName.ReferenceMatchesDefinition(a.GetName(), asmName));
                    if (already != null) { return already; }

                    return Assembly.LoadFrom(path);
                }
                catch
                {
                    return null;
                }
            };

            _assemblyResolverRegistered = true;
        }
    }

    private static void CleanupResults(string reportsResultsPath, string reportsTuningPath)
    {
        if (!Directory.Exists(reportsResultsPath)) { return; }

        Directory.CreateDirectory(reportsTuningPath);

        foreach (var file in Directory.GetFiles(reportsResultsPath))
        {
            var targetFile = Path.Combine(reportsTuningPath, Path.GetFileName(file));
            File.Move(file, targetFile, true);
        }

        Directory.Delete(reportsResultsPath, recursive: true);
    }
}
