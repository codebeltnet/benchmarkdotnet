namespace Codebelt.Extensions.BenchmarkDotNet.Console;

/// <summary>
/// Represents the command-line context for a benchmark run.
/// </summary>
public class BenchmarkContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BenchmarkContext"/> class.
    /// </summary>
    /// <param name="args">The command-line arguments passed to the <see cref="BenchmarkProgram"/>.</param>
    public BenchmarkContext(string[] args)
    {
        Args = args ?? [];
    }

    /// <summary>
    /// Gets the command-line arguments passed to the <see cref="BenchmarkProgram"/>.
    /// </summary>
    /// <value>The command-line arguments.</value>
    public string[] Args { get; }
}
