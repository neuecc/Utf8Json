using System;
using System.IO;
using Utf8Json.Internal;
using Utf8Json.Resolvers;

namespace Utf8Json
{
    /// <summary>
    /// High-Level API of Utf8Json.
    /// </summary>
    public static partial class JsonSerializer
    {
        static IJsonFormatterResolver defaultResolver;

        /// <summary>
        /// FormatterResolver that used resolver less overloads. If does not set it, used StandardResolver.Default.
        /// </summary>
        public static IJsonFormatterResolver DefaultResolver
        {
            get
            {
                if (defaultResolver == null)
                {
                    defaultResolver = StandardResolver.Default;
                }

                return defaultResolver;
            }
        }

        /// <summary>
        /// Is resolver decided?
        /// </summary>
        public static bool IsInitialized
        {
            get
            {
                return defaultResolver != null;
            }
        }

        /// <summary>
        /// Set default resolver of Utf8Json APIs.
        /// </summary>
        /// <param name="resolver"></param>
        public static void SetDefaultResolver(IJsonFormatterResolver resolver)
        {
            defaultResolver = resolver;
        }

        /// <summary>
        /// Serialize to binary with default resolver.
        /// </summary>
        public static byte[] Serialize<T>(T obj)
        {
            return Serialize(obj, defaultResolver);
        }

        /// <summary>
        /// Serialize to binary with specified resolver.
        /// </summary>
        public static byte[] Serialize<T>(T value, IJsonFormatterResolver resolver)
        {
            if (resolver == null) resolver = DefaultResolver;

            var writer = new JsonWriter(MemoryPool.GetBuffer());
            var formatter = resolver.GetFormatterWithVerify<T>();
            formatter.Serialize(ref writer, value, resolver);
            return writer.ToUtf8ByteArray();
        }

        public static void Serialize<T>(ref JsonWriter writer, T value)
        {
            Serialize<T>(ref writer, value, defaultResolver);
        }

        public static void Serialize<T>(ref JsonWriter writer, T value, IJsonFormatterResolver resolver)
        {
            if (resolver == null) resolver = DefaultResolver;

            var formatter = resolver.GetFormatterWithVerify<T>();
            formatter.Serialize(ref writer, value, resolver);
        }

        /// <summary>
        /// Serialize to stream.
        /// </summary>
        public static void Serialize<T>(Stream stream, T value)
        {
            Serialize(stream, value, defaultResolver);
        }

        /// <summary>
        /// Serialize to stream with specified resolver.
        /// </summary>
        public static void Serialize<T>(Stream stream, T value, IJsonFormatterResolver resolver)
        {
            if (resolver == null) resolver = DefaultResolver;

            var buffer = SerializeUnsafe(value, resolver);
            stream.Write(buffer.Array, buffer.Offset, buffer.Count);
        }

        /// <summary>
        /// Serialize to binary. Get the raw memory pool byte[]. The result can not share across thread and can not hold, so use quickly.
        /// </summary>
        public static ArraySegment<byte> SerializeUnsafe<T>(T obj)
        {
            return SerializeUnsafe(obj, defaultResolver);
        }

        /// <summary>
        /// Serialize to binary with specified resolver. Get the raw memory pool byte[]. The result can not share across thread and can not hold, so use quickly.
        /// </summary>
        public static ArraySegment<byte> SerializeUnsafe<T>(T value, IJsonFormatterResolver resolver)
        {
            if (resolver == null) resolver = DefaultResolver;

            var writer = new JsonWriter(MemoryPool.GetBuffer());
            var formatter = resolver.GetFormatterWithVerify<T>();
            formatter.Serialize(ref writer, value, resolver);
            return writer.GetBuffer();
        }

        /// <summary>
        /// Serialize to JsonString.
        /// </summary>
        public static string ToJsonString<T>(T value)
        {
            return ToJsonString(value, defaultResolver);
        }

        /// <summary>
        /// Serialize to JsonString with specified resolver.
        /// </summary>
        public static string ToJsonString<T>(T value, IJsonFormatterResolver resolver)
        {
            if (resolver == null) resolver = DefaultResolver;

            var writer = new JsonWriter(MemoryPool.GetBuffer());
            var formatter = resolver.GetFormatterWithVerify<T>();
            formatter.Serialize(ref writer, value, resolver);
            return writer.ToString();
        }

        public static T Deserialize<T>(string json)
        {
            return Deserialize<T>(json, defaultResolver);
        }

        public static T Deserialize<T>(string json, IJsonFormatterResolver resolver)
        {
            return Deserialize<T>(StringEncoding.UTF8.GetBytes(json), resolver);
        }

        public static T Deserialize<T>(byte[] bytes)
        {
            return Deserialize<T>(bytes, defaultResolver);
        }

        public static T Deserialize<T>(byte[] bytes, IJsonFormatterResolver resolver)
        {
            return Deserialize<T>(bytes, 0, resolver);
        }

        public static T Deserialize<T>(byte[] bytes, int offset)
        {
            return Deserialize<T>(bytes, offset, defaultResolver);
        }

        public static T Deserialize<T>(byte[] bytes, int offset, IJsonFormatterResolver resolver)
        {
            if (resolver == null) resolver = DefaultResolver;

            var reader = new JsonReader(bytes, offset);
            var formatter = resolver.GetFormatterWithVerify<T>();
            return formatter.Deserialize(ref reader, resolver);
        }

        public static T Deserialize<T>(ref JsonReader reader)
        {
            return Deserialize<T>(ref reader, defaultResolver);
        }

        public static T Deserialize<T>(ref JsonReader reader, IJsonFormatterResolver resolver)
        {
            if (resolver == null) resolver = DefaultResolver;

            var formatter = resolver.GetFormatterWithVerify<T>();
            return formatter.Deserialize(ref reader, resolver);
        }

        public static T Deserialize<T>(Stream stream)
        {
            return Deserialize<T>(stream, defaultResolver);
        }

        public static T Deserialize<T>(Stream stream, IJsonFormatterResolver resolver)
        {
            if (resolver == null) resolver = DefaultResolver;

#if NETSTANDARD && !NET45
            var ms = stream as MemoryStream;
            if (ms != null)
            {
                ArraySegment<byte> buf2;
                if (ms.TryGetBuffer(out buf2))
                {
                    // when token is number, can not use from pool(can not find end line).
                    var token = new JsonReader(buf2.Array, buf2.Offset).GetCurrentJsonToken();
                    if (token == JsonToken.Number)
                    {
                        var buf3 = new byte[buf2.Count];
                        Buffer.BlockCopy(buf2.Array, buf2.Offset, buf3, 0, buf3.Length);
                        return Deserialize<T>(buf3, 0, resolver);
                    }

                    return Deserialize<T>(buf2.Array, buf2.Offset, resolver);
                }
            }
#endif
            {
                var buf = MemoryPool.GetBuffer();
                var len = FillFromStream(stream, ref buf);

                // when token is number, can not use from pool(can not find end line).
                var token = new JsonReader(buf).GetCurrentJsonToken();
                if (token == JsonToken.Number)
                {
                    buf = BinaryUtil.FastCloneWithResize(buf, len);
                }

                return Deserialize<T>(buf, resolver);
            }
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