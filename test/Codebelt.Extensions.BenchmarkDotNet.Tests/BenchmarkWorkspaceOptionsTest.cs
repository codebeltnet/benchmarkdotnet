using BenchmarkDotNet.Configs;
using Codebelt.Extensions.Xunit;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Codebelt.Extensions.BenchmarkDotNet;

public class BenchmarkWorkspaceOptionsTest : Test
{
    public BenchmarkWorkspaceOptionsTest(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var options = new BenchmarkWorkspaceOptions();

        // Assert
        Assert.NotNull(options.RepositoryPath);
        Assert.NotNull(options.Configuration);
        Assert.NotNull(options.TargetFrameworkMoniker);
        Assert.Equal(BenchmarkWorkspaceOptions.DefaultRepositoryTuningFolder, options.RepositoryTuningFolder);
        Assert.Equal(BenchmarkWorkspaceOptions.DefaultRepositoryReportsFolder, options.RepositoryReportsFolder);
        Assert.Equal(BenchmarkWorkspaceOptions.DefaultBenchmarkProjectSuffix, options.BenchmarkProjectSuffix);
        Assert.False(options.AllowDebugBuild);
    }

    [Fact]
    public void DefaultRepositoryReportsFolder_ShouldBeReports()
    {
        // Assert
        Assert.Equal("reports", BenchmarkWorkspaceOptions.DefaultRepositoryReportsFolder);
    }

    [Fact]
    public void DefaultRepositoryTuningFolder_ShouldBeTuning()
    {
        // Assert
        Assert.Equal("tuning", BenchmarkWorkspaceOptions.DefaultRepositoryTuningFolder);
    }

    [Fact]
    public void DefaultBenchmarkProjectSuffix_ShouldBeBenchmarks()
    {
        // Assert
        Assert.Equal("Benchmarks", BenchmarkWorkspaceOptions.DefaultBenchmarkProjectSuffix);
    }

    [Fact]
    public void RepositoryPath_ShouldBeSettable()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions();
        var testPath = @"C:\TestRepo";

        // Act
        options.RepositoryPath = testPath;

        // Assert
        Assert.Equal(testPath, options.RepositoryPath);
    }

    [Fact]
    public void Configuration_ShouldBeSettable()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions();
        var customConfig = ManualConfig.CreateEmpty();

        // Act
        options.Configuration = customConfig;

        // Assert
        Assert.Same(customConfig, options.Configuration);
    }

    [Fact]
    public void TargetFrameworkMoniker_ShouldBeSettable()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions();
        var testTfm = "net8.0";

        // Act
        options.TargetFrameworkMoniker = testTfm;

        // Assert
        Assert.Equal(testTfm, options.TargetFrameworkMoniker);
    }

    [Fact]
    public void RepositoryTuningFolder_ShouldBeSettable()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions();
        var testFolder = "custom-tuning";

        // Act
        options.RepositoryTuningFolder = testFolder;

        // Assert
        Assert.Equal(testFolder, options.RepositoryTuningFolder);
    }

    [Fact]
    public void RepositoryReportsFolder_ShouldBeSettable()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions();
        var testFolder = "custom-reports";

        // Act
        options.RepositoryReportsFolder = testFolder;

        // Assert
        Assert.Equal(testFolder, options.RepositoryReportsFolder);
    }

    [Fact]
    public void BenchmarkProjectSuffix_ShouldBeSettable()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions();
        var testSuffix = "Perf";

        // Act
        options.BenchmarkProjectSuffix = testSuffix;

        // Assert
        Assert.Equal(testSuffix, options.BenchmarkProjectSuffix);
    }

    [Fact]
    public void UseDebugBuild_ShouldBeSettable()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions();

        // Act
        options.AllowDebugBuild = true;

        // Assert
        Assert.True(options.AllowDebugBuild);
    }

    [Fact]
    public void PostConfigureOptions_ShouldCombineRepositoryPathAndReportsFolder()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions();
        var testRepoPath = @"C:\MyBenchmarks";
        var testReportsFolder = "output";
        options.RepositoryPath = testRepoPath;
        options.RepositoryReportsFolder = testReportsFolder;

        // Act
        options.PostConfigureOptions();

        // Assert
        var expectedPath = Path.Combine(testRepoPath, testReportsFolder);
        Assert.Equal(expectedPath, options.Configuration.ArtifactsPath);
    }

    [Fact]
    public void ValidateOptions_ShouldNotThrow_WhenAllPropertiesAreValid()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions();

        // Act & Assert
        var exception = Record.Exception(() => options.ValidateOptions());
        Assert.Null(exception);
    }

    [Fact]
    public void ValidateOptions_ShouldThrow_WhenConfigurationIsNull()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions
        {
            Configuration = null
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => options.ValidateOptions());
    }

    [Fact]
    public void ValidateOptions_ShouldThrow_WhenRepositoryPathIsNull()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions
        {
            RepositoryPath = null
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => options.ValidateOptions());
    }

    [Fact]
    public void ValidateOptions_ShouldThrow_WhenRepositoryPathIsEmpty()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions
        {
            RepositoryPath = string.Empty
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => options.ValidateOptions());
    }

    [Fact]
    public void ValidateOptions_ShouldThrow_WhenRepositoryPathIsWhitespace()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions
        {
            RepositoryPath = "   "
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => options.ValidateOptions());
    }

    [Fact]
    public void ValidateOptions_ShouldThrow_WhenRepositoryTuningFolderIsNull()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions
        {
            RepositoryTuningFolder = null
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => options.ValidateOptions());
    }

    [Fact]
    public void ValidateOptions_ShouldThrow_WhenRepositoryTuningFolderIsEmpty()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions
        {
            RepositoryTuningFolder = string.Empty
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => options.ValidateOptions());
    }

    [Fact]
    public void ValidateOptions_ShouldThrow_WhenRepositoryTuningFolderIsWhitespace()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions
        {
            RepositoryTuningFolder = "   "
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => options.ValidateOptions());
    }

    [Fact]
    public void ValidateOptions_ShouldThrow_WhenRepositoryReportsFolderIsNull()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions
        {
            RepositoryReportsFolder = null
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => options.ValidateOptions());
    }

    [Fact]
    public void ValidateOptions_ShouldThrow_WhenRepositoryReportsFolderIsEmpty()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions
        {
            RepositoryReportsFolder = string.Empty
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => options.ValidateOptions());
    }

    [Fact]
    public void ValidateOptions_ShouldThrow_WhenRepositoryReportsFolderIsWhitespace()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions
        {
            RepositoryReportsFolder = "   "
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => options.ValidateOptions());
    }

    [Fact]
    public void ValidateOptions_ShouldThrow_WhenTargetFrameworkMonikerIsNull()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions
        {
            TargetFrameworkMoniker = null
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => options.ValidateOptions());
    }

    [Fact]
    public void ValidateOptions_ShouldThrow_WhenTargetFrameworkMonikerIsEmpty()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions
        {
            TargetFrameworkMoniker = string.Empty
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => options.ValidateOptions());
    }

    [Fact]
    public void ValidateOptions_ShouldThrow_WhenTargetFrameworkMonikerIsWhitespace()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions
        {
            TargetFrameworkMoniker = "   "
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => options.ValidateOptions());
    }

    [Fact]
    public void ValidateOptions_ShouldThrow_WhenBenchmarkProjectSuffixIsNull()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions
        {
            BenchmarkProjectSuffix = null
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => options.ValidateOptions());
    }

    [Fact]
    public void ValidateOptions_ShouldThrow_WhenBenchmarkProjectSuffixIsEmpty()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions
        {
            BenchmarkProjectSuffix = string.Empty
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => options.ValidateOptions());
    }

    [Fact]
    public void ValidateOptions_ShouldThrow_WhenBenchmarkProjectSuffixIsWhitespace()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions
        {
            BenchmarkProjectSuffix = "   "
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => options.ValidateOptions());
    }

    [Fact]
    public void Constructor_ShouldSetRepositoryPath_ToGitRootOrFallback()
    {
        // Arrange & Act
        var options = new BenchmarkWorkspaceOptions();

        // Assert
        Assert.NotNull(options.RepositoryPath);
        Assert.NotEmpty(options.RepositoryPath);

        TestOutput.WriteLine($"RepositoryPath: {options.RepositoryPath}");
    }

    [Fact]
    public void Constructor_ShouldSetConfiguration_WithMemoryDiagnoser()
    {
        // Arrange & Act
        var options = new BenchmarkWorkspaceOptions();

        // Assert
        Assert.NotNull(options.Configuration);
        Assert.Contains(options.Configuration.GetDiagnosers(), d => d.GetType().Name.Contains("Memory"));
    }

    [Fact]
    public void Constructor_ShouldSetConfiguration_WithConsoleLogger()
    {
        // Arrange & Act
        var options = new BenchmarkWorkspaceOptions();

        // Assert
        Assert.NotNull(options.Configuration);
        Assert.NotEmpty(options.Configuration.GetLoggers());
    }

    [Fact]
    public void Constructor_ShouldSetConfiguration_WithMarkdownExporter()
    {
        // Arrange & Act
        var options = new BenchmarkWorkspaceOptions();

        // Assert
        Assert.NotNull(options.Configuration);
        Assert.NotEmpty(options.Configuration.GetExporters());
    }

    [Fact]
    public void Constructor_ShouldSetTargetFrameworkMoniker_ToCurrentRuntime()
    {
        // Arrange & Act
        var options = new BenchmarkWorkspaceOptions();

        // Assert
        Assert.NotNull(options.TargetFrameworkMoniker);
        Assert.StartsWith("net", options.TargetFrameworkMoniker, StringComparison.OrdinalIgnoreCase);

        TestOutput.WriteLine($"TargetFrameworkMoniker: {options.TargetFrameworkMoniker}");
    }

    [Theory]
    [InlineData("net8.0")]
    [InlineData("net9.0")]
    [InlineData("net10.0")]
    public void TargetFrameworkMoniker_ShouldAcceptValidTfmFormats(string tfm)
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions();

        // Act
        options.TargetFrameworkMoniker = tfm;

        // Assert
        Assert.Equal(tfm, options.TargetFrameworkMoniker);
    }

    [Fact]
    public void PostConfigureOptions_ShouldPreserveOtherConfigurationSettings()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions();
        var originalLoggers = options.Configuration.GetLoggers();
        var originalExporters = options.Configuration.GetExporters();

        // Act
        options.PostConfigureOptions();

        // Assert
        Assert.Equal(originalLoggers.Count(), options.Configuration.GetLoggers().Count());
        Assert.Equal(originalExporters.Count(), options.Configuration.GetExporters().Count());
    }

    [Fact]
    public void UseDebugBuild_ShouldDefaultToFalse()
    {
        // Arrange & Act
        var options = new BenchmarkWorkspaceOptions();

        // Assert
        Assert.False(options.AllowDebugBuild);
    }
}
