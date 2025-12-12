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
| Method                                           | Runtime   | Mean          | Error      | StdDev     | Median        | Min           | Max           | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|------------------------------------------------- |---------- |--------------:|-----------:|-----------:|--------------:|--------------:|--------------:|------:|--------:|-------:|----------:|------------:|
| &#39;PostConfigureOptions - default config&#39;          | .NET 10.0 | 2,078.7319 ns | 58.4506 ns | 62.5414 ns | 2,071.1990 ns | 1,973.5708 ns | 2,178.6508 ns |     ? |       ? | 0.2657 |    4248 B |           ? |
| &#39;PostConfigureOptions - custom config&#39;           | .NET 10.0 | 2,063.4760 ns | 62.8084 ns | 69.8114 ns | 2,058.5810 ns | 1,975.1579 ns | 2,212.9844 ns |     ? |       ? | 0.3027 |    4840 B |           ? |
|                                                  |           |               |            |            |               |               |               |       |         |        |           |             |
| &#39;PostConfigureOptions - default config&#39;          | .NET 9.0  | 2,933.8417 ns | 57.4521 ns | 53.7407 ns | 2,921.3425 ns | 2,860.3389 ns | 3,030.9748 ns |     ? |       ? | 0.2644 |    4248 B |           ? |
| &#39;PostConfigureOptions - custom config&#39;           | .NET 9.0  | 3,027.1892 ns | 69.3406 ns | 77.0719 ns | 3,014.0595 ns | 2,911.8273 ns | 3,165.0277 ns |     ? |       ? | 0.3041 |    4840 B |           ? |
|                                                  |           |               |            |            |               |               |               |       |         |        |           |             |
| &#39;Create default BenchmarkWorkspaceOptions&#39;       | .NET 10.0 | 2,112.7347 ns | 40.1347 ns | 42.9437 ns | 2,107.3630 ns | 2,023.5091 ns | 2,166.5292 ns |  1.00 |    0.03 | 0.2575 |    4120 B |        1.00 |
| &#39;Create and configure BenchmarkWorkspaceOptions&#39; | .NET 10.0 | 2,060.7947 ns | 40.9970 ns | 43.8663 ns | 2,051.6384 ns | 1,997.5080 ns | 2,135.4734 ns |  0.98 |    0.03 | 0.2585 |    4120 B |        1.00 |
|                                                  |           |               |            |            |               |               |               |       |         |        |           |             |
| &#39;Create default BenchmarkWorkspaceOptions&#39;       | .NET 9.0  | 3,008.0985 ns | 41.5610 ns | 36.8428 ns | 3,006.9752 ns | 2,957.3965 ns | 3,084.6125 ns |  1.00 |    0.02 | 0.2564 |    4120 B |        1.00 |
| &#39;Create and configure BenchmarkWorkspaceOptions&#39; | .NET 9.0  | 3,018.5043 ns | 64.9885 ns | 72.2346 ns | 3,020.6452 ns | 2,898.3530 ns | 3,166.4212 ns |  1.00 |    0.03 | 0.2543 |    4120 B |        1.00 |
|                                                  |           |               |            |            |               |               |               |       |         |        |           |             |
| &#39;Full lifecycle - create, configure, validate&#39;   | .NET 10.0 | 2,133.8326 ns | 41.8069 ns | 39.1062 ns | 2,127.8066 ns | 2,091.4368 ns | 2,212.8536 ns |     ? |       ? | 0.2765 |    4464 B |           ? |
|                                                  |           |               |            |            |               |               |               |       |         |        |           |             |
| &#39;Full lifecycle - create, configure, validate&#39;   | .NET 9.0  | 3,059.8789 ns | 67.8953 ns | 78.1884 ns | 3,053.1393 ns | 2,965.1217 ns | 3,244.5957 ns |     ? |       ? | 0.2740 |    4464 B |           ? |
|                                                  |           |               |            |            |               |               |               |       |         |        |           |             |
| &#39;Property access - RepositoryPath&#39;               | .NET 10.0 |     0.6729 ns |  0.0517 ns |  0.0484 ns |     0.6575 ns |     0.6005 ns |     0.7820 ns |     ? |       ? |      - |         - |           ? |
| &#39;Property access - Configuration&#39;                | .NET 10.0 |     0.6834 ns |  0.0442 ns |  0.0414 ns |     0.6914 ns |     0.6219 ns |     0.7441 ns |     ? |       ? |      - |         - |           ? |
|                                                  |           |               |            |            |               |               |               |       |         |        |           |             |
| &#39;Property access - RepositoryPath&#39;               | .NET 9.0  |     0.7156 ns |  0.0646 ns |  0.0604 ns |     0.6920 ns |     0.6508 ns |     0.8309 ns |     ? |       ? |      - |         - |           ? |
| &#39;Property access - Configuration&#39;                | .NET 9.0  |     0.7295 ns |  0.0387 ns |  0.0362 ns |     0.7216 ns |     0.6821 ns |     0.8011 ns |     ? |       ? |      - |         - |           ? |
|                                                  |           |               |            |            |               |               |               |       |         |        |           |             |
| &#39;Property modification - set all properties&#39;     | .NET 10.0 | 2,145.0926 ns | 39.4528 ns | 34.9739 ns | 2,140.4873 ns | 2,100.3063 ns | 2,195.2403 ns |     ? |       ? | 0.2594 |    4120 B |           ? |
|                                                  |           |               |            |            |               |               |               |       |         |        |           |             |
| &#39;Property modification - set all properties&#39;     | .NET 9.0  | 2,958.1106 ns | 65.5500 ns | 72.8586 ns | 2,931.3603 ns | 2,871.0808 ns | 3,147.9898 ns |     ? |       ? | 0.2529 |    4120 B |           ? |
|                                                  |           |               |            |            |               |               |               |       |         |        |           |             |
| &#39;ValidateOptions - valid state&#39;                  | .NET 10.0 |     7.0237 ns |  0.0981 ns |  0.0870 ns |     7.0394 ns |     6.8917 ns |     7.1845 ns |     ? |       ? |      - |         - |           ? |
|                                                  |           |               |            |            |               |               |               |       |         |        |           |             |
| &#39;ValidateOptions - valid state&#39;                  | .NET 9.0  |     3.9379 ns |  0.1053 ns |  0.1081 ns |     3.9329 ns |     3.7514 ns |     4.1705 ns |     ? |       ? |      - |         - |           ? |
