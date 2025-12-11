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
| Method                                      | Runtime   | Mean          | Error       | StdDev      | Median        | Min           | Max           | Ratio     | RatioSD   | Gen0   | Allocated | Alloc Ratio |
|-------------------------------------------- |---------- |--------------:|------------:|------------:|--------------:|--------------:|--------------:|----------:|----------:|-------:|----------:|------------:|
| &#39;Access BuildConfiguration property&#39;        | .NET 10.0 |     0.0610 ns |   0.0436 ns |   0.0386 ns |     0.0570 ns |     0.0108 ns |     0.1273 ns |      1.72 |      2.01 |      - |         - |          NA |
| &#39;Access IsDebugBuild property&#39;              | .NET 10.0 |     0.0048 ns |   0.0127 ns |   0.0106 ns |     0.0000 ns |     0.0000 ns |     0.0356 ns |      0.13 |      0.40 |      - |         - |          NA |
| &#39;Check assembly debug build status&#39;         | .NET 10.0 | 2,062.5568 ns | 227.9485 ns | 243.9024 ns | 1,989.5074 ns | 1,820.6402 ns | 2,634.0309 ns | 58,090.94 | 50,285.20 | 0.5438 |    8793 B |          NA |
| &#39;Resolve build configuration from assembly&#39; | .NET 10.0 | 2,098.8584 ns | 223.0681 ns | 238.6804 ns | 1,976.7257 ns | 1,880.0177 ns | 2,626.4587 ns | 59,113.36 | 51,111.52 | 0.5473 |    8793 B |          NA |
| &#39;Check entry assembly debug build status&#39;   | .NET 10.0 | 2,150.8535 ns |  68.8797 ns |  67.6491 ns | 2,123.1321 ns | 2,072.9937 ns | 2,304.4094 ns | 60,577.77 | 51,701.90 | 0.5289 |    8529 B |          NA |
| &#39;Static property access pattern&#39;            | .NET 10.0 |     0.0582 ns |   0.0507 ns |   0.0543 ns |     0.0508 ns |     0.0000 ns |     0.1548 ns |      1.64 |      2.40 |      - |         - |          NA |
|                                             |           |               |             |             |               |               |               |           |           |        |           |             |
| &#39;Access BuildConfiguration property&#39;        | .NET 9.0  |     0.0260 ns |   0.0296 ns |   0.0277 ns |     0.0210 ns |     0.0000 ns |     0.0874 ns |         ? |         ? |      - |         - |           ? |
| &#39;Access IsDebugBuild property&#39;              | .NET 9.0  |     0.0312 ns |   0.0285 ns |   0.0292 ns |     0.0331 ns |     0.0000 ns |     0.1032 ns |         ? |         ? |      - |         - |           ? |
| &#39;Check assembly debug build status&#39;         | .NET 9.0  | 2,434.8425 ns | 197.4947 ns | 219.5148 ns | 2,389.7296 ns | 2,216.3000 ns | 3,040.9758 ns |         ? |         ? | 0.5510 |    8825 B |           ? |
| &#39;Resolve build configuration from assembly&#39; | .NET 9.0  | 2,256.3108 ns | 122.3223 ns | 130.8835 ns | 2,195.9803 ns | 2,166.4730 ns | 2,592.2347 ns |         ? |         ? | 0.5484 |    8793 B |           ? |
| &#39;Check entry assembly debug build status&#39;   | .NET 9.0  | 2,321.5775 ns | 155.7788 ns | 166.6816 ns | 2,229.0164 ns | 2,149.6204 ns | 2,659.6258 ns |         ? |         ? | 0.5423 |    8561 B |           ? |
| &#39;Static property access pattern&#39;            | .NET 9.0  |     0.2924 ns |   0.0498 ns |   0.0466 ns |     0.2807 ns |     0.2035 ns |     0.3729 ns |         ? |         ? |      - |         - |           ? |
