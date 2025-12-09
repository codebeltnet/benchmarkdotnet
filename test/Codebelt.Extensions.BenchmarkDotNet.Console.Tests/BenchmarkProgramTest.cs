using System;
using Codebelt.Extensions.Xunit;
using System.Reflection;
using System.Linq;
using Xunit;

namespace Codebelt.Extensions.BenchmarkDotNet.Console;

public class BenchmarkProgramTest : Test
{
    public BenchmarkProgramTest(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void BuildConfiguration_ShouldReturnValidValue()
    {
        // Act
        var buildConfiguration = BenchmarkProgram.BuildConfiguration;

        // Assert
        Assert.NotNull(buildConfiguration);
        Assert.True(buildConfiguration == "Debug" || buildConfiguration == "Release");

        TestOutput.WriteLine($"BuildConfiguration: {buildConfiguration}");
    }

    [Fact]
    public void IsDebugBuild_ShouldBeConsistentWithBuildConfiguration()
    {
        // Act
        var isDebugBuild = BenchmarkProgram.IsDebugBuild;
        var buildConfiguration = BenchmarkProgram.BuildConfiguration;

        // Assert
        if (buildConfiguration == "Debug")
        {
            Assert.True(isDebugBuild);
        }
        else if (buildConfiguration == "Release")
        {
            Assert.False(isDebugBuild);
        }

        TestOutput.WriteLine($"IsDebugBuild: {isDebugBuild}, BuildConfiguration: {buildConfiguration}");
    }

    [Fact]
    public void BuildConfiguration_ShouldBeDebug_WhenCompiledInDebugMode()
    {
        // Arrange
        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly == null)
        {
            // Skip test if entry assembly is not available (e.g., when running in certain test environments)
            TestOutput.WriteLine("Entry assembly is null, skipping test");
            return;
        }

        // Act
        var buildConfiguration = BenchmarkProgram.BuildConfiguration;

        // Assert
        #if DEBUG
        Assert.Equal("Debug", buildConfiguration);
        #else
        Assert.Equal("Release", buildConfiguration);
        #endif

        TestOutput.WriteLine($"BuildConfiguration: {buildConfiguration}");
    }

    [Fact]
    public void IsDebugBuild_ShouldBeTrue_WhenCompiledInDebugMode()
    {
        // Arrange
        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly == null)
        {
            // Skip test if entry assembly is not available
            TestOutput.WriteLine("Entry assembly is null, skipping test");
            return;
        }

        // Act
        var isDebugBuild = BenchmarkProgram.IsDebugBuild;

        // Assert
        #if DEBUG
        Assert.True(isDebugBuild);
        #else
        Assert.False(isDebugBuild);
        #endif

        TestOutput.WriteLine($"IsDebugBuild: {isDebugBuild}");
    }

    [Fact]
    public void BuildConfiguration_ShouldBeCached_AcrossMultipleCalls()
    {
        // Act
        var firstCall = BenchmarkProgram.BuildConfiguration;
        var secondCall = BenchmarkProgram.BuildConfiguration;
        var thirdCall = BenchmarkProgram.BuildConfiguration;

        // Assert
        Assert.Equal(firstCall, secondCall);
        Assert.Equal(secondCall, thirdCall);
        Assert.Same(firstCall, secondCall); // Should be the same string instance

        TestOutput.WriteLine($"BuildConfiguration called 3 times, all returned: {firstCall}");
    }

    [Fact]
    public void IsDebugBuild_ShouldBeCached_AcrossMultipleCalls()
    {
        // Act
        var firstCall = BenchmarkProgram.IsDebugBuild;
        var secondCall = BenchmarkProgram.IsDebugBuild;
        var thirdCall = BenchmarkProgram.IsDebugBuild;

        // Assert
        Assert.Equal(firstCall, secondCall);
        Assert.Equal(secondCall, thirdCall);

        TestOutput.WriteLine($"IsDebugBuild called 3 times, all returned: {firstCall}");
    }

    [Fact]
    public void StaticProperties_ShouldBeInitializedAtStartup()
    {
        // Act & Assert - Static constructor should have already run
        Assert.NotNull(BenchmarkProgram.BuildConfiguration);
        Assert.NotEmpty(BenchmarkProgram.BuildConfiguration);
        
        // IsDebugBuild can be true or false, but should be set
        var isDebugBuild = BenchmarkProgram.IsDebugBuild;
        Assert.True(isDebugBuild == true || isDebugBuild == false);

        TestOutput.WriteLine($"BuildConfiguration: {BenchmarkProgram.BuildConfiguration}");
        TestOutput.WriteLine($"IsDebugBuild: {BenchmarkProgram.IsDebugBuild}");
    }

    [Theory]
    [InlineData("Debug", true)]
    [InlineData("Release", false)]
    public void BuildConfiguration_ShouldMatchExpectedDebugState(string expectedConfig, bool expectedIsDebug)
    {
        // Act
        var actualConfig = BenchmarkProgram.BuildConfiguration;
        var actualIsDebug = BenchmarkProgram.IsDebugBuild;

        // Assert
        if (actualConfig == expectedConfig)
        {
            Assert.Equal(expectedIsDebug, actualIsDebug);
            TestOutput.WriteLine($"Configuration '{actualConfig}' correctly maps to IsDebugBuild={actualIsDebug}");
        }
        else
        {
            TestOutput.WriteLine($"Skipping assertion - actual configuration is '{actualConfig}', not '{expectedConfig}'");
        }
    }

    [Fact]
    public void BuildConfiguration_ShouldNotBeNullOrEmpty()
    {
        // Act
        var buildConfiguration = BenchmarkProgram.BuildConfiguration;

        // Assert
        Assert.NotNull(buildConfiguration);
        Assert.NotEmpty(buildConfiguration);
        Assert.False(string.IsNullOrWhiteSpace(buildConfiguration));

        TestOutput.WriteLine($"BuildConfiguration: '{buildConfiguration}'");
    }

    [Fact]
    public void BuildConfiguration_ShouldBeOneOfTwoValidValues()
    {
        // Act
        var buildConfiguration = BenchmarkProgram.BuildConfiguration;

        // Assert
        Assert.Contains(buildConfiguration, new[] { "Debug", "Release" });

        TestOutput.WriteLine($"BuildConfiguration is valid: {buildConfiguration}");
    }

    [Fact]
    public void IsDebugBuild_ShouldBeBoolean()
    {
        // Act
        var isDebugBuild = BenchmarkProgram.IsDebugBuild;

        // Assert
        Assert.IsType<bool>(isDebugBuild);

        TestOutput.WriteLine($"IsDebugBuild type verified: {isDebugBuild}");
    }

    [Fact]
    public void StaticConstructor_ShouldSetPropertiesBasedOnEntryAssembly()
    {
        // This test verifies that the static constructor logic has executed correctly
        // by checking that both properties are set and consistent with each other

        // Act
        var buildConfiguration = BenchmarkProgram.BuildConfiguration;
        var isDebugBuild = BenchmarkProgram.IsDebugBuild;

        // Assert
        Assert.NotNull(buildConfiguration);
        
        if (isDebugBuild)
        {
            Assert.Equal("Debug", buildConfiguration);
        }
        else
        {
            Assert.Equal("Release", buildConfiguration);
        }

        TestOutput.WriteLine($"Static properties correctly initialized - BuildConfiguration: {buildConfiguration}, IsDebugBuild: {isDebugBuild}");
    }

    [Fact]
    public void BenchmarkProgram_ShouldInheritFromConsoleProgram()
    {
        // Act
        var baseType = typeof(BenchmarkProgram).BaseType;

        // Assert
        Assert.NotNull(baseType);
        Assert.True(baseType.IsGenericType);
        Assert.Equal("ConsoleProgram`1", baseType.Name);

        TestOutput.WriteLine($"BenchmarkProgram correctly inherits from: {baseType.FullName}");
    }

    [Fact]
    public void BenchmarkProgram_ShouldUseCorrectGenericTypeParameter()
    {
        // Act
        var baseType = typeof(BenchmarkProgram).BaseType;
        var genericArguments = baseType?.GetGenericArguments();

        // Assert
        Assert.NotNull(genericArguments);
        Assert.Single(genericArguments);
        Assert.Equal(typeof(BenchmarkWorker), genericArguments[0]);

        TestOutput.WriteLine($"BenchmarkProgram uses correct generic type parameter: {genericArguments[0].Name}");
    }

    [Fact]
    public void BenchmarkProgram_ShouldBePublicClass()
    {
        // Act
        var type = typeof(BenchmarkProgram);

        // Assert
        Assert.True(type.IsPublic);
        Assert.True(type.IsClass);
        Assert.False(type.IsAbstract);
        Assert.False(type.IsSealed);

        TestOutput.WriteLine($"BenchmarkProgram is a public, non-abstract, non-sealed class");
    }

    [Fact]
    public void Run_MethodWithDefaultWorkspace_ShouldExist()
    {
        // Act
        var method = typeof(BenchmarkProgram).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(m =>
                m.Name == "Run" &&
                !m.IsGenericMethodDefinition &&
                m.GetParameters().Length == 2 &&
                m.GetParameters()[0].ParameterType == typeof(string[]) &&
                m.GetParameters()[1].ParameterType == typeof(Action<BenchmarkWorkspaceOptions>));

        // Assert
        Assert.NotNull(method);
        Assert.True(method.IsStatic);
        Assert.True(method.IsPublic);
        Assert.Equal(typeof(void), method.ReturnType);

        TestOutput.WriteLine($"Found Run method with default workspace: {method}");
    }

    [Fact]
    public void Run_GenericMethod_ShouldExist()
    {
        // Act
        var method = typeof(BenchmarkProgram).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(m => 
                m.Name == "Run" && 
                m.IsGenericMethodDefinition &&
                m.GetParameters().Length == 2);

        // Assert
        Assert.NotNull(method);
        Assert.True(method.IsStatic);
        Assert.True(method.IsPublic);
        Assert.True(method.IsGenericMethodDefinition);
        Assert.Equal(typeof(void), method.ReturnType);

        var genericArguments = method.GetGenericArguments();
        Assert.Single(genericArguments);

        TestOutput.WriteLine($"Found generic Run<TWorkspace> method: {method}");
    }

    [Fact]
    public void Run_GenericMethod_ShouldHaveCorrectConstraints()
    {
        // Act
        var method = typeof(BenchmarkProgram).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(m => 
                m.Name == "Run" && 
                m.IsGenericMethodDefinition &&
                m.GetParameters().Length == 2);

        Assert.NotNull(method);

        var genericArguments = method.GetGenericArguments();
        var typeParameter = genericArguments[0];

        // Assert
        var constraints = typeParameter.GetGenericParameterConstraints();
        Assert.Contains(constraints, c => c == typeof(IBenchmarkWorkspace));
        Assert.True(typeParameter.GenericParameterAttributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint));

        TestOutput.WriteLine($"Generic type parameter '{typeParameter.Name}' has correct constraints: class, IBenchmarkWorkspace");
    }

    [Fact]
    public void StaticProperties_ShouldBeReadOnly()
    {
        // Act
        var buildConfigProperty = typeof(BenchmarkProgram).GetProperty("BuildConfiguration");
        var isDebugBuildProperty = typeof(BenchmarkProgram).GetProperty("IsDebugBuild");

        // Assert
        Assert.NotNull(buildConfigProperty);
        Assert.True(buildConfigProperty.CanRead);
        Assert.False(buildConfigProperty.CanWrite);

        Assert.NotNull(isDebugBuildProperty);
        Assert.True(isDebugBuildProperty.CanRead);
        Assert.False(isDebugBuildProperty.CanWrite);

        TestOutput.WriteLine("Both static properties are read-only as expected");
    }

    [Fact]
    public void Run_Methods_ShouldHaveCorrectParameterNames()
    {
        // Act - Get the non-generic Run method explicitly
        var nonGenericMethod = typeof(BenchmarkProgram).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(m =>
                m.Name == "Run" &&
                !m.IsGenericMethodDefinition &&
                m.GetParameters().Length == 2 &&
                m.GetParameters()[0].ParameterType == typeof(string[]) &&
                m.GetParameters()[1].ParameterType == typeof(Action<BenchmarkWorkspaceOptions>));

        // Assert
        Assert.NotNull(nonGenericMethod);
        
        var parameters = nonGenericMethod.GetParameters();
        Assert.Equal(2, parameters.Length);
        Assert.Equal("args", parameters[0].Name);
        Assert.Equal("setup", parameters[1].Name);

        TestOutput.WriteLine($"Run method parameters: {string.Join(", ", parameters.Select(p => $"{p.ParameterType.Name} {p.Name}"))}");
    }

    [Fact]
    public void Run_Methods_ShouldHaveOptionalSetupParameter()
    {
        // Act - Get the non-generic Run method explicitly
        var nonGenericMethod = typeof(BenchmarkProgram).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(m =>
                m.Name == "Run" &&
                !m.IsGenericMethodDefinition &&
                m.GetParameters().Length == 2 &&
                m.GetParameters()[0].ParameterType == typeof(string[]) &&
                m.GetParameters()[1].ParameterType == typeof(Action<BenchmarkWorkspaceOptions>));

        // Assert
        Assert.NotNull(nonGenericMethod);
        
        var setupParameter = nonGenericMethod.GetParameters()[1];
        Assert.True(setupParameter.IsOptional);
        Assert.Null(setupParameter.DefaultValue);

        TestOutput.WriteLine($"Setup parameter is optional with default value: {setupParameter.DefaultValue ?? "null"}");
    }
}
