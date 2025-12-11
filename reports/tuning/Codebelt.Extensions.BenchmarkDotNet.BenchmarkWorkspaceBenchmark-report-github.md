```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.7462/25H2/2025Update/HudsonValley2)
12th Gen Intel Core i9-12900KF 3.20GHz, 1 CPU, 24 logical and 16 physical cores
.NET SDK 10.0.101
  [Host]     : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v3
  Job-LDLMHG : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v3
  Job-IOAYXE : .NET 9.0.11 (9.0.11, 9.0.1125.51716), X64 RyuJIT x86-64-v3

PowerPlanMode=00000000-0000-0000-0000-000000000000  IterationTime=250ms  MaxIterationCount=20  
MinIterationCount=15  WarmupCount=1  

```
| Method                                                        | Runtime   | Mean       | Error      | StdDev     | Median     | Min        | Max        | Ratio  | RatioSD | Gen0   | Allocated | Alloc Ratio |
|-------------------------------------------------------------- |---------- |-----------:|-----------:|-----------:|-----------:|-----------:|-----------:|-------:|--------:|-------:|----------:|------------:|
| &#39;Construct BenchmarkDotNetWorkspace&#39;                          | .NET 10.0 |   2.162 μs |  0.0289 μs |  0.0241 μs |   2.162 μs |   2.131 μs |   2.213 μs |   1.00 |    0.02 | 0.2678 |    4248 B |        1.00 |
| &#39;Load assemblies from tuning folder (no matching assemblies)&#39; | .NET 10.0 | 434.051 μs | 32.0943 μs | 36.9599 μs | 422.094 μs | 395.167 μs | 523.232 μs | 200.80 |   16.84 |      - |   15913 B |        3.75 |
| &#39;PostProcessArtifacts (move results -&gt; tuning folder)&#39;        | .NET 10.0 |  10.441 μs |  0.2018 μs |  0.2073 μs |  10.398 μs |  10.196 μs |  10.922 μs |   4.83 |    0.11 |      - |     392 B |        0.09 |
|                                                               |           |            |            |            |            |            |            |        |         |        |           |             |
| &#39;Construct BenchmarkDotNetWorkspace&#39;                          | .NET 9.0  |   3.030 μs |  0.0734 μs |  0.0815 μs |   3.055 μs |   2.885 μs |   3.190 μs |   1.00 |    0.04 | 0.2628 |    4272 B |        1.00 |
| &#39;Load assemblies from tuning folder (no matching assemblies)&#39; | .NET 9.0  | 422.517 μs | 13.7315 μs | 15.8133 μs | 416.600 μs | 405.125 μs | 460.289 μs | 139.56 |    6.30 |      - |   15817 B |        3.70 |
| &#39;PostProcessArtifacts (move results -&gt; tuning folder)&#39;        | .NET 9.0  |  10.349 μs |  0.2326 μs |  0.2585 μs |  10.293 μs |   9.973 μs |  10.872 μs |   3.42 |    0.12 |      - |     392 B |        0.09 |
