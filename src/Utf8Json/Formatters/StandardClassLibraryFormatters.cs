using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
    public sealed class CharFormatter : IJsonFormatter<char>
    {
        public static readonly IJsonFormatter<char> Default = new CharFormatter();

        // MEMO:can be improvement write directly
        public void Serialize(ref JsonWriter writer, char value, IJsonFormatterResolver formatterResolver)
        {
            writer.WriteString(value.ToString());
        }

        public char Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            return reader.ReadString()[0];
        }
    }

    public sealed class GuidFormatter : IJsonFormatter<Guid>
    {
        public static readonly IJsonFormatter<Guid> Default = new GuidFormatter();

        public void Serialize(ref JsonWriter writer, Guid value, IJsonFormatterResolver formatterResolver)
        {
            writer.EnsureCapacity(38); // unsafe, control underlying buffer manually

            writer.WriteRawUnsafe((byte)'\"');

            var rawData = writer.GetBuffer();
            new GuidBits(ref value).Write(rawData.Array, rawData.Offset); // len = 36

            writer.WriteRawUnsafe((byte)'\"');
        }

        public Guid Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            var segment = reader.ReadStringSegmentUnsafe();
            return new GuidBits(ref segment).Value;
        }
    }

    // MEMO:should write/read base64 directly like corefxlab/System.Binary.Base64
    // https://github.com/dotnet/corefxlab/tree/master/src/System.Binary.Base64/System/Binary
    public sealed class ByteArrayFormatter : IJsonFormatter<byte[]>
    {
        public static readonly IJsonFormatter<byte[]> Default = new ByteArrayFormatter();

        public void Serialize(ref JsonWriter writer, byte[] value, IJsonFormatterResolver formatterResolver)
        {
            if (value == null) { writer.WriteNull(); return; }

            writer.WriteString(Convert.ToBase64String(value, Base64FormattingOptions.None));
        }

        public byte[] Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull()) return null;

            var str = reader.ReadString();
            return Convert.FromBase64String(str);
        }
    }

    public sealed class ByteArraySegmentFormatter : IJsonFormatter<ArraySegment<byte>>
    {
        public static readonly IJsonFormatter<ArraySegment<byte>> Default = new ByteArraySegmentFormatter();

        public void Serialize(ref JsonWriter writer, ArraySegment<byte> value, IJsonFormatterResolver formatterResolver)
        {
            if (value.Array == null) { writer.WriteNull(); return; }

            writer.WriteString(Convert.ToBase64String(value.Array, value.Offset, value.Count, Base64FormattingOptions.None));
        }

        public ArraySegment<byte> Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull()) return default(ArraySegment<byte>);

            var str = reader.ReadString();
            var bytes = Convert.FromBase64String(str);
            return new ArraySegment<byte>(bytes, 0, bytes.Length);
        }
    }

}