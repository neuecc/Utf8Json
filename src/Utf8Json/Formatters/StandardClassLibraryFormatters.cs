using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Utf8Json.Formatters.Internal;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
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

    public sealed class NullableStringFormatter : IJsonFormatter<string>
    {
        public static readonly IJsonFormatter<string> Default = new NullableStringFormatter();

        public void Serialize(ref JsonWriter writer, string value, IJsonFormatterResolver formatterResolver)
        {
            writer.WriteString(value);
        }

        public string Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            return reader.ReadString();
        }
    }

    public sealed class NullableStringArrayFormatter : IJsonFormatter<string[]>
    {
        public static readonly NullableStringArrayFormatter Default = new NullableStringArrayFormatter();

        public void Serialize(ref JsonWriter writer, string[] value, IJsonFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteBeginArray();

                if (value.Length != 0)
                {
                    writer.WriteString(value[0]);
                }
                for (int i = 1; i < value.Length; i++)
                {
                    writer.WriteValueSeparator();
                    writer.WriteString(value[i]);
                }

                writer.WriteEndArray();
            }
        }

        public string[] Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull())
            {
                return null;
            }
            else
            {
                reader.ReadIsBeginArrayWithVerify();
                var array = new string[4];
                var count = 0;
                while (!reader.ReadIsEndArrayWithSkipValueSeparator(ref count))
                {
                    if (array.Length < count)
                    {
                        Array.Resize(ref array, count * 2);
                    }
                    array[count - 1] = reader.ReadString();
                }

                Array.Resize(ref array, count);
                return array;
            }
        }
    }

    public sealed class CharFormatter : IJsonFormatter<char>
    {
        public static readonly CharFormatter Default = new CharFormatter();

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

    public sealed class NullableCharFormatter : IJsonFormatter<Char?>
    {
        public static readonly NullableCharFormatter Default = new NullableCharFormatter();

        public void Serialize(ref JsonWriter writer, Char? value, IJsonFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                CharFormatter.Default.Serialize(ref writer, value.Value, formatterResolver);
            }
        }

        public Char? Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull())
            {
                return null;
            }
            else
            {
                return CharFormatter.Default.Deserialize(ref reader, formatterResolver);
            }
        }
    }

    public sealed class CharArrayFormatter : IJsonFormatter<char[]>
    {
        public static readonly CharArrayFormatter Default = new CharArrayFormatter();

        public void Serialize(ref JsonWriter writer, char[] value, IJsonFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteBeginArray();

                if (value.Length != 0)
                {
                    CharFormatter.Default.Serialize(ref writer, value[0], formatterResolver);
                }
                for (int i = 1; i < value.Length; i++)
                {
                    writer.WriteValueSeparator();
                    CharFormatter.Default.Serialize(ref writer, value[i], formatterResolver);
                }

                writer.WriteEndArray();
            }
        }

        public char[] Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull())
            {
                return null;
            }
            else
            {
                reader.ReadIsBeginArrayWithVerify();
                var array = new char[4];
                var count = 0;
                while (!reader.ReadIsEndArrayWithSkipValueSeparator(ref count))
                {
                    if (array.Length < count)
                    {
                        Array.Resize(ref array, count * 2);
                    }
                    array[count - 1] = CharFormatter.Default.Deserialize(ref reader, formatterResolver);
                }

                Array.Resize(ref array, count);
                return array;
            }
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

    public sealed class DecimalFormatter : IJsonFormatter<decimal>
    {
        public static readonly IJsonFormatter<decimal> Default = new DecimalFormatter();

        public void Serialize(ref JsonWriter writer, decimal value, IJsonFormatterResolver formatterResolver)
        {
            writer.WriteString(value.ToString());
        }

        public decimal Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            return decimal.Parse(reader.ReadString(), CultureInfo.InvariantCulture);
        }
    }

    public sealed class UriFormatter : IJsonFormatter<Uri>
    {
        public static readonly IJsonFormatter<Uri> Default = new UriFormatter();

        public void Serialize(ref JsonWriter writer, Uri value, IJsonFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteString(value.ToString());
            }
        }

        public Uri Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull())
            {
                return null;
            }
            else
            {
                return new Uri(reader.ReadString(), UriKind.RelativeOrAbsolute);
            }
        }
    }

    public sealed class VersionFormatter : IJsonFormatter<Version>
    {
        public static readonly IJsonFormatter<Version> Default = new VersionFormatter();

        public void Serialize(ref JsonWriter writer, Version value, IJsonFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteString(value.ToString());
            }
        }

        public Version Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull())
            {
                return null;
            }
            else
            {
                return new Version(reader.ReadString());
            }
        }
    }

    public sealed class KeyValuePairFormatter<TKey, TValue> : IJsonFormatter<KeyValuePair<TKey, TValue>>
    {
        public void Serialize(ref JsonWriter writer, KeyValuePair<TKey, TValue> value, IJsonFormatterResolver formatterResolver)
        {
            writer.WriteBeginObject();

            writer.WriteRaw(StandardClassLibraryFormatterHelper.keyValuePairName[0]);
            formatterResolver.GetFormatterWithVerify<TKey>().Serialize(ref writer, value.Key, formatterResolver);
            writer.WriteRaw(StandardClassLibraryFormatterHelper.keyValuePairName[1]);
            formatterResolver.GetFormatterWithVerify<TValue>().Serialize(ref writer, value.Value, formatterResolver);

            writer.WriteEndObject();
        }

        public KeyValuePair<TKey, TValue> Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull()) throw new InvalidOperationException("Data is Nil, KeyValuePair can not be null.");

            TKey resultKey = default(TKey);
            TValue resultValue = default(TValue);

            var count = 0;
            while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref count))
            {
                var keyString = reader.ReadPropertyNameSegmentUnescaped();
                int key;
#if NETSTANDARD
                StandardClassLibraryFormatterHelper.keyValuePairAutomata.TryGetValue(keyString, out key);
#else
                StandardClassLibraryFormatterHelper.keyValuePairAutomata.TryGetValueSafe(keyString, out key);
#endif

                switch (key)
                {
                    case 0:
                        resultKey = formatterResolver.GetFormatterWithVerify<TKey>().Deserialize(ref reader, formatterResolver);
                        break;
                    case 1:
                        resultValue = formatterResolver.GetFormatterWithVerify<TValue>().Deserialize(ref reader, formatterResolver);
                        break;
                    default:
                        reader.ReadNextBlock();
                        break;
                }
            }

            return new KeyValuePair<TKey, TValue>(resultKey, resultValue);
        }
    }

    public sealed class StringBuilderFormatter : IJsonFormatter<StringBuilder>
    {
        public static readonly IJsonFormatter<StringBuilder> Default = new StringBuilderFormatter();

        public void Serialize(ref JsonWriter writer, StringBuilder value, IJsonFormatterResolver formatterResolver)
        {
            if (value == null) { writer.WriteNull(); return; }
            writer.WriteString(value.ToString());
        }

        public StringBuilder Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull()) return null;
            return new StringBuilder(reader.ReadString());
        }
    }

    // BitArray can be represents other format...
    public sealed class BitArrayFormatter : IJsonFormatter<BitArray>
    {
        public static readonly IJsonFormatter<BitArray> Default = new BitArrayFormatter();

        public void Serialize(ref JsonWriter writer, BitArray value, IJsonFormatterResolver formatterResolver)
        {
            if (value == null) { writer.WriteNull(); return; }

            writer.WriteBeginArray();
            for (int i = 0; i < value.Length; i++)
            {
                writer.WriteBoolean(value[i]);
            }
            writer.WriteEndArray();
        }

        public BitArray Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull()) return null;
            reader.ReadIsBeginArrayWithVerify();
            var c = 0;
            var buffer = new ArrayBuffer<bool>();
            while (!reader.ReadIsEndArrayWithSkipValueSeparator(ref c))
            {
                buffer.Add(reader.ReadBoolean());
            }
            return new BitArray(buffer.ToArray());
        }
    }

    public sealed class BigIntegerFormatter : IJsonFormatter<BigInteger>
    {
        public static readonly IJsonFormatter<BigInteger> Default = new BigIntegerFormatter();

        public void Serialize(ref JsonWriter writer, BigInteger value, IJsonFormatterResolver formatterResolver)
        {
            // JSON.NET writes Integer format, not compatible.
            writer.WriteString(value.ToString());
        }

        public BigInteger Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            var s = reader.ReadString();
            return BigInteger.Parse(s, CultureInfo.InvariantCulture);
        }
    }

    // Convert to [Real, Imaginary]
    public sealed class ComplexFormatter : IJsonFormatter<Complex>
    {
        public static readonly IJsonFormatter<Complex> Default = new ComplexFormatter();

        public void Serialize(ref JsonWriter writer, Complex value, IJsonFormatterResolver formatterResolver)
        {
            writer.WriteBeginArray();
            writer.WriteDouble(value.Real);
            writer.WriteDouble(value.Imaginary);
            writer.WriteEndArray();
        }

        public Complex Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            reader.ReadIsBeginArrayWithVerify();
            var real = reader.ReadDouble();
            reader.ReadIsValueSeparatorWithVerify();
            var imaginary = reader.ReadDouble();
            reader.ReadIsEndArrayWithVerify();

            return new Complex(real, imaginary);
        }
    }

    public sealed class LazyFormatter<T> : IJsonFormatter<Lazy<T>>
    {
        public void Serialize(ref JsonWriter writer, Lazy<T> value, IJsonFormatterResolver formatterResolver)
        {
            if (value == null) { writer.WriteNull(); return; }
            formatterResolver.GetFormatterWithVerify<T>().Serialize(ref writer, value.Value, formatterResolver);
        }

        public Lazy<T> Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull()) return null;

            // deserialize immediately(no delay, because capture byte[] causes memory leak)
            var v = formatterResolver.GetFormatterWithVerify<T>().Deserialize(ref reader, formatterResolver);
            return new Lazy<T>(() => v);
        }
    }

    public sealed class TaskUnitFormatter : IJsonFormatter<Task>
    {
        public static readonly IJsonFormatter<Task> Default = new TaskUnitFormatter();
        static readonly Task CompletedTask = Task.FromResult<object>(null);

        public void Serialize(ref JsonWriter writer, Task value, IJsonFormatterResolver formatterResolver)
        {
            if (value == null) { writer.WriteNull(); return; }

            value.Wait(); // wait!
            writer.WriteNull();
        }

        public Task Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            if (!reader.ReadIsNull()) throw new InvalidOperationException("Invalid input");

            return CompletedTask;
        }
    }

    public sealed class TaskValueFormatter<T> : IJsonFormatter<Task<T>>
    {
        public void Serialize(ref JsonWriter writer, Task<T> value, IJsonFormatterResolver formatterResolver)
        {
            if (value == null) { writer.WriteNull(); return; }

            // value.Result -> wait...!
            formatterResolver.GetFormatterWithVerify<T>().Serialize(ref writer, value.Result, formatterResolver);
        }

        public Task<T> Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull()) return null;

            var v = formatterResolver.GetFormatterWithVerify<T>().Deserialize(ref reader, formatterResolver);
            return Task.FromResult(v);
        }
    }

    public sealed class ValueTaskFormatter<T> : IJsonFormatter<ValueTask<T>>
    {
        public void Serialize(ref JsonWriter writer, ValueTask<T> value, IJsonFormatterResolver formatterResolver)
        {
            // value.Result -> wait...!
            formatterResolver.GetFormatterWithVerify<T>().Serialize(ref writer, value.Result, formatterResolver);
        }

        public ValueTask<T> Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            var v = formatterResolver.GetFormatterWithVerify<T>().Deserialize(ref reader, formatterResolver);
            return new ValueTask<T>(v);
        }
    }
}

namespace Utf8Json.Formatters.Internal
{
    internal static class StandardClassLibraryFormatterHelper
    {
        internal static readonly byte[][] keyValuePairName;
        internal static readonly AutomataDictionary keyValuePairAutomata;

        static StandardClassLibraryFormatterHelper()
        {
            keyValuePairName = new byte[][]
            {
                JsonWriter.GetEncodedPropertyNameWithBeginObject("Key"),
                JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Value"),
            };
            keyValuePairAutomata = new AutomataDictionary
            {
                {JsonWriter.GetEncodedPropertyNameWithoutQuotation("Key"), 0 },
                {JsonWriter.GetEncodedPropertyNameWithoutQuotation("Value"), 1 },
            };
        }
    }
}