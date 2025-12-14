using BenchmarkDotNet.Configs;
using Codebelt.Extensions.Xunit;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Cuemon;
using Cuemon.Reflection;
using Xunit;

namespace Codebelt.Extensions.BenchmarkDotNet;

public class BenchmarkWorkspaceTest : Test
{
    private static readonly bool IsDebugBuild = GetBuildConfiguration();

    public BenchmarkWorkspaceTest(ITestOutputHelper output) : base(output)
    {
    }

    private static bool GetBuildConfiguration()
    {
        return Decorator.Enclose(typeof(BenchmarkWorkspaceTest).Assembly).IsDebugBuild();
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenOptionsIsNull()
    {
        // Arrange
        BenchmarkWorkspaceOptions options = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new BenchmarkWorkspace(options));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenOptionsAreInvalid()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions
        {
            RepositoryPath = null
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new BenchmarkWorkspace(options));
    }

    [Fact]
    public void Constructor_ShouldSucceed_WhenOptionsAreValid()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions();

        // Act
        var workspace = new BenchmarkWorkspace(options);

        // Assert
        Assert.NotNull(workspace);
    }

    [Fact]
    public void Constructor_ShouldEnableDisableOptimizationsValidator_WhenUseDebugBuildIsTrue()
    {
        // Arrange
        var config = ManualConfig.CreateEmpty();
        var options = new BenchmarkWorkspaceOptions
        {
            Configuration = config,
            AllowDebugBuild = true
        };

        // Act
        var workspace = new BenchmarkWorkspace(options);

        // Assert
        Assert.NotNull(workspace);
        var manualConfig = options.Configuration as ManualConfig;
        Assert.NotNull(manualConfig);
        Assert.True(manualConfig.Options.HasFlag(ConfigOptions.DisableOptimizationsValidator));
    }

    [Fact]
    public void Constructor_ShouldNotModifyConfiguration_WhenUseDebugBuildIsFalse()
    {
        // Arrange
        var config = ManualConfig.CreateEmpty();
        var originalOptions = config.Options;
        var options = new BenchmarkWorkspaceOptions
        {
            Configuration = config,
            AllowDebugBuild = false
        };

        // Act
        var workspace = new BenchmarkWorkspace(options);

        // Assert
        Assert.NotNull(workspace);
        var manualConfig = options.Configuration as ManualConfig;
        Assert.NotNull(manualConfig);
        Assert.Equal(originalOptions, manualConfig.Options);
    }

    [Fact]
    public void Constructor_ShouldNotModifyConfiguration_WhenConfigurationIsNotManualConfig()
    {
        // Arrange
        var config = DefaultConfig.Instance;
        var options = new BenchmarkWorkspaceOptions
        {
            Configuration = config
        };

        // Act
        var workspace = new BenchmarkWorkspace(options);

        // Assert
        Assert.NotNull(workspace);
        Assert.Same(config, options.Configuration);
    }

    [Fact]
    public void LoadBenchmarkAssemblies_ShouldThrowInvalidOperationException_WhenNoAssembliesFound()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            Directory.CreateDirectory(tempPath);
            var options = new BenchmarkWorkspaceOptions
            {
                RepositoryPath = tempPath,
                RepositoryTuningFolder = "tuning",
                BenchmarkProjectSuffix = "Benchmarks",
                TargetFrameworkMoniker = "net10.0",
                AllowDebugBuild = IsDebugBuild
            };

            var workspace = new BenchmarkWorkspace(options);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => workspace.LoadBenchmarkAssemblies());

            TestOutput.WriteLine($"{exception.Message}");

            Assert.Contains("No assemblies were loaded", exception.Message);
            Assert.Contains(IsDebugBuild ? "Debug" : "Release", exception.Message);
            Assert.Contains("net10.0", exception.Message);
        }
        finally
        {
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    [Fact]
    public void LoadBenchmarkAssemblies_ShouldCreateTuningDirectory_WhenItDoesNotExist()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            Directory.CreateDirectory(tempPath);
            var options = new BenchmarkWorkspaceOptions
            {
                RepositoryPath = tempPath,
                RepositoryTuningFolder = "tuning",
                BenchmarkProjectSuffix = "Benchmarks",
                TargetFrameworkMoniker = "net10.0",
                AllowDebugBuild = IsDebugBuild
            };

            var workspace = new BenchmarkWorkspace(options);
            var expectedTuningPath = Path.Combine(tempPath, "tuning");

            // Act
            try
            {
                workspace.LoadBenchmarkAssemblies();
            }
            catch (InvalidOperationException)
            {
                // Expected when no assemblies found
            }

            // Assert
            Assert.True(Directory.Exists(expectedTuningPath));
        }
        finally
        {
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    [Fact]
    public void LoadBenchmarkAssemblies_ShouldReturnLoadedAssemblies_WhenMatchingAssembliesExist()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions()
        {
            AllowDebugBuild = IsDebugBuild,
            TargetFrameworkMoniker = "net10.0"
        };
        var workspace = new BenchmarkWorkspace(options);

        // Act
        var assemblies = workspace.LoadBenchmarkAssemblies();

        // Assert
        Assert.NotNull(assemblies);
        Assert.NotEmpty(assemblies);
        Assert.All(assemblies, assembly => Assert.NotNull(assembly));
        
        TestOutput.WriteLine($"Current build configuration: {(IsDebugBuild ? "Debug" : "Release")}");
        TestOutput.WriteLine($"Loaded {assemblies.Length} benchmark assemblies:");
        foreach (var assembly in assemblies)
        {
            TestOutput.WriteLine($"  - {assembly.GetName().Name}");
        }
    }

    [Fact]
    public void LoadBenchmarkAssemblies_ShouldFilterByBuildConfiguration()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions
        {
            AllowDebugBuild = IsDebugBuild,
            TargetFrameworkMoniker = "net10.0"
        };
        var workspace = new BenchmarkWorkspace(options);
        var expectedBuildConfig = IsDebugBuild ? "Debug" : "Release";

        // Act
        var assemblies = workspace.LoadBenchmarkAssemblies();

        // Assert
        Assert.NotNull(assemblies);
        Assert.All(assemblies, assembly =>
        {
            var location = assembly.Location;
            TestOutput.WriteLine($"Assembly location: {location}");
            Assert.Contains(expectedBuildConfig, location, StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public void LoadBenchmarkAssemblies_ShouldFilterByTargetFrameworkMoniker()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions()
        {
            AllowDebugBuild = IsDebugBuild,
            TargetFrameworkMoniker = "net10.0"
        };
        var workspace = new BenchmarkWorkspace(options);
        var expectedTfm = options.TargetFrameworkMoniker;

        // Act
        var assemblies = workspace.LoadBenchmarkAssemblies();

        // Assert
        Assert.NotNull(assemblies);
        Assert.All(assemblies, assembly =>
        {
            var location = assembly.Location;
            TestOutput.WriteLine($"Assembly location: {location}");
            Assert.Contains(expectedTfm, location, StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public void LoadBenchmarkAssemblies_ShouldFilterByBenchmarkProjectSuffix()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions
        {
            BenchmarkProjectSuffix = "Benchmarks",
            AllowDebugBuild = IsDebugBuild,
            TargetFrameworkMoniker = "net10.0"
        };
        var workspace = new BenchmarkWorkspace(options);

        // Act
        var assemblies = workspace.LoadBenchmarkAssemblies();

        // Assert
        Assert.NotNull(assemblies);
        Assert.All(assemblies, assembly =>
        {
            var name = assembly.GetName().Name;
            TestOutput.WriteLine($"Assembly name: {name}");
            Assert.Contains("Benchmarks", name, StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public void LoadBenchmarkAssemblies_ShouldNotLoadDuplicateAssemblies()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions()
        {
            TargetFrameworkMoniker = "net10.0",
            AllowDebugBuild = IsDebugBuild
        };
        var workspace = new BenchmarkWorkspace(options);

        // Act
        var assemblies = workspace.LoadBenchmarkAssemblies();

        // Assert
        Assert.NotNull(assemblies);
        var uniqueAssemblies = assemblies.Distinct().ToArray();
        Assert.Equal(assemblies.Length, uniqueAssemblies.Length);
    }

    [Fact]
    public void LoadBenchmarkAssemblies_ShouldReuseAlreadyLoadedAssemblies()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions()
        {
            AllowDebugBuild = IsDebugBuild,
            TargetFrameworkMoniker = "net10.0"
        };
        var workspace = new BenchmarkWorkspace(options);

        // Act
        var firstLoad = workspace.LoadBenchmarkAssemblies();
        var secondLoad = workspace.LoadBenchmarkAssemblies();

        // Assert
        Assert.NotNull(firstLoad);
        Assert.NotNull(secondLoad);
        Assert.All(firstLoad, firstAssembly =>
        {
            var matchingSecondAssembly = secondLoad.FirstOrDefault(second => 
                AssemblyName.ReferenceMatchesDefinition(second.GetName(), firstAssembly.GetName()));
            Assert.NotNull(matchingSecondAssembly);
        });
    }

    [Fact]
    public void PostProcessArtifacts_ShouldMoveFilesFromResultsDirectory()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            Directory.CreateDirectory(tempPath);
            var artifactsPath = Path.Combine(tempPath, "artifacts");
            var resultsDir = Path.Combine(artifactsPath, "results");
            var tuningDir = Path.Combine(artifactsPath, "tuning");
            
            Directory.CreateDirectory(resultsDir);
            
            var testFile1 = Path.Combine(resultsDir, "test1.txt");
            var testFile2 = Path.Combine(resultsDir, "test2.md");
            File.WriteAllText(testFile1, "Test content 1");
            File.WriteAllText(testFile2, "Test content 2");

            var config = ManualConfig.CreateEmpty().WithArtifactsPath(artifactsPath);
            var options = new BenchmarkWorkspaceOptions
            {
                RepositoryPath = tempPath,
                Configuration = config,
                RepositoryTuningFolder = "tuning"
            };

            var workspace = new BenchmarkWorkspace(options);

            // Act
            workspace.PostProcessArtifacts();

            // Assert
            Assert.False(Directory.Exists(resultsDir));
            Assert.True(Directory.Exists(tuningDir));
            Assert.True(File.Exists(Path.Combine(tuningDir, "test1.txt")));
            Assert.True(File.Exists(Path.Combine(tuningDir, "test2.md")));
            
            TestOutput.WriteLine($"Files moved successfully to: {tuningDir}");
        }
        finally
        {
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    [Fact]
    public void PostProcessArtifacts_ShouldDoNothing_WhenResultsDirectoryDoesNotExist()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            Directory.CreateDirectory(tempPath);
            var artifactsPath = Path.Combine(tempPath, "artifacts");
            Directory.CreateDirectory(artifactsPath);

            var config = ManualConfig.CreateEmpty().WithArtifactsPath(artifactsPath);
            var options = new BenchmarkWorkspaceOptions
            {
                RepositoryPath = tempPath,
                Configuration = config,
                RepositoryTuningFolder = "tuning"
            };

            var workspace = new BenchmarkWorkspace(options);

            // Act
            var exception = Record.Exception(() => workspace.PostProcessArtifacts());

            // Assert
            Assert.Null(exception);
        }
        finally
        {
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    [Fact]
    public void PostProcessArtifacts_ShouldOverwriteExistingFiles()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            Directory.CreateDirectory(tempPath);
            var artifactsPath = Path.Combine(tempPath, "artifacts");
            var resultsDir = Path.Combine(artifactsPath, "results");
            var tuningDir = Path.Combine(artifactsPath, "tuning");
            
            Directory.CreateDirectory(resultsDir);
            Directory.CreateDirectory(tuningDir);
            
            var sourceFile = Path.Combine(resultsDir, "test.txt");
            var targetFile = Path.Combine(tuningDir, "test.txt");
            
            File.WriteAllText(sourceFile, "New content");
            File.WriteAllText(targetFile, "Old content");

            var config = ManualConfig.CreateEmpty().WithArtifactsPath(artifactsPath);
            var options = new BenchmarkWorkspaceOptions
            {
                RepositoryPath = tempPath,
                Configuration = config,
                RepositoryTuningFolder = "tuning"
            };

            var workspace = new BenchmarkWorkspace(options);

            // Act
            workspace.PostProcessArtifacts();

            // Assert
            Assert.False(Directory.Exists(resultsDir));
            Assert.True(File.Exists(targetFile));
            var content = File.ReadAllText(targetFile);
            Assert.Equal("New content", content);
            
            TestOutput.WriteLine($"File successfully overwritten at: {targetFile}");
        }
        finally
        {
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    [Fact]
    public void PostProcessArtifacts_ShouldDeleteResultsDirectoryRecursively()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            Directory.CreateDirectory(tempPath);
            var artifactsPath = Path.Combine(tempPath, "artifacts");
            var resultsDir = Path.Combine(artifactsPath, "results");
            var subDir = Path.Combine(resultsDir, "subdir");
            
            Directory.CreateDirectory(subDir);
            File.WriteAllText(Path.Combine(resultsDir, "file1.txt"), "Content 1");
            File.WriteAllText(Path.Combine(subDir, "file2.txt"), "Content 2");

            var config = ManualConfig.CreateEmpty().WithArtifactsPath(artifactsPath);
            var options = new BenchmarkWorkspaceOptions
            {
                RepositoryPath = tempPath,
                Configuration = config,
                RepositoryTuningFolder = "tuning"
            };

            var workspace = new BenchmarkWorkspace(options);

            // Act
            workspace.PostProcessArtifacts();

            // Assert
            Assert.False(Directory.Exists(resultsDir));
            Assert.False(Directory.Exists(subDir));
        }
        finally
        {
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    [Fact]
    public void PostProcessArtifacts_ShouldCreateTuningDirectory_WhenItDoesNotExist()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            Directory.CreateDirectory(tempPath);
            var artifactsPath = Path.Combine(tempPath, "artifacts");
            var resultsDir = Path.Combine(artifactsPath, "results");
            
            Directory.CreateDirectory(resultsDir);
            File.WriteAllText(Path.Combine(resultsDir, "test.txt"), "Content");

            var config = ManualConfig.CreateEmpty().WithArtifactsPath(artifactsPath);
            var options = new BenchmarkWorkspaceOptions
            {
                RepositoryPath = tempPath,
                Configuration = config,
                RepositoryTuningFolder = "tuning"
            };

            var workspace = new BenchmarkWorkspace(options);
            var expectedTuningPath = Path.Combine(artifactsPath, "tuning");

            // Act
            workspace.PostProcessArtifacts();

            // Assert
            Assert.True(Directory.Exists(expectedTuningPath));
        }
        finally
        {
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    [Fact]
    public void BenchmarkWorkspace_ShouldImplementIBenchmarkWorkspace()
    {
        // Arrange & Act
        var options = new BenchmarkWorkspaceOptions();
        var workspace = new BenchmarkWorkspace(options);

        // Assert
        Assert.IsAssignableFrom<IBenchmarkWorkspace>(workspace);
    }

    [Fact]
    public void LoadBenchmarkAssemblies_ShouldHandleAssemblyLoadFailuresGracefully()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var buildConfig = IsDebugBuild ? "Debug" : "Release";
        try
        {
            Directory.CreateDirectory(tempPath);
            var tuningDir = Path.Combine(tempPath, "tuning");
            var buildDir = Path.Combine(tuningDir, "bin", buildConfig, "net10.0");
            Directory.CreateDirectory(buildDir);

            // Create an invalid DLL file
            var invalidDll = Path.Combine(buildDir, "Invalid.Benchmarks.dll");
            File.WriteAllText(invalidDll, "This is not a valid assembly");

            // Create a valid assembly reference
            var validDll = Path.Combine(buildDir, "Valid.Benchmarks.dll");
            File.Copy(Assembly.GetExecutingAssembly().Location, validDll, true);

            var options = new BenchmarkWorkspaceOptions
            {
                RepositoryPath = tempPath,
                RepositoryTuningFolder = "tuning",
                BenchmarkProjectSuffix = "Benchmarks",
                TargetFrameworkMoniker = "net10.0",
                AllowDebugBuild = IsDebugBuild
            };

            var workspace = new BenchmarkWorkspace(options);

            // Act
            var assemblies = workspace.LoadBenchmarkAssemblies();

            // Assert
            Assert.NotNull(assemblies);
            Assert.NotEmpty(assemblies);
            Assert.All(assemblies, assembly => Assert.NotNull(assembly));
            
            TestOutput.WriteLine($"Build configuration: {buildConfig}");
            TestOutput.WriteLine($"Successfully loaded {assemblies.Length} valid assemblies while skipping invalid ones");
        }
        finally
        {
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    [Fact]
    public void GetReportsResultsPath_ShouldReturnCorrectPath_WhenOptionsAreValid()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            Directory.CreateDirectory(tempPath);
            var artifactsPath = Path.Combine(tempPath, "artifacts");
            var config = ManualConfig.CreateEmpty().WithArtifactsPath(artifactsPath);
            var options = new BenchmarkWorkspaceOptions
            {
                RepositoryPath = tempPath,
                Configuration = config
            };

            // Act
            var resultsPath = BenchmarkWorkspace.GetReportsResultsPath(options);

            // Assert
            var expectedPath = Path.Combine(artifactsPath, "results");
            Assert.Equal(expectedPath, resultsPath);

            TestOutput.WriteLine($"Results path: {resultsPath}");
        }
        finally
        {
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    [Fact]
    public void GetReportsResultsPath_ShouldThrowArgumentException_WhenOptionsAreInvalid()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions
        {
            RepositoryPath = null
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => BenchmarkWorkspace.GetReportsResultsPath(options));
    }

    [Fact]
    public void GetReportsResultsPath_ShouldThrowArgumentNullException_WhenOptionsIsNull()
    {
        // Arrange
        BenchmarkWorkspaceOptions options = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => BenchmarkWorkspace.GetReportsResultsPath(options));
    }

    [Fact]
    public void GetReportsTuningPath_ShouldReturnCorrectPath_WhenOptionsAreValid()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            Directory.CreateDirectory(tempPath);
            var artifactsPath = Path.Combine(tempPath, "artifacts");
            var config = ManualConfig.CreateEmpty().WithArtifactsPath(artifactsPath);
            var options = new BenchmarkWorkspaceOptions
            {
                RepositoryPath = tempPath,
                Configuration = config,
                RepositoryTuningFolder = "tuning"
            };

            // Act
            var tuningPath = BenchmarkWorkspace.GetReportsTuningPath(options);

            // Assert
            var expectedPath = Path.Combine(artifactsPath, "tuning");
            Assert.Equal(expectedPath, tuningPath);

            TestOutput.WriteLine($"Tuning path: {tuningPath}");
        }
        finally
        {
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    [Fact]
    public void GetReportsTuningPath_ShouldThrowArgumentException_WhenOptionsAreInvalid()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions
        {
            RepositoryPath = null
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => BenchmarkWorkspace.GetReportsTuningPath(options));
    }

    [Fact]
    public void GetReportsTuningPath_ShouldThrowArgumentNullException_WhenOptionsIsNull()
    {
        // Arrange
        BenchmarkWorkspaceOptions options = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => BenchmarkWorkspace.GetReportsTuningPath(options));
    }

    [Fact]
    public void GetReportsTuningPath_ShouldUseCustomTuningFolder_WhenSpecified()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            Directory.CreateDirectory(tempPath);
            var artifactsPath = Path.Combine(tempPath, "artifacts");
            var customTuningFolder = "custom-tuning";
            var config = ManualConfig.CreateEmpty().WithArtifactsPath(artifactsPath);
            var options = new BenchmarkWorkspaceOptions
            {
                RepositoryPath = tempPath,
                Configuration = config,
                RepositoryTuningFolder = customTuningFolder
            };

            // Act
            var tuningPath = BenchmarkWorkspace.GetReportsTuningPath(options);

            // Assert
            var expectedPath = Path.Combine(artifactsPath, customTuningFolder);
            Assert.Equal(expectedPath, tuningPath);
            Assert.Contains(customTuningFolder, tuningPath);

            TestOutput.WriteLine($"Custom tuning path: {tuningPath}");
        }
        finally
        {
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }
    }

    [Fact]
    public void GetReportsResultsPath_AndGetReportsTuningPath_ShouldReturnDifferentPaths()
    {
        // Arrange
        var options = new BenchmarkWorkspaceOptions
        {
            AllowDebugBuild = IsDebugBuild
        };

        // Act
        var resultsPath = BenchmarkWorkspace.GetReportsResultsPath(options);
        var tuningPath = BenchmarkWorkspace.GetReportsTuningPath(options);

        // Assert
        Assert.NotEqual(resultsPath, tuningPath);
        Assert.Contains("results", resultsPath);
        Assert.Contains("tuning", tuningPath);

        TestOutput.WriteLine($"Results path: {resultsPath}");
        TestOutput.WriteLine($"Tuning path: {tuningPath}");
    }
}
