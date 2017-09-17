using Newtonsoft.Json;
using System;
using System.Text;
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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void BoolTest(bool target)
        {
            var writer = new JsonWriter();
            writer.WriteBoolean(target);

            var reader = SameAsReference(target, writer.GetBuffer());

            reader.ReadBoolean().Is(target);
        }

        [Theory]
        [InlineData(byte.MinValue)]
        [InlineData(9)]
        [InlineData(10)]
        [InlineData(99)]
        [InlineData(100)]
        [InlineData(101)]
        [InlineData(byte.MaxValue)]
        public void ByteTest(byte target)
        {
            var writer = new JsonWriter();
            writer.WriteByte(target);

            var reader = SameAsReference(target, writer.GetBuffer());

            reader.ReadByte().Is(target);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(12)]
        [InlineData(123)]
        [InlineData(12345)]
        [InlineData(123456)]
        [InlineData(1234567)]
        [InlineData(12345678)]
        [InlineData(123456789)]
        [InlineData(1234567890)]
        [InlineData(12345678901)]
        [InlineData(123456789012)]
        [InlineData(1234567890123)]
        [InlineData(12345678901234)]
        [InlineData(123456789012345)]
        [InlineData(1234567890123456)]
        [InlineData(12345678901234567)]
        [InlineData(123456789012345678)]
        [InlineData(1234567890123456789)]
        [InlineData(12345678901234567890)]
        [InlineData(ulong.MaxValue)]
        public void UInt64Test(ulong target)
        {
            var writer = new JsonWriter();
            writer.WriteUInt64(target);

            var reader = SameAsReference(target, writer.GetBuffer());

            reader.ReadUInt64().Is(target);
        }
    }
}
