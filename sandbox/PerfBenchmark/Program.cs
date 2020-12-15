using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using PerfBenchmark;

class Program
{
    static void Main(string[] args)
    {
        var switcher = new BenchmarkSwitcher(new[]
        {
            typeof(SerializeBenchmark),
            typeof(DeserializeBenchmark),
            typeof(JsonSerializeBench)
        });
        
#if DEBUG
        var b = new DeserializeBenchmark();
        b.Utf8JsonSerializer();
#else
        switcher.Run(args);
#endif
    }
}

public class BenchmarkConfig : ManualConfig
{
    public BenchmarkConfig()
    {
        IDiagnoser[] diagnosers = {
            MemoryDiagnoser.Default
        };

        IExporter[] exporters = {
            MarkdownExporter.GitHub
        };

        AddDiagnoser(diagnosers);
        AddExporter(exporters);

        AddJob(Job.Default.WithRuntime(CoreRuntime.Core50).WithPlatform(Platform.X64));
    }
}