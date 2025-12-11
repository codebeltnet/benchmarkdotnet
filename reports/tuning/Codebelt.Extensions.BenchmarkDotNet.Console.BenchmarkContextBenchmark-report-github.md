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
| Method                                              | Runtime   | ArgsCount | Mean      | Error     | StdDev    | Median    | Min       | Max       | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
|---------------------------------------------------- |---------- |---------- |----------:|----------:|----------:|----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
| **&#39;Construct BenchmarkContext with empty args&#39;**        | **.NET 10.0** | **0**         | **4.3308 ns** | **0.1697 ns** | **0.1887 ns** | **4.3068 ns** | **4.0916 ns** | **4.7537 ns** |  **1.00** |    **0.06** | **0.0015** |      **24 B** |        **1.00** |
| &#39;Construct BenchmarkContext with null args&#39;         | .NET 10.0 | 0         | 2.9383 ns | 0.2773 ns | 0.3082 ns | 2.8849 ns | 2.6235 ns | 3.6384 ns |  0.68 |    0.07 | 0.0015 |      24 B |        1.00 |
| &#39;Construct BenchmarkContext with varied args count&#39; | .NET 10.0 | 0         | 3.7137 ns | 0.1044 ns | 0.0976 ns | 3.6983 ns | 3.5100 ns | 3.8736 ns |  0.86 |    0.04 | 0.0015 |      24 B |        1.00 |
| &#39;Access Args property&#39;                              | .NET 10.0 | 0         | 0.6430 ns | 0.0409 ns | 0.0362 ns | 0.6344 ns | 0.5997 ns | 0.7230 ns |  0.15 |    0.01 |      - |         - |        0.00 |
|                                                     |           |           |           |           |           |           |           |           |       |         |        |           |             |
| &#39;Construct BenchmarkContext with empty args&#39;        | .NET 9.0  | 0         | 3.8301 ns | 0.2265 ns | 0.2517 ns | 3.8544 ns | 3.4316 ns | 4.3832 ns |  1.00 |    0.09 | 0.0015 |      24 B |        1.00 |
| &#39;Construct BenchmarkContext with null args&#39;         | .NET 9.0  | 0         | 3.5669 ns | 0.1647 ns | 0.1692 ns | 3.5704 ns | 3.3070 ns | 3.9297 ns |  0.94 |    0.07 | 0.0015 |      24 B |        1.00 |
| &#39;Construct BenchmarkContext with varied args count&#39; | .NET 9.0  | 0         | 3.7700 ns | 0.2227 ns | 0.2475 ns | 3.7711 ns | 3.4919 ns | 4.4114 ns |  0.99 |    0.09 | 0.0015 |      24 B |        1.00 |
| &#39;Access Args property&#39;                              | .NET 9.0  | 0         | 0.7330 ns | 0.0683 ns | 0.0701 ns | 0.7152 ns | 0.6434 ns | 0.8802 ns |  0.19 |    0.02 |      - |         - |        0.00 |
|                                                     |           |           |           |           |           |           |           |           |       |         |        |           |             |
| **&#39;Construct BenchmarkContext with empty args&#39;**        | **.NET 10.0** | **8**         | **3.1951 ns** | **0.3348 ns** | **0.3856 ns** | **3.1650 ns** | **2.6105 ns** | **3.7070 ns** |  **1.01** |    **0.17** | **0.0015** |      **24 B** |        **1.00** |
| &#39;Construct BenchmarkContext with null args&#39;         | .NET 10.0 | 8         | 3.2411 ns | 0.3719 ns | 0.4283 ns | 3.2930 ns | 2.5685 ns | 4.2290 ns |  1.03 |    0.18 | 0.0015 |      24 B |        1.00 |
| &#39;Construct BenchmarkContext with varied args count&#39; | .NET 10.0 | 8         | 3.1851 ns | 0.2819 ns | 0.3133 ns | 3.0243 ns | 2.7742 ns | 3.8016 ns |  1.01 |    0.16 | 0.0015 |      24 B |        1.00 |
| &#39;Access Args property&#39;                              | .NET 10.0 | 8         | 0.6626 ns | 0.0460 ns | 0.0430 ns | 0.6569 ns | 0.6087 ns | 0.7280 ns |  0.21 |    0.03 |      - |         - |        0.00 |
|                                                     |           |           |           |           |           |           |           |           |       |         |        |           |             |
| &#39;Construct BenchmarkContext with empty args&#39;        | .NET 9.0  | 8         | 3.7102 ns | 0.1381 ns | 0.1535 ns | 3.7001 ns | 3.5006 ns | 4.0536 ns |  1.00 |    0.06 | 0.0015 |      24 B |        1.00 |
| &#39;Construct BenchmarkContext with null args&#39;         | .NET 9.0  | 8         | 3.7332 ns | 0.1588 ns | 0.1630 ns | 3.7490 ns | 3.5074 ns | 4.1163 ns |  1.01 |    0.06 | 0.0015 |      24 B |        1.00 |
| &#39;Construct BenchmarkContext with varied args count&#39; | .NET 9.0  | 8         | 3.8434 ns | 0.2557 ns | 0.2945 ns | 3.7878 ns | 3.4957 ns | 4.5042 ns |  1.04 |    0.09 | 0.0015 |      24 B |        1.00 |
| &#39;Access Args property&#39;                              | .NET 9.0  | 8         | 0.6815 ns | 0.0474 ns | 0.0444 ns | 0.6644 ns | 0.6268 ns | 0.7629 ns |  0.18 |    0.01 |      - |         - |        0.00 |
|                                                     |           |           |           |           |           |           |           |           |       |         |        |           |             |
| **&#39;Construct BenchmarkContext with empty args&#39;**        | **.NET 10.0** | **64**        | **2.8310 ns** | **0.1972 ns** | **0.2192 ns** | **2.7632 ns** | **2.6250 ns** | **3.4390 ns** |  **1.01** |    **0.10** | **0.0015** |      **24 B** |        **1.00** |
| &#39;Construct BenchmarkContext with null args&#39;         | .NET 10.0 | 64        | 2.8201 ns | 0.1224 ns | 0.1360 ns | 2.8519 ns | 2.6071 ns | 3.0278 ns |  1.00 |    0.08 | 0.0015 |      24 B |        1.00 |
| &#39;Construct BenchmarkContext with varied args count&#39; | .NET 10.0 | 64        | 2.9536 ns | 0.1123 ns | 0.1153 ns | 2.9485 ns | 2.7572 ns | 3.1038 ns |  1.05 |    0.08 | 0.0015 |      24 B |        1.00 |
| &#39;Access Args property&#39;                              | .NET 10.0 | 64        | 0.6605 ns | 0.0620 ns | 0.0580 ns | 0.6445 ns | 0.5776 ns | 0.7656 ns |  0.23 |    0.03 |      - |         - |        0.00 |
|                                                     |           |           |           |           |           |           |           |           |       |         |        |           |             |
| &#39;Construct BenchmarkContext with empty args&#39;        | .NET 9.0  | 64        | 3.7801 ns | 0.2699 ns | 0.3108 ns | 3.6587 ns | 3.4339 ns | 4.3658 ns |  1.01 |    0.11 | 0.0015 |      24 B |        1.00 |
| &#39;Construct BenchmarkContext with null args&#39;         | .NET 9.0  | 64        | 4.0339 ns | 0.3728 ns | 0.4293 ns | 3.9704 ns | 3.4332 ns | 4.9281 ns |  1.07 |    0.14 | 0.0015 |      24 B |        1.00 |
| &#39;Construct BenchmarkContext with varied args count&#39; | .NET 9.0  | 64        | 4.1926 ns | 0.3751 ns | 0.4320 ns | 4.3145 ns | 3.6149 ns | 4.8710 ns |  1.12 |    0.14 | 0.0015 |      24 B |        1.00 |
| &#39;Access Args property&#39;                              | .NET 9.0  | 64        | 0.7207 ns | 0.0680 ns | 0.0636 ns | 0.6908 ns | 0.6453 ns | 0.8471 ns |  0.19 |    0.02 |      - |         - |        0.00 |
|                                                     |           |           |           |           |           |           |           |           |       |         |        |           |             |
| **&#39;Construct BenchmarkContext with empty args&#39;**        | **.NET 10.0** | **256**       | **2.7304 ns** | **0.1031 ns** | **0.1059 ns** | **2.6816 ns** | **2.5805 ns** | **2.9866 ns** |  **1.00** |    **0.05** | **0.0015** |      **24 B** |        **1.00** |
| &#39;Construct BenchmarkContext with null args&#39;         | .NET 10.0 | 256       | 2.7992 ns | 0.1377 ns | 0.1530 ns | 2.8273 ns | 2.5493 ns | 3.1046 ns |  1.03 |    0.07 | 0.0015 |      24 B |        1.00 |
| &#39;Construct BenchmarkContext with varied args count&#39; | .NET 10.0 | 256       | 3.0138 ns | 0.1205 ns | 0.1339 ns | 2.9849 ns | 2.8299 ns | 3.2886 ns |  1.11 |    0.06 | 0.0015 |      24 B |        1.00 |
| &#39;Access Args property&#39;                              | .NET 10.0 | 256       | 0.6323 ns | 0.0360 ns | 0.0281 ns | 0.6335 ns | 0.5944 ns | 0.6804 ns |  0.23 |    0.01 |      - |         - |        0.00 |
|                                                     |           |           |           |           |           |           |           |           |       |         |        |           |             |
| &#39;Construct BenchmarkContext with empty args&#39;        | .NET 9.0  | 256       | 4.7236 ns | 0.1670 ns | 0.1856 ns | 4.6982 ns | 4.5087 ns | 5.1179 ns |  1.00 |    0.05 | 0.0015 |      24 B |        1.00 |
| &#39;Construct BenchmarkContext with null args&#39;         | .NET 9.0  | 256       | 3.5940 ns | 0.1319 ns | 0.1466 ns | 3.5709 ns | 3.3897 ns | 3.9573 ns |  0.76 |    0.04 | 0.0015 |      24 B |        1.00 |
| &#39;Construct BenchmarkContext with varied args count&#39; | .NET 9.0  | 256       | 3.7935 ns | 0.1367 ns | 0.1520 ns | 3.8089 ns | 3.5272 ns | 4.0886 ns |  0.80 |    0.04 | 0.0015 |      24 B |        1.00 |
| &#39;Access Args property&#39;                              | .NET 9.0  | 256       | 0.6915 ns | 0.0522 ns | 0.0436 ns | 0.6913 ns | 0.6360 ns | 0.8050 ns |  0.15 |    0.01 |      - |         - |        0.00 |
