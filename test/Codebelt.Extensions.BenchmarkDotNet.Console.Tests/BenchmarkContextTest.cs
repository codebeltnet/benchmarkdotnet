using Codebelt.Extensions.Xunit;
using Xunit;

namespace Codebelt.Extensions.BenchmarkDotNet.Console;

public class BenchmarkContextTest : Test
{
    public BenchmarkContextTest(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Constructor_ShouldInitializeWithEmptyArray_WhenArgsIsNull()
    {
        // Act
        var context = new BenchmarkContext(null);

        // Assert
        Assert.NotNull(context.Args);
        Assert.Empty(context.Args);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithEmptyArray_WhenArgsIsEmpty()
    {
        // Arrange
        var emptyArgs = new string[] { };

        // Act
        var context = new BenchmarkContext(emptyArgs);

        // Assert
        Assert.NotNull(context.Args);
        Assert.Empty(context.Args);
        Assert.Same(emptyArgs, context.Args);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithProvidedArgs_WhenArgsIsNotEmpty()
    {
        // Arrange
        var args = new[] { "arg1", "arg2", "arg3" };

        // Act
        var context = new BenchmarkContext(args);

        // Assert
        Assert.NotNull(context.Args);
        Assert.Equal(3, context.Args.Length);
        Assert.Same(args, context.Args);
    }

    [Fact]
    public void Args_ShouldReturnOriginalArray_WhenProvided()
    {
        // Arrange
        var args = new[] { "--filter", "MyBenchmark", "--job", "short" };
        var context = new BenchmarkContext(args);

        // Act
        var result = context.Args;

        // Assert
        Assert.Same(args, result);
        Assert.Equal(4, result.Length);
        Assert.Equal("--filter", result[0]);
        Assert.Equal("MyBenchmark", result[1]);
        Assert.Equal("--job", result[2]);
        Assert.Equal("short", result[3]);
    }

    [Fact]
    public void Args_ShouldBeReadOnly_ButArrayContentsAreMutable()
    {
        // Arrange
        var args = new[] { "original" };
        var context = new BenchmarkContext(args);

        // Act - Modify the original array
        args[0] = "modified";

        // Assert - The property reflects the change (no defensive copy)
        Assert.Equal("modified", context.Args[0]);
    }

    [Fact]
    public void Constructor_ShouldHandleSingleArgument()
    {
        // Arrange
        var args = new[] { "single-arg" };

        // Act
        var context = new BenchmarkContext(args);

        // Assert
        Assert.NotNull(context.Args);
        Assert.Single(context.Args);
        Assert.Equal("single-arg", context.Args[0]);
    }

    [Fact]
    public void Constructor_ShouldHandleArgsWithSpecialCharacters()
    {
        // Arrange
        var args = new[] { "--config", "Debug", "--output", "C:\\Reports\\benchmark-results.txt" };

        // Act
        var context = new BenchmarkContext(args);

        // Assert
        Assert.NotNull(context.Args);
        Assert.Equal(4, context.Args.Length);
        Assert.Equal("--config", context.Args[0]);
        Assert.Equal("Debug", context.Args[1]);
        Assert.Equal("--output", context.Args[2]);
        Assert.Equal("C:\\Reports\\benchmark-results.txt", context.Args[3]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void Constructor_ShouldHandleVariousArrayLengths(int length)
    {
        // Arrange
        var args = new string[length];
        for (int i = 0; i < length; i++)
        {
            args[i] = $"arg{i}";
        }

        // Act
        var context = new BenchmarkContext(args);

        // Assert
        Assert.NotNull(context.Args);
        Assert.Equal(length, context.Args.Length);

        TestOutput.WriteLine($"Created context with {length} arguments");
    }

    [Fact]
    public void Constructor_ShouldHandleArgsWithEmptyStrings()
    {
        // Arrange
        var args = new[] { "", "valid", "" };

        // Act
        var context = new BenchmarkContext(args);

        // Assert
        Assert.NotNull(context.Args);
        Assert.Equal(3, context.Args.Length);
        Assert.Equal("", context.Args[0]);
        Assert.Equal("valid", context.Args[1]);
        Assert.Equal("", context.Args[2]);
    }

    [Fact]
    public void Constructor_ShouldHandleArgsWithWhitespace()
    {
        // Arrange
        var args = new[] { "   ", "valid", "\t\t" };

        // Act
        var context = new BenchmarkContext(args);

        // Assert
        Assert.NotNull(context.Args);
        Assert.Equal(3, context.Args.Length);
        Assert.Equal("   ", context.Args[0]);
        Assert.Equal("valid", context.Args[1]);
        Assert.Equal("\t\t", context.Args[2]);
    }

    [Fact]
    public void Args_ShouldReturnEmptyArray_AfterConstructorWithNull()
    {
        // Arrange
        var context = new BenchmarkContext(null);

        // Act
        var args = context.Args;

        // Assert
        Assert.NotNull(args);
        Assert.Empty(args);
        Assert.IsType<string[]>(args);
    }

    [Fact]
    public void Constructor_ShouldHandleTypicalBenchmarkDotNetArgs()
    {
        // Arrange
        var args = new[]
        {
            "--filter",
            "Codebelt.Extensions.BenchmarkDotNet.*",
            "--job",
            "short",
            "--exporters",
            "markdown",
            "--memory"
        };

        // Act
        var context = new BenchmarkContext(args);

        // Assert
        Assert.NotNull(context.Args);
        Assert.Equal(7, context.Args.Length);
        Assert.Equal("--filter", context.Args[0]);
        Assert.Equal("Codebelt.Extensions.BenchmarkDotNet.*", context.Args[1]);
        Assert.Equal("--job", context.Args[2]);
        Assert.Equal("short", context.Args[3]);
        Assert.Equal("--exporters", context.Args[4]);
        Assert.Equal("markdown", context.Args[5]);
        Assert.Equal("--memory", context.Args[6]);

        TestOutput.WriteLine($"Successfully created context with {context.Args.Length} BenchmarkDotNet arguments");
    }
}
