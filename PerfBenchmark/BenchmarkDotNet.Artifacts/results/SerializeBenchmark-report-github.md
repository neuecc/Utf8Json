``` ini

BenchmarkDotNet=v0.10.9, OS=Windows 10 Redstone 1 (10.0.14393)
Processor=Intel Core i7-6700 CPU 3.40GHz (Skylake), ProcessorCount=8
Frequency=3328129 Hz, Resolution=300.4691 ns, Timer=TSC
.NET Core SDK=2.0.0
  [Host]     : .NET Core 2.0.0 (Framework 4.6.00001.0), 64bit RyuJIT
  Job-SWZORH : .NET Core 2.0.0 (Framework 4.6.00001.0), 64bit RyuJIT

Jit=RyuJit  Runtime=Core  Toolchain=CoreCsProj  
LaunchCount=1  TargetCount=1  WarmupCount=1  

```
 |                        Method |        Mean | Error | Scaled | ScaledSD |  Gen 0 | Allocated |
 |------------------------------ |------------:|------:|-------:|---------:|-------:|----------:|
 |            Utf8JsonSerializer |          NA |    NA |      ? |        ? |    N/A |       N/A |
 |             MessagePackCSharp |    256.1 ns |    NA |      ? |        ? | 0.0262 |     112 B |
 | MessagePackCSharpContractless |    337.1 ns |    NA |      ? |        ? | 0.0472 |     200 B |
 |                   Protobufnet |    625.8 ns |    NA |      ? |        ? | 0.1202 |     504 B |
 |                          _Jil |  6,655.8 ns |    NA |      ? |        ? | 0.5951 |    2520 B |
 |                _JilTextWriter |  6,995.3 ns |    NA |      ? |        ? | 1.5411 |    6480 B |
 |                      _NetJson |  6,916.8 ns |    NA |      ? |        ? | 0.3891 |    1648 B |
 |                      _JsonNet | 15,379.9 ns |    NA |      ? |        ? | 0.7935 |    3440 B |

Benchmarks with issues:
  SerializeBenchmark.Utf8JsonSerializer: Job-SWZORH(Jit=RyuJit, Runtime=Core, Toolchain=CoreCsProj, LaunchCount=1, TargetCount=1, WarmupCount=1)
