```

BenchmarkDotNet v0.15.6, Windows 11 (10.0.26200.7309)
12th Gen Intel Core i9-12900KF 3.20GHz, 1 CPU, 24 logical and 16 physical cores
.NET SDK 10.0.100
  [Host]     : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v3
  Job-LDLMHG : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v3
  Job-IOAYXE : .NET 9.0.11 (9.0.11, 9.0.1125.51716), X64 RyuJIT x86-64-v3

PowerPlanMode=00000000-0000-0000-0000-000000000000  IterationTime=250ms  MaxIterationCount=20  
MinIterationCount=15  WarmupCount=1  

```
| Method                                                        | Runtime   | Mean       | Error      | StdDev     | Median     | Min        | Max        | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------------------------------------------- |---------- |-----------:|-----------:|-----------:|-----------:|-----------:|-----------:|-------:|--------:|-------:|----------:|------------:|
| &#39;Construct BenchmarkDotNetWorkspace&#39;                          | .NET 10.0 |   2.163 μs |  0.0318 μs |  0.0282 μs |   2.166 μs |   2.125 μs |   2.206 μs |   1.00 |    0.02 | 0.2659 |    4248 B |        1.00 |
| &#39;Load assemblies from tuning folder (no matching assemblies)&#39; | .NET 10.0 | 406.714 μs | 17.1587 μs | 19.7600 μs | 409.632 μs | 386.064 μs | 455.586 μs | 188.03 |    9.23 |      - |   15913 B |        3.75 |
| &#39;PostProcessArtifacts (move results -&gt; tuning folder)&#39;        | .NET 10.0 |  10.829 μs |  0.2468 μs |  0.2743 μs |  10.775 μs |  10.421 μs |  11.538 μs |   5.01 |    0.14 |      - |     392 B |        0.09 |
|                                                               |           |            |            |            |            |            |            |        |         |        |           |             |
| &#39;Construct BenchmarkDotNetWorkspace&#39;                          | .NET 9.0  |   3.041 μs |  0.0650 μs |  0.0749 μs |   3.053 μs |   2.937 μs |   3.181 μs |   1.00 |    0.03 | 0.2656 |    4272 B |        1.00 |
| &#39;Load assemblies from tuning folder (no matching assemblies)&#39; | .NET 9.0  | 400.892 μs | 15.9646 μs | 18.3849 μs | 398.322 μs | 380.181 μs | 441.577 μs | 131.89 |    6.70 |      - |   15817 B |        3.70 |
| &#39;PostProcessArtifacts (move results -&gt; tuning folder)&#39;        | .NET 9.0  |  10.772 μs |  0.2729 μs |  0.3143 μs |  10.744 μs |  10.320 μs |  11.353 μs |   3.54 |    0.13 |      - |     392 B |        0.09 |
