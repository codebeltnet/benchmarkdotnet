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

    [Fact]
    public void LoadBenchmarkAssemblies_ShouldSkipEmptyFileName_WhenDllHasNoStem()
    {
        // Arrange – place a file whose stem is empty so that Path.GetFileNameWithoutExtension returns ""
        // which exercises the IsNullOrEmpty(simpleName) guard in UpdateAssemblyLookup (lines 169-170).
        var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var buildConfig = IsDebugBuild ? "Debug" : "Release";
        try
        {
            Directory.CreateDirectory(tempPath);
            var tuningDir = Path.Combine(tempPath, "tuning");
            var buildDir = Path.Combine(tuningDir, "bin", buildConfig, "net10.0");
            Directory.CreateDirectory(buildDir);

            // A file named ".dll" has an empty stem – Path.GetFileNameWithoutExtension(".dll") == ""
            File.WriteAllText(Path.Combine(buildDir, ".dll"), "not-an-assembly");

            var options = new BenchmarkWorkspaceOptions
            {
                RepositoryPath = tempPath,
                RepositoryTuningFolder = "tuning",
                BenchmarkProjectSuffix = "Benchmarks",
                TargetFrameworkMoniker = "net10.0",
                AllowDebugBuild = IsDebugBuild
            };
            var workspace = new BenchmarkWorkspace(options);

            // Act – will throw because there are no real assemblies; that's expected
            var exception = Record.Exception(() => workspace.LoadBenchmarkAssemblies());

            // Assert – the important thing is that no unhandled exception from UpdateAssemblyLookup occurred
            Assert.IsType<InvalidOperationException>(exception);
            Assert.Contains("No assemblies were loaded", exception.Message);

            TestOutput.WriteLine("Empty-stem .dll file was silently skipped as expected.");
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
    public void PostProcessArtifacts_ShouldSkipBenchmarkRunFiles_WhenResultsDirContainsThem()
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

            // BenchmarkRun-prefixed files should NOT be moved
            File.WriteAllText(Path.Combine(resultsDir, "BenchmarkRun-20240101-120000.log"), "run log");
            // Regular report files SHOULD be moved
            File.WriteAllText(Path.Combine(resultsDir, "MyBenchmark-report.md"), "report");

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

            // Assert – the report file was moved but the BenchmarkRun log was NOT
            Assert.False(Directory.Exists(resultsDir), "results directory should be deleted");
            Assert.True(File.Exists(Path.Combine(tuningDir, "MyBenchmark-report.md")));
            Assert.False(File.Exists(Path.Combine(tuningDir, "BenchmarkRun-20240101-120000.log")));

            TestOutput.WriteLine("BenchmarkRun files correctly excluded from PostProcessArtifacts move.");
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
    public void LoadBenchmarkAssemblies_ShouldSkipDuplicateAssembly_WhenSameIdentityAppearsAtTwoPaths()
    {
            // This test exercises lines 141-142 (the duplicate-assembly-in-queue guard):
            //
            //   if (assemblies.Any(a => AssemblyName.ReferenceMatchesDefinition(a.GetName(), candidateName)))
            //   {
            //       continue;  // ← lines 141-142
            //   }
            //
            // The guard fires when:
            //  1. 'alreadyLoaded' (snapshot taken BEFORE the loop) does NOT contain the assembly,
            //  2. the loop adds path1 of the assembly to 'assemblies',
            //  3. the loop then encounters path2 with the SAME identity –
            //     'alreadyLoaded' still has no entry (same snapshot), so the first `if` is skipped,
            //     but `assemblies.Any(...)` is TRUE → duplicate guard fires.
            //
            // To guarantee the assembly is never in the initial AppDomain snapshot we generate a
            // brand-new assembly with a unique GUID-based name via PersistedAssemblyBuilder.

            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var buildConfig = IsDebugBuild ? "Debug" : "Release";
            try
            {
                // Create two subdirectories that both match the build segment
                var proj1Dir = Path.Combine(tempPath, "tuning", "proj1", "bin", buildConfig, "net10.0");
                var proj2Dir = Path.Combine(tempPath, "tuning", "proj2", "bin", buildConfig, "net10.0");
                Directory.CreateDirectory(proj1Dir);
                Directory.CreateDirectory(proj2Dir);

                // Build a minimal in-memory assembly with a guaranteed-unique name
                var uniqueId = Guid.NewGuid().ToString("N");
                var asmName = new AssemblyName($"Unique{uniqueId}.Benchmarks") { Version = new Version(1, 0, 0, 0) };

                var persistedBuilder = new System.Reflection.Emit.PersistedAssemblyBuilder(asmName, typeof(object).Assembly);
                persistedBuilder.DefineDynamicModule("main")
                    .DefineType("Placeholder", System.Reflection.TypeAttributes.Public | System.Reflection.TypeAttributes.Class)
                    .CreateType();

                // Save to path1, then copy to path2 so both carry the SAME assembly identity
                var fileName = $"Unique{uniqueId}.Benchmarks.dll";
                var path1 = Path.Combine(proj1Dir, fileName);
                var path2 = Path.Combine(proj2Dir, fileName);
                persistedBuilder.Save(path1);
                File.Copy(path1, path2);

                var options = new BenchmarkWorkspaceOptions
                {
                    RepositoryPath = tempPath,
                    RepositoryTuningFolder = "tuning",
                    BenchmarkProjectSuffix = "Benchmarks",
                    TargetFrameworkMoniker = "net10.0",
                    AllowDebugBuild = IsDebugBuild
                };
                var workspace = new BenchmarkWorkspace(options);

                // Act – both paths match, one is loaded, the second hits the duplicate guard
                var assemblies = workspace.LoadBenchmarkAssemblies();

                // Assert – only ONE assembly loaded despite two matching paths
                Assert.NotNull(assemblies);
                Assert.Equal(1, assemblies.Length);
                Assert.Contains(assemblies, a => a.GetName().Name == asmName.Name);

                TestOutput.WriteLine($"Loaded {assemblies.Length} assembly (duplicate suppressed): {assemblies[0].GetName().Name}");
            }
            finally
            {
                // Unload assemblies and release file locks before cleanup on Windows
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                // On Windows, loaded DLLs may still hold locks; retry deletion with delay
                if (Directory.Exists(tempPath))
                {
                    const int maxRetries = 5;
                    const int delayMs = 200;
                    var lastException = (Exception)null;
                    for (int i = 0; i < maxRetries; i++)
                    {
                        try
                        {
                            Directory.Delete(tempPath, true);
                            break;
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            lastException = ex;
                            if (i < maxRetries - 1)
                            {
                                System.Threading.Thread.Sleep(delayMs);
                            }
                        }
                    }
                    // If we still can't delete after retries, log but don't fail the test
                    // The OS will clean up the temp directory eventually
                    if (Directory.Exists(tempPath))
                    {
                        TestOutput.WriteLine($"Warning: Could not delete temp directory {tempPath}");
                    }
                }
            }
    }
}
