``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 10 Redstone 2 (10.0.15063)
Processor=Intel Core i7-6700K CPU 4.00GHz (Skylake), ProcessorCount=8
Frequency=3914059 Hz, Resolution=255.4893 ns, Timer=TSC
.NET Core SDK=2.0.0
  [Host]     : .NET Core 2.0.0 (Framework 4.6.00001.0), 64bit RyuJIT
  Job-FSXVKE : .NET Core 2.0.0 (Framework 4.6.00001.0), 64bit RyuJIT

Jit=RyuJit  Runtime=Core  Toolchain=CoreCsProj  
LaunchCount=1  TargetCount=1  WarmupCount=1  

```
 |             Method |     Mean | Error | Scaled |  Gen 0 | Allocated |
 |------------------- |---------:|------:|-------:|-------:|----------:|
 | Utf8JsonSerializer | 322.0 ns |    NA |   1.00 | 0.0472 |     200 B |
 | Utf8Json_Handwrite | 309.4 ns |    NA |   0.96 | 0.0415 |     176 B |
