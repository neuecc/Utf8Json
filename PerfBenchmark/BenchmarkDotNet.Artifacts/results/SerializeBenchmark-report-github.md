``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 10 Redstone 1 (10.0.14393)
Processor=Intel Core i7-6700 CPU 3.40GHz (Skylake), ProcessorCount=8
Frequency=3328129 Hz, Resolution=300.4691 ns, Timer=TSC
.NET Core SDK=2.0.0
  [Host]     : .NET Core 2.0.0 (Framework 4.6.00001.0), 64bit RyuJIT
  Job-SUIUKH : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2053.0

Jit=RyuJit  Platform=X64  Runtime=Clr  
LaunchCount=1  TargetCount=1  WarmupCount=1  

```
 |                        Method |       Mean | Error | Scaled |  Gen 0 | Allocated |
 |------------------------------ |-----------:|------:|-------:|-------:|----------:|
 |            Utf8JsonSerializer |   365.6 ns |    NA |   1.00 | 0.0415 |     176 B |
 |             MessagePackCSharp |   141.5 ns |    NA |   0.39 | 0.0150 |      64 B |
 | MessagePackCSharpContractless |   164.7 ns |    NA |   0.45 | 0.0303 |     128 B |
 |                   Protobufnet |   404.6 ns |    NA |   1.11 | 0.1273 |     536 B |
 |                           Jil |   931.1 ns |    NA |   2.55 | 0.3481 |    1464 B |
 |                 JilTextWriter | 1,155.8 ns |    NA |   3.16 | 1.4019 |    5890 B |
 |                       NetJson | 1,021.2 ns |    NA |   2.79 | 0.2270 |     960 B |
 |                       JsonNet | 2,813.7 ns |    NA |   7.70 | 0.4768 |    2016 B |
