using BenchmarkDotNet.Attributes;
using System;
using System.IO;
using System.Text;

namespace PerfBenchmark
{
    [Config(typeof(BenchmarkConfig))]
    public class SerializeBenchmark
    {
        static TargetClass obj1;
        static TargetClassContractless objContractless;

        static Utf8Json.IJsonFormatterResolver jsonresolver;
        Encoding utf8 = Encoding.UTF8;

        static SerializeBenchmark()
        {
            var rand = new Random(34151513);
            obj1 = TargetClass.Create(rand);
            objContractless = new TargetClassContractless(obj1);
        }

        [Benchmark(Baseline = true)]
        public byte[] Utf8JsonSerializer()
        {
            return Utf8Json.JsonSerializer.Serialize(obj1, jsonresolver);
        }

        [Benchmark]
        public byte[] MessagePackCSharp()
        {
            return MessagePack.MessagePackSerializer.Serialize(obj1);
        }

        [Benchmark]
        public byte[] MessagePackCSharpContractless()
        {
            return MessagePack.MessagePackSerializer.Serialize(objContractless, MessagePack.Resolvers.ContractlessStandardResolver.Instance);
        }

        [Benchmark]
        public void Protobufnet()
        {
            using (var ms = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(ms, obj1);
            }
        }

        [Benchmark]
        public byte[] _Jil()
        {
            return utf8.GetBytes(Jil.JSON.Serialize(obj1));
        }

        [Benchmark]
        public void _JilTextWriter()
        {
            using (var ms = new MemoryStream())
            using (var sw = new StreamWriter(ms, utf8))
            {
                Jil.JSON.Serialize(obj1, sw);
            }
        }

        [Benchmark]
        public byte[] _NetJson()
        {
            return utf8.GetBytes(NetJSON.NetJSON.Serialize(obj1));
        }

        [Benchmark]
        public byte[] _JsonNet()
        {
            return utf8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(obj1));
        }
    }

    [Config(typeof(BenchmarkConfig))]
    public class DeserializeBenchmark
    {
        byte[] json = new SerializeBenchmark().Utf8JsonSerializer();
        byte[] msgpack1 = new SerializeBenchmark().MessagePackCSharp();
        byte[] msgpack2 = new SerializeBenchmark().MessagePackCSharpContractless();
        static Utf8Json.IJsonFormatterResolver jsonresolver;
        Encoding utf8 = Encoding.UTF8;

        [Benchmark(Baseline = true)]
        public TargetClass SugoiJsonSerializer()
        {
            return Utf8Json.JsonSerializer.Deserialize<TargetClass>(json, jsonresolver);
        }

        [Benchmark]
        public TargetClass MessagePackCSharp()
        {
            return MessagePack.MessagePackSerializer.Deserialize<TargetClass>(msgpack1);
        }

        [Benchmark]
        public TargetClassContractless MessagePackCSharpContractless()
        {
            return MessagePack.MessagePackSerializer.Deserialize<TargetClassContractless>(msgpack2, MessagePack.Resolvers.ContractlessStandardResolver.Instance);
        }

        [Benchmark]
        public TargetClass _Jil()
        {
            return Jil.JSON.Deserialize<TargetClass>(utf8.GetString(json));
        }

        [Benchmark]
        public TargetClass _JilTextReader()
        {
            using (var ms = new MemoryStream(json))
            using (var sr = new StreamReader(ms, utf8))
            {
                return Jil.JSON.Deserialize<TargetClass>(sr);
            }
        }

        [Benchmark]
        public TargetClass _JsonNet()
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<TargetClass>(utf8.GetString(json));
        }

        [Benchmark]
        public TargetClass _NetJson()
        {
            return NetJSON.NetJSON.Deserialize<TargetClass>(utf8.GetString(json));
        }
    }
}
