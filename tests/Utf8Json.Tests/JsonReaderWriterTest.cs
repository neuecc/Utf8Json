using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
using FsCheck.Xunit;
using Xunit;

namespace Utf8Json.Tests
{
    public class JsonReaderWriterTest
    {
        JsonReader SameAsReference<T>(T target, ArraySegment<byte> result)
        {
            var reference = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(target));
            result.ToArray().Is(reference);

            return new JsonReader(result.Array, result.Offset);
        }

        [Fact]
        public void NullTest()
        {
            var writer = new JsonWriter();
            writer.WriteNull();

            var reader = SameAsReference((object)null, writer.GetBuffer());
            reader.ReadIsNull().IsTrue();
        }
        
        [Property]
        public void BoolTest(bool target)
        {
            var writer = new JsonWriter();
            writer.WriteBoolean(target);

            var reader = SameAsReference(target, writer.GetBuffer());
            reader.ReadBoolean().Is(target);
        }
        
        [Property]
        public void ByteTest(byte target)
        {
            var writer = new JsonWriter();
            writer.WriteByte(target);

            var reader = SameAsReference(target, writer.GetBuffer());
            reader.ReadByte().Is(target);
        }
        
        [Property]
        public void UInt64Test(ulong target)
        {
            var writer = new JsonWriter();
            writer.WriteUInt64(target);

            var reader = SameAsReference(target, writer.GetBuffer());
            reader.ReadUInt64().Is(target);
        }
        
        [Property]
        public void Int64Test(long target)
        {
            var writer = new JsonWriter();
            writer.WriteInt64(target);

            var reader = SameAsReference(target, writer.GetBuffer());
            reader.ReadInt64().Is(target);
        }
                
        [Property]
        public void FloatTest(float value)
        {
            var bin = JsonSerializer.Serialize(value);
            JsonSerializer.Deserialize<float>(bin).Is(Convert.ToSingle(value));
        }
        
        [Property]
        public void DoubleTest(double value)
        {
            var bin = JsonSerializer.Serialize(value);
            JsonSerializer.Deserialize<double>(bin).Is(Convert.ToDouble(value));
        }
        
        [Property]
        public void StringTest(string v)
        {
            var js = JsonSerializer.Serialize<string>(v);
            var ok = JsonSerializer.Deserialize<string>(js);
            ok.Is(v);
        }
                
        [Property]
        public void IntArrayTest(int[] origin)
        {
            var serialized = JsonSerializer.Serialize(origin);
            var deserialized = JsonSerializer.Deserialize<int[]>(serialized);
            Assert.Equal(origin, deserialized);
        }
        
        [Property]
        public void StringArrayTest(string[] origin)
        {
            var serialized = JsonSerializer.Serialize(origin);
            var deserialized = JsonSerializer.Deserialize<string[]>(serialized);
            Assert.Equal(origin, deserialized);
        }
        
        [Fact]
        public void LargeString()
        {
            var origstr = new string('a', 99999);
            var str = "\\u0313" + origstr;
            str = "\"" + str + "\"";

            var reader = new JsonReader(Encoding.UTF8.GetBytes(str), 0);
            var aaa = reader.ReadString();

            aaa.Is("\u0313" + origstr);
        }

        [Fact]
        public void LargeString2()
        {
            var origstr = new string('a', 99999);
            var str = "\"" + origstr + "\"";

            var reader = new JsonReader(Encoding.UTF8.GetBytes(str), 0);
            var aaa = reader.ReadString();

            aaa.Is(origstr);
        }

        [Fact]
        public void LargeString3()
        {
            var origstr = new string('a', 999999);
            var str = "\"" + origstr + "\"";

            var reader = new JsonReader(Encoding.UTF8.GetBytes(str), 0);
            var aaa = reader.ReadString();

            aaa.Is(origstr);
        }

        [Fact]
        public void LargeString4()
        {
            var origstr = new string('a', 999999);
            var str = "\"" + origstr + "\"";

            var serialized = JsonSerializer.Serialize(str);
            var deserialized = JsonSerializer.Deserialize<string>(serialized);
        }

        [Fact]
        public void LargeArray()
        {
            var array = Enumerable.Range(1, 100000).ToArray();
            var bin = JsonSerializer.Serialize(array);

            var reader = new JsonReader(bin);
            reader.ReadNextBlock();

            // ok, can read.
            reader.GetCurrentOffsetUnsafe().Is(bin.Length);
        }

        [Fact]
        public void LargeNestedArraySkip()
        {
            var array = Enumerable.Range(1, 100000).Select(x => new int[0]).ToArray();
            var bin = JsonSerializer.Serialize(array);

            var reader = new JsonReader(bin);
            reader.ReadNextBlock();

            // ok, can read.
            reader.GetCurrentOffsetUnsafe().Is(bin.Length);
        }
    }
}
