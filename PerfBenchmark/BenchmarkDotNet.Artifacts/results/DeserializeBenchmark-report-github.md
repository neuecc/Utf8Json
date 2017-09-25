``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 10 Redstone 1 (10.0.14393)
Processor=Intel Core i7-6700 CPU 3.40GHz (Skylake), ProcessorCount=8
Frequency=3328129 Hz, Resolution=300.4691 ns, Timer=TSC
.NET Core SDK=2.0.0
  [Host]     : .NET Core 2.0.0 (Framework 4.6.00001.0), 64bit RyuJIT
  Job-DNHDNW : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2053.0

Jit=RyuJit  Platform=X64  Runtime=Clr  
LaunchCount=1  TargetCount=1  WarmupCount=1  

```
 |                        Method |       Mean | Error | Scaled |  Gen 0 | Allocated |
 |------------------------------ |-----------:|------:|-------:|-------:|----------:|
 |            Utf8JsonSerializer |   534.5 ns |    NA |   1.00 | 0.0105 |      48 B |
 |             MessagePackCSharp |   105.8 ns |    NA |   0.20 | 0.0113 |      48 B |
 | MessagePackCSharpContractless |   250.6 ns |    NA |   0.47 | 0.0110 |      48 B |
 |                   Protobufnet |   333.1 ns |    NA |   0.62 | 0.0362 |     152 B |
 |                           Jil |   995.1 ns |    NA |   1.86 | 0.1087 |     464 B |
 |                 JilTextReader | 1,672.7 ns |    NA |   3.13 | 0.8430 |    3544 B |
 |                       JsonNet | 3,855.7 ns |    NA |   7.21 | 0.7782 |    3297 B |
 |                       NetJson | 1,330.9 ns |    NA |   2.49 | 0.2441 |    1032 B |
