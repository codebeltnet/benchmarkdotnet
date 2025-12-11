using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

namespace Codebelt.Extensions.BenchmarkDotNet.Console;

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class BenchmarkContextBenchmark
{
    private string[] _emptyArgs;
    private string[] _smallArgs;
    private string[] _mediumArgs;
    private string[] _largeArgs;

    [Params(0, 8, 64, 256)]
    public int ArgsCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        // Deterministic initialization of test data
        _emptyArgs = [];
        
        _smallArgs = new string[8];
        for (int i = 0; i < _smallArgs.Length; i++)
        {
            _smallArgs[i] = $"--arg{i}";
        }

        _mediumArgs = new string[64];
        for (int i = 0; i < _mediumArgs.Length; i++)
        {
            _mediumArgs[i] = $"--option{i}";
        }

        _largeArgs = new string[256];
        for (int i = 0; i < _largeArgs.Length; i++)
        {
            _largeArgs[i] = $"--parameter{i}=value{i}";
        }
    }

    [Benchmark(Baseline = true, Description = "Construct BenchmarkContext with empty args")]
    public BenchmarkContext ConstructWithEmptyArgs()
    {
        return new BenchmarkContext(_emptyArgs);
    }

    [Benchmark(Description = "Construct BenchmarkContext with null args")]
    public BenchmarkContext ConstructWithNullArgs()
    {
        return new BenchmarkContext(null);
    }

    [Benchmark(Description = "Construct BenchmarkContext with varied args count")]
    public BenchmarkContext ConstructWithVariedArgs()
    {
        return ArgsCount switch
        {
            0 => new BenchmarkContext(_emptyArgs),
            8 => new BenchmarkContext(_smallArgs),
            64 => new BenchmarkContext(_mediumArgs),
            256 => new BenchmarkContext(_largeArgs),
            _ => new BenchmarkContext(_emptyArgs)
        };
    }

    [Benchmark(Description = "Access Args property")]
    public string[] AccessArgsProperty()
    {
        var context = new BenchmarkContext(_smallArgs);
        return context.Args;
    }
}
