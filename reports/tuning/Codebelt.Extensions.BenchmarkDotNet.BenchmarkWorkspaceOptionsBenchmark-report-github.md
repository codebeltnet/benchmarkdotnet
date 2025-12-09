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
| Method                                           | Runtime   | Mean          | Error      | StdDev     | Median        | Min           | Max           | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------------------- |---------- |--------------:|-----------:|-----------:|--------------:|--------------:|--------------:|------:|--------:|-------:|----------:|------------:|
| &#39;PostConfigureOptions - default config&#39;          | .NET 10.0 | 2,110.8640 ns | 56.8229 ns | 65.4374 ns | 2,095.9841 ns | 2,013.1433 ns | 2,238.9894 ns |     ? |       ? | 0.2705 |    4248 B |           ? |
| &#39;PostConfigureOptions - custom config&#39;           | .NET 10.0 | 2,095.8151 ns | 68.6064 ns | 76.2558 ns | 2,101.2500 ns | 1,977.0449 ns | 2,221.1213 ns |     ? |       ? | 0.3073 |    4840 B |           ? |
|                                                  |           |               |            |            |               |               |               |       |         |        |           |             |
| &#39;PostConfigureOptions - default config&#39;          | .NET 9.0  | 2,986.9093 ns | 55.9419 ns | 52.3281 ns | 3,005.3757 ns | 2,905.3320 ns | 3,077.9813 ns |     ? |       ? | 0.2603 |    4248 B |           ? |
| &#39;PostConfigureOptions - custom config&#39;           | .NET 9.0  | 3,044.9700 ns | 24.8852 ns | 20.7803 ns | 3,042.8357 ns | 2,998.1666 ns | 3,071.4844 ns |     ? |       ? | 0.3000 |    4840 B |           ? |
|                                                  |           |               |            |            |               |               |               |       |         |        |           |             |
| &#39;Create default BenchmarkWorkspaceOptions&#39;       | .NET 10.0 | 2,072.5817 ns | 46.8350 ns | 52.0570 ns | 2,064.2872 ns | 2,003.2817 ns | 2,172.9578 ns |  1.00 |    0.03 | 0.2550 |    4120 B |        1.00 |
| &#39;Create and configure BenchmarkWorkspaceOptions&#39; | .NET 10.0 | 2,030.7542 ns | 68.5204 ns | 76.1602 ns | 2,004.4376 ns | 1,941.1354 ns | 2,174.2652 ns |  0.98 |    0.04 | 0.2548 |    4120 B |        1.00 |
|                                                  |           |               |            |            |               |               |               |       |         |        |           |             |
| &#39;Create default BenchmarkWorkspaceOptions&#39;       | .NET 9.0  | 2,979.9549 ns | 59.5392 ns | 63.7062 ns | 2,978.6564 ns | 2,892.7934 ns | 3,115.2483 ns |  1.00 |    0.03 | 0.2570 |    4120 B |        1.00 |
| &#39;Create and configure BenchmarkWorkspaceOptions&#39; | .NET 9.0  | 2,985.9997 ns | 74.5567 ns | 82.8695 ns | 3,015.2933 ns | 2,867.5636 ns | 3,138.8891 ns |  1.00 |    0.03 | 0.2534 |    4120 B |        1.00 |
|                                                  |           |               |            |            |               |               |               |       |         |        |           |             |
| &#39;Full lifecycle - create, configure, validate&#39;   | .NET 10.0 | 2,174.6726 ns | 54.0249 ns | 57.8061 ns | 2,180.9386 ns | 2,077.8922 ns | 2,284.7667 ns |     ? |       ? | 0.2779 |    4464 B |           ? |
|                                                  |           |               |            |            |               |               |               |       |         |        |           |             |
| &#39;Full lifecycle - create, configure, validate&#39;   | .NET 9.0  | 3,043.1674 ns | 63.6125 ns | 70.7051 ns | 3,018.4952 ns | 2,960.1038 ns | 3,190.1593 ns |     ? |       ? | 0.2764 |    4464 B |           ? |
|                                                  |           |               |            |            |               |               |               |       |         |        |           |             |
| &#39;Property access - RepositoryPath&#39;               | .NET 10.0 |     0.6960 ns |  0.0311 ns |  0.0276 ns |     0.6961 ns |     0.6520 ns |     0.7445 ns |     ? |       ? |      - |         - |           ? |
| &#39;Property access - Configuration&#39;                | .NET 10.0 |     0.6842 ns |  0.0374 ns |  0.0312 ns |     0.6787 ns |     0.6287 ns |     0.7299 ns |     ? |       ? |      - |         - |           ? |
|                                                  |           |               |            |            |               |               |               |       |         |        |           |             |
| &#39;Property access - RepositoryPath&#39;               | .NET 9.0  |     0.7740 ns |  0.0509 ns |  0.0476 ns |     0.7728 ns |     0.6898 ns |     0.8455 ns |     ? |       ? |      - |         - |           ? |
| &#39;Property access - Configuration&#39;                | .NET 9.0  |     0.7484 ns |  0.0522 ns |  0.0489 ns |     0.7393 ns |     0.6860 ns |     0.8567 ns |     ? |       ? |      - |         - |           ? |
|                                                  |           |               |            |            |               |               |               |       |         |        |           |             |
| &#39;Property modification - set all properties&#39;     | .NET 10.0 | 2,059.4134 ns | 54.0381 ns | 62.2303 ns | 2,050.0560 ns | 1,950.9536 ns | 2,162.7347 ns |     ? |       ? | 0.2571 |    4120 B |           ? |
|                                                  |           |               |            |            |               |               |               |       |         |        |           |             |
| &#39;Property modification - set all properties&#39;     | .NET 9.0  | 3,025.8012 ns | 65.7136 ns | 73.0405 ns | 3,030.1731 ns | 2,919.6998 ns | 3,167.3712 ns |     ? |       ? | 0.2567 |    4120 B |           ? |
|                                                  |           |               |            |            |               |               |               |       |         |        |           |             |
| &#39;ValidateOptions - valid state&#39;                  | .NET 10.0 |     6.6174 ns |  0.1017 ns |  0.0951 ns |     6.5929 ns |     6.5174 ns |     6.8342 ns |     ? |       ? |      - |         - |           ? |
|                                                  |           |               |            |            |               |               |               |       |         |        |           |             |
| &#39;ValidateOptions - valid state&#39;                  | .NET 9.0  |     3.8764 ns |  0.0950 ns |  0.0933 ns |     3.8613 ns |     3.7397 ns |     4.0996 ns |     ? |       ? |      - |         - |           ? |
