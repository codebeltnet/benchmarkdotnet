using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using Cuemon;
using Cuemon.Configuration;
using Perfolizer.Horology;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;

namespace Codebelt.Extensions.BenchmarkDotNet;

/// <summary>
/// Configuration options for <see cref="BenchmarkWorkspace"/>.
/// </summary>
/// <remarks>
/// The following table shows the initial property values for an instance of <see cref="BenchmarkWorkspaceOptions"/>.
/// <list type="table">
///     <listheader>
///         <term>Property</term>
///         <description>Initial Value</description>
///     </listheader>
///     <item>
///         <term><see cref="RepositoryPath"/></term>
///         <description>Resolved runtime by filesystem, e.g. C:\Repos\MyBenchmarkRepo.</description>
///     </item>
///     <item>
///         <term><see cref="Configuration"/></term>
///         <description>BenchmarkDotNet configured to use recommended settings as outlined in: https://github.com/dotnet/performance/blob/main/src/harness/BenchmarkDotNet.Extensions/RecommendedConfig.cs.</description>
///     </item>
///     <item>
///         <term><see cref="TargetFrameworkMoniker"/></term>
///         <description>Resolved runtime by reflection, e.g. net10.0.</description>
///     </item>
///     <item>
///         <term><see cref="RepositoryTuningFolder"/></term>
///         <description><c><see cref="DefaultRepositoryTuningFolder"/></c></description>
///     </item>
///     <item>
///         <term><see cref="RepositoryReportsFolder"/></term>
///         <description><c><see cref="DefaultRepositoryReportsFolder"/></c></description>
///     </item>
///     <item>
///         <term><see cref="BenchmarkProjectSuffix"/></term>
///         <description><c><see cref="DefaultBenchmarkProjectSuffix"/></c></description>
///     </item>
///     <item>
///         <term><see cref="AllowDebugBuild"/></term>
///         <description><c>false</c></description>
///     </item>
/// </list>
/// </remarks>
public class BenchmarkWorkspaceOptions : IValidatableParameterObject, IPostConfigurableParameterObject
{
    /// <summary>
    /// The default folder name where benchmark reports are written relative to the repository path.
    /// </summary>
    public const string DefaultRepositoryReportsFolder = "reports";

    /// <summary>
    /// The default folder name used for tuning artifacts relative to the repository path.
    /// </summary>
    public const string DefaultRepositoryTuningFolder = "tuning";

    /// <summary>
    /// The default suffix used to identify benchmark projects.
    /// </summary>
    public const string DefaultBenchmarkProjectSuffix = "Benchmarks";

    /// <summary>
    /// A tuned <see cref="Job"/> preset that serves as a fast, reliable baseline for most benchmarks, balancing measurement accuracy with developer efficiency.
    /// </summary>
    /// <value>
    /// A <see cref="Job"/> configured with reduced warmup, shortened iteration duration, controlled iteration counts, and without system power plan enforcement.
    /// </value>
    /// <remarks>
    /// Based on the recommended configuration used in the .NET Performance repository: https://github.com/dotnet/performance/blob/main/src/harness/BenchmarkDotNet.Extensions/RecommendedConfig.cs
    /// </remarks>
    public static readonly Job Slim = GetDefaultConfiguredJob();

    private static readonly string DefaultRepositoryPath = GetDefaultRepositoryPath();
    private static readonly string DefaultTargetFrameworkMoniker = ResolveCurrentTfm();
    private static readonly CultureInfo DanishCulture = CultureInfo.GetCultureInfo("da-DK");

    /// <summary>
    /// Initializes a new instance of the <see cref="BenchmarkWorkspaceOptions"/> class with sensible defaults.
    /// </summary>
    public BenchmarkWorkspaceOptions()
    {
        Configuration = GetDefaultConfiguration();
        RepositoryPath = DefaultRepositoryPath;
        TargetFrameworkMoniker = DefaultTargetFrameworkMoniker;
        RepositoryTuningFolder = DefaultRepositoryTuningFolder;
        RepositoryReportsFolder = DefaultRepositoryReportsFolder;
        BenchmarkProjectSuffix = DefaultBenchmarkProjectSuffix;
    }

    /// <summary>
    /// Gets or sets the root path of the repository where benchmark projects and reports live.
    /// </summary>
    /// <value>The repository root path.</value>
    public string RepositoryPath { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="IConfig"/> instance used by BenchmarkDotNet.
    /// </summary>
    /// <value>The BenchmarkDotNet configuration.</value>
    public IConfig Configuration { get; set; }

    /// <summary>
    /// Gets or sets the target framework moniker (TFM) string to be used for benchmark discovery and reporting.
    /// </summary>
    /// <value>The target framework moniker to run benchmarks for, e.g. <c>net10.0</c>.</value>
    public string TargetFrameworkMoniker { get; set; }

    /// <summary>
    /// Gets or sets the folder name under <see cref="RepositoryPath"/> used to store tuning artifacts.
    /// </summary>
    /// <value>The tuning folder name. Defaults to <see cref="DefaultRepositoryTuningFolder"/>.</value>
    public string RepositoryTuningFolder { get; set; }

    /// <summary>
    /// Gets or sets the folder name under <see cref="RepositoryPath"/> used to store generated reports.
    /// </summary>
    /// <value>The reports folder name. Defaults to <see cref="DefaultRepositoryReportsFolder"/>.</value>
    public string RepositoryReportsFolder { get; set; }

    /// <summary>
    /// Gets or sets the project name suffix used to identify benchmark projects.
    /// </summary>
    /// <value>
    /// The benchmark project suffix. Defaults to <see cref="DefaultBenchmarkProjectSuffix"/>.
    /// </value>
    public string BenchmarkProjectSuffix { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the benchmarks should be allowed to run using a Debug build.
    /// </summary>
    /// <value>
    /// <c>true</c> to allow Debug builds for benchmarks; otherwise, <c>false</c>. Default is <c>false</c>.
    /// </value>
    public bool AllowDebugBuild { get; set; }

    /// <summary>
    /// Finalizes the configured options before use.
    /// </summary>
    /// <remarks>This method updates the <see cref="Configuration"/> to set the BenchmarkDotNet artifacts path to the combination of <see cref="RepositoryPath"/> and <see cref="RepositoryReportsFolder"/>.</remarks>
    public void PostConfigureOptions()
    {
        if (string.IsNullOrWhiteSpace(Configuration.ArtifactsPath))
        {
            var artifactsPath = Path.Combine(RepositoryPath, RepositoryReportsFolder);

            if (Configuration is ManualConfig manual)
            {
                manual.ArtifactsPath = artifactsPath;
            }
            else
            {
                Configuration = Configuration.WithArtifactsPath(artifactsPath);
            }
        }
    }

    /// <summary>
    /// Determines whether the public read-write properties of this instance are in a valid state.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// <see cref="Configuration"/> cannot be <see langword="null"/>.
    /// -or-
    /// <see cref="RepositoryPath"/> cannot be <see langword="null"/>, empty or consist only of white-space characters.
    /// -or-
    /// <see cref="RepositoryTuningFolder"/> cannot be <see langword="null"/>, empty or consist only of white-space characters.
    /// -or-
    /// <see cref="RepositoryReportsFolder"/> cannot be <see langword="null"/>, empty or consist only of white-space characters.
    /// -or-
    /// <see cref="TargetFrameworkMoniker"/> cannot be <see langword="null"/>, empty or consist only of white-space characters.
    /// -or-
    /// <see cref="BenchmarkProjectSuffix"/> cannot be <see langword="null"/>, empty or consist only of white-space characters.
    /// </exception>
    public void ValidateOptions()
    {
        Validator.ThrowIfInvalidState(Configuration == null);
        Validator.ThrowIfInvalidState(string.IsNullOrWhiteSpace(RepositoryPath));
        Validator.ThrowIfInvalidState(string.IsNullOrWhiteSpace(RepositoryTuningFolder));
        Validator.ThrowIfInvalidState(string.IsNullOrWhiteSpace(RepositoryReportsFolder));
        Validator.ThrowIfInvalidState(string.IsNullOrWhiteSpace(TargetFrameworkMoniker));
        Validator.ThrowIfInvalidState(string.IsNullOrWhiteSpace(BenchmarkProjectSuffix));
    }

    private static string GetDefaultRepositoryPath()
    {
        var dir = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(dir))
        {
            if (Directory.Exists(Path.Combine(dir, ".git")))
            {
                return dir;
            }
            dir = Path.GetDirectoryName(dir);
        }
        return Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..");
    }

    private static ManualConfig GetDefaultConfiguration()
    {
        var config = ManualConfig.CreateEmpty()
            .WithBuildTimeout(TimeSpan.FromMinutes(15)) // for slow machines
            .AddLogger(ConsoleLogger.Default) // log output to console
            .AddValidator(DefaultConfig.Instance.GetValidators().ToArray()) // copy default validators
            .AddAnalyser(DefaultConfig.Instance.GetAnalysers().ToArray()) // copy default analysers
            .AddExporter(MarkdownExporter.GitHub) // export to GitHub markdown
            .AddColumnProvider(DefaultColumnProviders.Instance) // display default columns (method name, args etc)
            .AddJob(Slim.AsDefault()) // tell BDN that this are our default settings
            .AddDiagnoser(MemoryDiagnoser.Default) // MemoryDiagnoser is enabled by default
            .AddColumn(StatisticColumn.Median, StatisticColumn.Min, StatisticColumn.Max)
            .WithSummaryStyle(SummaryStyle.Default.WithMaxParameterColumnWidth(36).WithCultureInfo(DanishCulture)); // the default is 20 and trims too aggressively some benchmark results
        config.Options = ConfigOptions.DisableLogFile;
        return config;
    }

    private static Job GetDefaultConfiguredJob()
    {
        return Job.Default
            .WithWarmupCount(1) // 1 warmup is enough for our purpose
            .WithIterationTime(TimeInterval.FromMilliseconds(250)) // the default is 0.5s per iteration, which is slightly too much for us
            .WithMinIterationCount(15)
            .WithMaxIterationCount(20) // we don't want to run more than 20 iterations
            .DontEnforcePowerPlan(); // make sure BDN does not try to enforce High Performance power plan on Windows
    }

    private static string ResolveCurrentTfm()
    {
        try
        {
            var entry = Assembly.GetEntryAssembly();
            var tfa = entry?.GetCustomAttribute<TargetFrameworkAttribute>();
            if (!string.IsNullOrEmpty(tfa?.FrameworkName))
            {
                var fn = new FrameworkName(tfa.FrameworkName);
                var v = fn.Version;

                // .NET Framework → net11, net20, net35, net40, net403, net45, net451, ..., net48, net481
                if (fn.Identifier.Equals(".NETFramework", StringComparison.OrdinalIgnoreCase))
                {
                    // Base: net4{minor} or net{major}{minor} for older ones
                    var tfm = $"net{v.Major}{v.Minor}";

                    // For 4.x: append Build when present (4.0.3 → net403, 4.5.1 → net451, 4.8.1 → net481)
                    if (v.Major >= 4 && v.Build > 0)
                    {
                        tfm += v.Build;
                    }

                    return tfm;
                }

                // .NET Standard → netstandard1.0–2.1
                if (fn.Identifier.Equals(".NETStandard", StringComparison.OrdinalIgnoreCase))
                {
                    return $"netstandard{v.Major}.{v.Minor}";
                }

                // .NET Core / .NET (CoreApp)
                if (fn.Identifier.Equals(".NETCoreApp", StringComparison.OrdinalIgnoreCase))
                {
                    // .NET Core 1.0–3.1 use netcoreappX.Y
                    if (v.Major <= 3)
                    {
                        return $"netcoreapp{v.Major}.{v.Minor}";
                    }

                    // .NET 5+ uses netX.Y
                    return $"net{v.Major}.{v.Minor}";
                }
            }
        }
        catch
        {
            // ignore and fallback to dir inspection
        }

        try
        {
            var baseDir = AppContext.BaseDirectory;
            var dir = new DirectoryInfo(baseDir);
            while (dir != null)
            {
                if (dir.Name.StartsWith("net", StringComparison.OrdinalIgnoreCase))
                {
                    return dir.Name;
                }
                dir = dir.Parent;
            }
        }
        catch
        {
            // ignore and fallback to null
        }

        return null;
    }
}
