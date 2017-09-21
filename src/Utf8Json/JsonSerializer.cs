using System;
using System.IO;
using Utf8Json.Internal;

namespace Utf8Json
{
    public static class JsonSerializer
    {
        public static byte[] Serialize<T>(T value, IJsonFormatterResolver resolver)
        {
            var writer = new JsonWriter(MemoryPool.GetBuffer());
            var formatter = resolver.GetFormatterWithVerify<T>();
            formatter.Serialize(ref writer, ref value, resolver);
            return writer.ToUtf8ByteArray();
        }

        public static void Serialize<T>(Stream stream, T value, IJsonFormatterResolver resolver)
        {
            var writer = new JsonWriter(MemoryPool.GetBuffer());
            var formatter = resolver.GetFormatterWithVerify<T>();
            formatter.Serialize(ref writer, ref value, resolver);
            var buffer = writer.GetBuffer();
            stream.Write(buffer.Array, buffer.Offset, buffer.Count);
        }

        public static string ToJsonString<T>(T value, IJsonFormatterResolver resolver)
        {
            var writer = new JsonWriter(MemoryPool.GetBuffer());
            var formatter = resolver.GetFormatterWithVerify<T>();
            formatter.Serialize(ref writer, ref value, resolver);
            return writer.ToString();
        }

        public static T Deserialize<T>(string json, IJsonFormatterResolver resolver)
        {
            return Deserialize<T>(StringEncoding.UTF8.GetBytes(json), resolver);
        }

        public static T Deserialize<T>(byte[] bytes, IJsonFormatterResolver resolver)
        {
            return Deserialize<T>(bytes, 0, resolver);
        }

        public static T Deserialize<T>(byte[] bytes, int offset, IJsonFormatterResolver resolver)
        {
            var reader = new JsonReader(bytes, offset);
            var formatter = resolver.GetFormatterWithVerify<T>();
            return formatter.Deserialize(ref reader, resolver);
        }

        public static T Deserialize<T>(Stream stream, IJsonFormatterResolver resolver)
        {
#if NETSTANDARD
            var ms = stream as MemoryStream;
            ArraySegment<byte> buf2;
            if (ms.TryGetBuffer(out buf2))
            {
                return Deserialize<T>(buf2.Array, buf2.Offset, resolver);
            }
#endif

            var buf = MemoryPool.GetBuffer();
            FillFromStream(stream, ref buf);
            return Deserialize<T>(buf, resolver);
        }

        static int FillFromStream(Stream input, ref byte[] buffer)
        {
            int length = 0;
            int read;
            while ((read = input.Read(buffer, length, buffer.Length - length)) > 0)
            {
                length += read;
                if (length == buffer.Length)
                {
                    BinaryUtil.FastResize(ref buffer, length * 2);
                }
            }

            return length;
        }

        static class MemoryPool
        {
            [ThreadStatic]
            static byte[] buffer = null;

            public static byte[] GetBuffer()
            {
                if (buffer == null)
                {
                    buffer = new byte[65536];
                }
                return buffer;
            }
        }
    }
}