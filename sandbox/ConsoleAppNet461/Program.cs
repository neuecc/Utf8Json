using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.CsProj;
using Newtonsoft.Json;
using System;
using System.Text;
using Utf8Json;

class Program
{
    static void Main(string[] args)
    {
        var switcher = new BenchmarkSwitcher(new[]
        {
            typeof(DoubleConvertBenchmark),
            typeof(StringToDoubleBenchmark)
        });

        // args = new string[] { "0" };

#if DEBUG
        // var b = new StringToDoubleBenchmark();


        var writer = new Utf8Json.JsonWriter();
        var cc = "\thogehoge, hu\t gahuga  dato\tya";
        writer.WriteString(cc);

        var ret1 = JsonConvert.SerializeObject(cc);
        var ret2 = Encoding.UTF8.GetString(writer.ToUtf8ByteArray());

        Console.WriteLine(ret1 == ret2);

#else
        switcher.Run(args);
#endif
    }
}

public class BenchmarkConfig : ManualConfig
{
    public BenchmarkConfig()
    {
        Add(MarkdownExporter.GitHub);
        Add(MemoryDiagnoser.Default);

        var baseJob = Job.ShortRun.WithLaunchCount(1).WithTargetCount(1).WithWarmupCount(1);
        Add(baseJob.With(Runtime.Clr).With(Jit.RyuJit).With(Platform.X64));
        // Add(baseJob.WithLaunchCount(1).WithTargetCount(1).WithWarmupCount(1).With(Runtime.Core).With(CsProjCoreToolchain.NetCoreApp20));
    }
}

[Config(typeof(BenchmarkConfig))]
public class DoubleConvertBenchmark
{
    const double value = 12345.6789;

    public DoubleConvertBenchmark()
    {

    }

    [Benchmark]
    public byte[] DoubleToStringConverter()
    {
        byte[] buf = new byte[20];
        Utf8Json.Internal.DoubleConversion.DoubleToStringConverter.GetBytes(ref buf, 0, value);
        return buf;
    }

    [Benchmark]
    public string DoubleToStringConverterToString()
    {
        return Utf8Json.Internal.DoubleConversion.DoubleToStringConverter.GetString(value);
    }

    [Benchmark]
    public byte[] StandardToStringUtf8()
    {
        return Encoding.UTF8.GetBytes(value.ToString());
    }

    [Benchmark]
    public string StandardToString()
    {
        return value.ToString();
    }
}


[Config(typeof(BenchmarkConfig))]
public class StringToDoubleBenchmark
{
    const double value = 12345.6789;
    static readonly byte[] strBytes = Encoding.UTF8.GetBytes(value.ToString());
    static readonly string str = value.ToString();

    public StringToDoubleBenchmark()
    {

    }

    [Benchmark]
    public double DoubleToStringConverter()
    {
        return Utf8Json.Internal.DoubleConversion.StringToDoubleConverter.ToDouble(strBytes, 0, out var _);
    }

    [Benchmark]
    public double DoubleParse()
    {
        return Double.Parse(str);
    }

    [Benchmark]
    public Double DoubleParseWithDecode()
    {
        return Double.Parse(Encoding.UTF8.GetString(strBytes));
    }


}