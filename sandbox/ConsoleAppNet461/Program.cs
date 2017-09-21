using System.Security;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.CsProj;
using System;
using System.IO;
using System.Text;
using Utf8Json;
using Utf8Json.Formatters;
using Utf8Json.Internal;
using System.Collections.Generic;


// [assembly: AllowPartiallyTrustedCallers]
// [assembly: SecurityTransparent]
[assembly: SecurityRules(SecurityRuleSet.Level2, SkipVerificationInFullTrust = true)]

class Program
{
    static void Main(string[] args)
    {
        var switcher = new BenchmarkSwitcher(new[]
        {
            typeof(DoubleConvertBenchmark),
            typeof(StringToDoubleBenchmark),
            typeof(SwitchVsIf),
            typeof(SwitchVsSwitch),
            typeof(SerializeCheck)
        });

        // args = new string[] { "0" };

#if DEBUG

        //var s1 = Encoding.UTF8.GetBytes("\"あいうえお\"");
        //var s1 = Encoding.UTF8.GetBytes("\"あいう\\tえお\"");
        //var s1 = Encoding.UTF8.GetBytes("\"あいう\tえお\t\"");
        var s1 = Encoding.UTF8.GetBytes("\"\\u3042\\u3044\\u3046なの\"");
        //var s1 = Encoding.UTF8.GetBytes("\"\uD840\uDC0B\"");

        var str = new JsonReader(s1, 0).ReadString();
        Console.WriteLine(str);

#else
        switcher.Run(args);
#endif
    }
}

public class SimplePerson
{
    public int Age { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

[MessagePack.MessagePackObject]
public class SimplePersonMsgpack
{
    [MessagePack.Key(0)]
    public int Age { get; set; }
    [MessagePack.Key(1)]
    public string FirstName { get; set; }
    [MessagePack.Key(2)]
    public string LastName { get; set; }
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

    //[Benchmark]
    //public byte[] DoubleToStringConverter()
    //{
    //    byte[] buf = new byte[20];
    //    Utf8Json.Internal.DoubleConversion.DoubleToStringConverter.GetBytes(ref buf, 0, value);
    //    return buf;
    //}

    //[Benchmark]
    //public string DoubleToStringConverterToString()
    //{
    //    return Utf8Json.Internal.DoubleConversion.DoubleToStringConverter.GetString(value);
    //}

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
        return NumberConverter.ReadDouble(strBytes, 0, out var _);
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



[Config(typeof(BenchmarkConfig))]
public class SwitchVsIf
{
    byte c = byte.Parse("8");

    [Benchmark]
    public bool SwitchOpt()
    {
        switch (c)
        {
            case (byte)'0':
            case (byte)'1':
            case (byte)'2':
            case (byte)'3':
            case (byte)'4':
            case (byte)'5':
            case (byte)'6':
            case (byte)'7':
            case (byte)'8':
            case (byte)'9':
                return true;
            case 0:
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
            case 8:
            case 9:
            case 10:
            case 11:
            case 12:
            case 13:
            case 14:
            case 15:
            case 16:
            case 17:
            case 18:
            case 19:
            case 20:
            case 21:
            case 22:
            case 23:
            case 24:
            case 25:
            case 26:
            case 27:
            case 28:
            case 29:
            case 30:
            case 31:
            case 32:
            case 33:
            case 34:
            case 35:
            case 36:
            case 37:
            case 38:
            case 39:
            case 40:
            case 41:
            case 42:
            case 43:
            case 44:
            case 45:
            case 46:
            case 47:
            default:
                return false;
        }
    }

    [Benchmark]
    public bool StandardSwitch()
    {
        switch (c)
        {
            case (byte)'0':
            case (byte)'1':
            case (byte)'2':
            case (byte)'3':
            case (byte)'4':
            case (byte)'5':
            case (byte)'6':
            case (byte)'7':
            case (byte)'8':
            case (byte)'9':
                return true;
            default:
                return false;
        }
    }

    [Benchmark]
    public bool If()
    {
        return (byte)'0' <= c && c <= (byte)'9';
    }
}

[Config(typeof(BenchmarkConfig))]
public class SwitchVsSwitch
{
    string value;
    int i;

    public SwitchVsSwitch()
    {
        value = "abcdefghijklmnopqrstu";
        i = int.Parse("5");
    }

    [Benchmark]
    public bool SwitchOptimized()
    {
        switch (value[i])
        {
            case '"':
                return true;
            case '\\':
                return true;
            case '\b':
                return true;
            case '\f':
                return true;
            case '\n':
                return true;
            case '\r':
                return true;
            case '\t':
                return true;
            // use switch jumptable
            case (char)0:
            case (char)1:
            case (char)2:
            case (char)3:
            case (char)4:
            case (char)5:
            case (char)6:
            case (char)7:
            case (char)11:
            case (char)14:
            case (char)15:
            case (char)16:
            case (char)17:
            case (char)18:
            case (char)19:
            case (char)20:
            case (char)21:
            case (char)22:
            case (char)23:
            case (char)24:
            case (char)25:
            case (char)26:
            case (char)27:
            case (char)28:
            case (char)29:
            case (char)30:
            case (char)31:
            case (char)32:
            case (char)33:
            case (char)35:
            case (char)36:
            case (char)37:
            case (char)38:
            case (char)39:
            case (char)40:
            case (char)41:
            case (char)42:
            case (char)43:
            case (char)44:
            case (char)45:
            case (char)46:
            case (char)47:
            case (char)48:
            case (char)49:
            case (char)50:
            case (char)51:
            case (char)52:
            case (char)53:
            case (char)54:
            case (char)55:
            case (char)56:
            case (char)57:
            case (char)58:
            case (char)59:
            case (char)60:
            case (char)61:
            case (char)62:
            case (char)63:
            case (char)64:
            case (char)65:
            case (char)66:
            case (char)67:
            case (char)68:
            case (char)69:
            case (char)70:
            case (char)71:
            case (char)72:
            case (char)73:
            case (char)74:
            case (char)75:
            case (char)76:
            case (char)77:
            case (char)78:
            case (char)79:
            case (char)80:
            case (char)81:
            case (char)82:
            case (char)83:
            case (char)84:
            case (char)85:
            case (char)86:
            case (char)87:
            case (char)88:
            case (char)89:
            case (char)90:
            case (char)91:
            default:
                return false;
        }
    }

    [Benchmark]
    public bool SwitchStandard()
    {
        switch (value[i])
        {
            case '"':
                return true;
            case '\\':
                return true;
            case '\b':
                return true;
            case '\f':
                return true;
            case '\n':
                return true;
            case '\r':
                return true;
            case '\t':
                return true;
            default:
                return false;
        }
    }
}

[Config(typeof(BenchmarkConfig))]
public class SerializeCheck
{
    byte[] cache = new byte[10000];
    SimplePerson p = new SimplePerson { Age = 99, FirstName = "foo", LastName = "baz" };
    SimplePersonMsgpack p2 = new SimplePersonMsgpack { Age = 99, FirstName = "foo", LastName = "baz" };
    IJsonFormatter<SimplePerson> formatter = new SimplePersonFormatter();
    Encoding utf8 = Encoding.UTF8;

    [Benchmark(Baseline = true)]
    public byte[] SugoiJsonSerializer()
    {
        var writer = new JsonWriter(cache);
        formatter.Serialize(ref writer, ref p, null);
        return writer.ToUtf8ByteArray();
    }

    [Benchmark]
    public byte[] MessagePackCSharp()
    {
        return MessagePack.MessagePackSerializer.Serialize(p2);
    }

    [Benchmark]
    public byte[] MessagePackCSharpContractless()
    {
        return MessagePack.MessagePackSerializer.Serialize(p, MessagePack.Resolvers.ContractlessStandardResolver.Instance);
    }

    [Benchmark]
    public byte[] _Jil()
    {
        return utf8.GetBytes(Jil.JSON.Serialize(p));
    }

    [Benchmark]
    public void _JilTextWriter()
    {
        using (var ms = new MemoryStream())
        using (var sw = new StreamWriter(ms, utf8))
        {
            Jil.JSON.Serialize(p, sw);
        }
    }

    [Benchmark]
    public byte[] _JsonNet()
    {
        return utf8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(p));
    }

    [Benchmark]
    public byte[] _NetJson()
    {
        return utf8.GetBytes(NetJSON.NetJSON.Serialize(p));
    }
}

[Config(typeof(BenchmarkConfig))]
public class DeserializeCheck
{
    byte[] json = new SerializeCheck().SugoiJsonSerializer();
    byte[] msgpack1 = new SerializeCheck().MessagePackCSharp();
    byte[] msgpack2 = new SerializeCheck().MessagePackCSharpContractless();
    IJsonFormatter<SimplePerson> formatter = new SimplePersonFormatter();

    Encoding utf8 = Encoding.UTF8;

    [Benchmark(Baseline = true)]
    public SimplePerson SugoiJsonSerializer()
    {
        var reader = new JsonReader(json, 0);
        return formatter.Deserialize(ref reader, null);
    }

    [Benchmark]
    public SimplePerson MessagePackCSharp()
    {
        return MessagePack.MessagePackSerializer.Deserialize<SimplePerson>(msgpack1);
    }

    [Benchmark]
    public SimplePerson MessagePackCSharpContractless()
    {
        return MessagePack.MessagePackSerializer.Deserialize<SimplePerson>(msgpack2, MessagePack.Resolvers.ContractlessStandardResolver.Instance);
    }

    [Benchmark]
    public SimplePerson _Jil()
    {
        return Jil.JSON.Deserialize<SimplePerson>(utf8.GetString(json));
    }

    [Benchmark]
    public SimplePerson _JilTextReader()
    {
        using (var ms = new MemoryStream(json))
        using (var sr = new StreamReader(ms, utf8))
        {
            return Jil.JSON.Deserialize<SimplePerson>(sr);
        }
    }

    [Benchmark]
    public SimplePerson _JsonNet()
    {
        return Newtonsoft.Json.JsonConvert.DeserializeObject<SimplePerson>(utf8.GetString(json));
    }

    [Benchmark]
    public SimplePerson _NetJson()
    {
        return NetJSON.NetJSON.Deserialize<SimplePerson>(utf8.GetString(json));
    }
}

public class SimplePersonFormatter : IJsonFormatter<SimplePerson>
{
    readonly byte[][] nameCaches;
    readonly AutomataDictionary dictionary;

    public SimplePersonFormatter()
    {
        // byte cache with "{" and ","
        nameCaches = new byte[3][]
        {
            JsonWriter.GetEncodedPropertyNameWithBeginObject("Age"), // {\"Age\":
            JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("FirstName"), // ",\"FirstName\":
            JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("LastName"), // ",\"LastName\":
        };
        dictionary = new AutomataDictionary
        {
            {  JsonWriter.GetEncodedPropertyNameWithoutQuotation("Age"), 0 },
            {  JsonWriter.GetEncodedPropertyNameWithoutQuotation("FirstName"), 1 },
            {  JsonWriter.GetEncodedPropertyNameWithoutQuotation("LastName"), 2 },
        };
    }

    public void Serialize(ref JsonWriter writer, ref SimplePerson value, IJsonFormatterResolver formatterResolver)
    {
        UnsafeMemory64.WriteRaw7(ref writer, nameCaches[0]); // optimize byte->byte copy we know src size.
        writer.WriteInt32(value.Age);
        UnsafeMemory64.WriteRaw13(ref writer, nameCaches[1]);
        writer.WriteString(value.FirstName);
        UnsafeMemory64.WriteRaw12(ref writer, nameCaches[2]);
        writer.WriteString(value.LastName);
        writer.WriteEndObject();
    }

    public SimplePerson Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        if (reader.ReadIsNull()) return null;

        var result = new SimplePerson();

        reader.ReadIsBeginObjectWithVerify(); // "{"
        var count = 0;
        while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref count)) // "}", skip "," when count != 0
        {
            // automata lookup
            var key = reader.ReadPropertyNameSegmentUnsafe();

            int switchKey;
            if (!dictionary.TryGetValue(key, out switchKey)) switchKey = -1;

            switch (switchKey)
            {
                case 0:
                    result.Age = reader.ReadInt32();
                    break;
                case 1:
                    result.FirstName = reader.ReadString();
                    break;
                case 2:
                    result.LastName = reader.ReadString();
                    break;
                default:
                    reader.ReadNextBlock();
                    break;
            }
        }

        return result;
    }
}