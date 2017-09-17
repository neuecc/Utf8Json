using System;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
    public sealed class GuidFormatter : IJsonFormatter<Guid>
    {
        public static readonly IJsonFormatter<Guid> Default = new GuidFormatter();

        public void Serialize(ref JsonWriter writer, ref Guid value, IFormatterResolver formatterResolver)
        {
            writer.EnsureCapacity(38); // unsafe, control underlyingbuffer manually

            writer.WriteRawUnsafe((byte)'\"');

            var rawData = writer.GetBuffer();
            new GuidBits(ref value).Write(rawData.Array, rawData.Offset);

            writer.WriteRawUnsafe((byte)'\"');
        }

        public Guid Deserialize(ref JsonReader reader, IFormatterResolver formatterResolver)
        {
            var segment = reader.ReadStringSegmentUnsafe();
            return new GuidBits(segment).Value;
        }
    }
}