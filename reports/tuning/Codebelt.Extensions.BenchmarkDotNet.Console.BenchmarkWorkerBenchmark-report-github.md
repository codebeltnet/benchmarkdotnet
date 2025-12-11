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
| Method                            | Runtime   | Mean          | Error       | StdDev      | Median        | Min           | Max           | Ratio  | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
|---------------------------------- |---------- |--------------:|------------:|------------:|--------------:|--------------:|--------------:|-------:|--------:|-------:|-------:|----------:|------------:|
| &#39;Construct BenchmarkWorker&#39;       | .NET 10.0 |     4.5280 ns |   0.2018 ns |   0.2324 ns |     4.5227 ns |     4.1448 ns |     5.0388 ns |   1.00 |    0.07 | 0.0020 |      - |      32 B |        1.00 |
| &#39;Configure services&#39;              | .NET 10.0 |    67.0518 ns |   2.4825 ns |   2.6562 ns |    66.3017 ns |    64.0598 ns |    73.2110 ns |  14.84 |    0.93 | 0.0395 |      - |     624 B |       19.50 |
| &#39;Configure services with options&#39; | .NET 10.0 | 1,310.1326 ns |  35.2009 ns |  39.1257 ns | 1,292.2983 ns | 1,252.4459 ns | 1,383.2906 ns | 290.06 |   16.68 | 0.5072 | 0.0961 |    7976 B |      249.25 |
| &#39;Access configuration&#39;            | .NET 10.0 |     0.6325 ns |   0.0491 ns |   0.0435 ns |     0.6303 ns |     0.5750 ns |     0.7115 ns |   0.14 |    0.01 |      - |      - |         - |        0.00 |
| &#39;Access environment&#39;              | .NET 10.0 |     0.6581 ns |   0.0604 ns |   0.0535 ns |     0.6514 ns |     0.5793 ns |     0.7540 ns |   0.15 |    0.01 |      - |      - |         - |        0.00 |
|                                   |           |               |             |             |               |               |               |        |         |        |        |           |             |
| &#39;Construct BenchmarkWorker&#39;       | .NET 9.0  |     4.5442 ns |   0.1618 ns |   0.1799 ns |     4.4841 ns |     4.2818 ns |     4.8308 ns |   1.00 |    0.05 | 0.0020 |      - |      32 B |        1.00 |
| &#39;Configure services&#39;              | .NET 9.0  |    78.0880 ns |   1.6229 ns |   1.8038 ns |    78.4799 ns |    75.0539 ns |    81.4879 ns |  17.21 |    0.76 | 0.0396 |      - |     624 B |       19.50 |
| &#39;Configure services with options&#39; | .NET 9.0  | 1,827.5207 ns | 215.5665 ns | 248.2466 ns | 1,916.0051 ns | 1,398.1489 ns | 2,121.1524 ns | 402.76 |   55.60 | 0.5046 | 0.1235 |    7944 B |      248.25 |
| &#39;Access configuration&#39;            | .NET 9.0  |     0.6918 ns |   0.0668 ns |   0.0714 ns |     0.6700 ns |     0.6141 ns |     0.8275 ns |   0.15 |    0.02 |      - |      - |         - |        0.00 |
| &#39;Access environment&#39;              | .NET 9.0  |     0.6554 ns |   0.0526 ns |   0.0492 ns |     0.6580 ns |     0.5846 ns |     0.7320 ns |   0.14 |    0.01 |      - |      - |         - |        0.00 |
