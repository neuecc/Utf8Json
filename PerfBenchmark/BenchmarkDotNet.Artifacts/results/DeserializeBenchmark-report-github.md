``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 10 Redstone 2 (10.0.15063)
Processor=Intel Core i7-6700K CPU 4.00GHz (Skylake), ProcessorCount=8
Frequency=3914059 Hz, Resolution=255.4893 ns, Timer=TSC
.NET Core SDK=2.0.0
  [Host]     : .NET Core 2.0.0 (Framework 4.6.00001.0), 64bit RyuJIT
  Job-XZXGHJ : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2110.0

Jit=RyuJit  Platform=X64  Runtime=Clr  
LaunchCount=1  TargetCount=1  WarmupCount=1  

```
 |                        Method |        Mean | Error | Scaled |  Gen 0 | Allocated |
 |------------------------------ |------------:|------:|-------:|-------:|----------:|
 |            Utf8JsonSerializer |   574.56 ns |    NA |   1.00 | 0.0110 |      48 B |
 |             MessagePackCSharp |    77.36 ns |    NA |   0.13 | 0.0113 |      48 B |
 | MessagePackCSharpContractless |   190.77 ns |    NA |   0.33 | 0.0112 |      48 B |
 |                          _Jil |   779.73 ns |    NA |   1.36 | 0.1097 |     464 B |
 |                _JilTextReader | 1,399.25 ns |    NA |   2.44 | 0.8430 |    3544 B |
 |                      _JsonNet | 3,078.89 ns |    NA |   5.36 | 0.7820 |    3296 B |
 |                      _NetJson | 1,252.37 ns |    NA |   2.18 | 0.2441 |    1032 B |
