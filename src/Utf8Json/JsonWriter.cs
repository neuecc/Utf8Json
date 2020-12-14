using System;
using System.Text;
using Utf8Json.Internal;

#if NETSTANDARD
using System.Runtime.CompilerServices;
#endif

namespace Utf8Json
{
    // JSON RFC: https://www.ietf.org/rfc/rfc4627.txt

    public struct JsonWriter
    {
        static readonly byte[] emptyBytes = new byte[0];

        // write direct from UnsafeMemory
#if NETSTANDARD
        internal
#endif
        byte[] buffer;
#if NETSTANDARD
        internal
#endif
        int offset;

        public int CurrentOffset
        {
            get
            {
                return offset;
            }
        }

        public void AdvanceOffset(int offset)
        {
            this.offset += offset;
        }

        public static byte[] GetEncodedPropertyName(string propertyName)
        {
            var writer = new JsonWriter();
            writer.WritePropertyName(propertyName);
            return writer.ToUtf8ByteArray();
        }

        public static byte[] GetEncodedPropertyNameWithPrefixValueSeparator(string propertyName)
        {
            var writer = new JsonWriter();
            writer.WriteValueSeparator();
            writer.WritePropertyName(propertyName);
            return writer.ToUtf8ByteArray();
        }

        public static byte[] GetEncodedPropertyNameWithBeginObject(string propertyName)
        {
            var writer = new JsonWriter();
            writer.WriteBeginObject();
            writer.WritePropertyName(propertyName);
            return writer.ToUtf8ByteArray();
        }

        public static byte[] GetEncodedPropertyNameWithoutQuotation(string propertyName)
        {
            var writer = new JsonWriter();
            writer.WriteString(propertyName); // "propname"
            var buf = writer.GetBuffer();
            var result = new byte[buf.Count - 2];
            Buffer.BlockCopy(buf.Array, buf.Offset + 1, result, 0, result.Length); // without quotation
            return result;
        }

        public JsonWriter(byte[] initialBuffer)
        {
            this.buffer = initialBuffer;
            this.offset = 0;
        }

        public ArraySegment<byte> GetBuffer()
        {
            if (buffer == null) return new ArraySegment<byte>(emptyBytes, 0, 0);
            return new ArraySegment<byte>(buffer, 0, offset);
        }

        public byte[] ToUtf8ByteArray()
        {
            if (buffer == null) return emptyBytes;
            return BinaryUtil.FastCloneWithResize(buffer, offset);
        }

        public override string ToString()
        {
            if (buffer == null) return null;
            return Encoding.UTF8.GetString(buffer, 0, offset);
        }

#if NETSTANDARD
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void EnsureCapacity(int appendLength)
        {
            BinaryUtil.EnsureCapacity(ref buffer, offset, appendLength);
        }

#if NETSTANDARD
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void WriteRaw(byte rawValue)
        {
            BinaryUtil.EnsureCapacity(ref buffer, offset, 1);
            buffer[offset++] = rawValue;
        }

#if NETSTANDARD
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void WriteRaw(byte[] rawValue)
        {
#if NETSTANDARD
            UnsafeMemory.WriteRaw(ref this, rawValue);
#else
            BinaryUtil.EnsureCapacity(ref buffer, offset, rawValue.Length);
            Buffer.BlockCopy(rawValue, 0, buffer, offset, rawValue.Length);
            offset += rawValue.Length;
#endif
        }

#if NETSTANDARD
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void WriteRawUnsafe(byte rawValue)
        {
            buffer[offset++] = rawValue;
        }

#if NETSTANDARD
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void WriteBeginArray()
        {
            BinaryUtil.EnsureCapacity(ref buffer, offset, 1);
            buffer[offset++] = (byte)'[';
        }

#if NETSTANDARD
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void WriteEndArray()
        {
            BinaryUtil.EnsureCapacity(ref buffer, offset, 1);
            buffer[offset++] = (byte)']';
        }

#if NETSTANDARD
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void WriteBeginObject()
        {
            BinaryUtil.EnsureCapacity(ref buffer, offset, 1);
            buffer[offset++] = (byte)'{';
        }

#if NETSTANDARD
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void WriteEndObject()
        {
            BinaryUtil.EnsureCapacity(ref buffer, offset, 1);
            buffer[offset++] = (byte)'}';
        }

#if NETSTANDARD
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void WriteValueSeparator()
        {
            BinaryUtil.EnsureCapacity(ref buffer, offset, 1);
            buffer[offset++] = (byte)',';
        }

        /// <summary>:</summary>
#if NETSTANDARD
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void WriteNameSeparator()
        {
            BinaryUtil.EnsureCapacity(ref buffer, offset, 1);
            buffer[offset++] = (byte)':';
        }

        /// <summary>WriteString + WriteNameSeparator</summary>
#if NETSTANDARD
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void WritePropertyName(string propertyName)
        {
            WriteString(propertyName);
            WriteNameSeparator();
        }

#if NETSTANDARD
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void WriteQuotation()
        {
            BinaryUtil.EnsureCapacity(ref buffer, offset, 1);
            buffer[offset++] = (byte)'\"';
        }

#if NETSTANDARD
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void WriteNull()
        {
            BinaryUtil.EnsureCapacity(ref buffer, offset, 4);
            buffer[offset + 0] = (byte)'n';
            buffer[offset + 1] = (byte)'u';
            buffer[offset + 2] = (byte)'l';
            buffer[offset + 3] = (byte)'l';
            offset += 4;
        }

#if NETSTANDARD
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void WriteBoolean(bool value)
        {
            if (value)
            {
                BinaryUtil.EnsureCapacity(ref buffer, offset, 4);
                buffer[offset + 0] = (byte)'t';
                buffer[offset + 1] = (byte)'r';
                buffer[offset + 2] = (byte)'u';
                buffer[offset + 3] = (byte)'e';
                offset += 4;
            }
            else
            {
                BinaryUtil.EnsureCapacity(ref buffer, offset, 5);
                buffer[offset + 0] = (byte)'f';
                buffer[offset + 1] = (byte)'a';
                buffer[offset + 2] = (byte)'l';
                buffer[offset + 3] = (byte)'s';
                buffer[offset + 4] = (byte)'e';
                offset += 5;
            }
        }

#if NETSTANDARD
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void WriteTrue()
        {
            BinaryUtil.EnsureCapacity(ref buffer, offset, 4);
            buffer[offset + 0] = (byte)'t';
            buffer[offset + 1] = (byte)'r';
            buffer[offset + 2] = (byte)'u';
            buffer[offset + 3] = (byte)'e';
            offset += 4;
        }

#if NETSTANDARD
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void WriteFalse()
        {
            BinaryUtil.EnsureCapacity(ref buffer, offset, 5);
            buffer[offset + 0] = (byte)'f';
            buffer[offset + 1] = (byte)'a';
            buffer[offset + 2] = (byte)'l';
            buffer[offset + 3] = (byte)'s';
            buffer[offset + 4] = (byte)'e';
            offset += 5;
        }

#if NETSTANDARD
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void WriteSingle(Single value)
        {
            offset += Utf8Json.Internal.DoubleConversion.DoubleToStringConverter.GetBytes(ref buffer, offset, value);
        }

#if NETSTANDARD
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void WriteDouble(double value)
        {
            offset += Utf8Json.Internal.DoubleConversion.DoubleToStringConverter.GetBytes(ref buffer, offset, value);
        }

#if NETSTANDARD
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void WriteByte(byte value)
        {
            WriteUInt64((ulong)value);
        }

#if NETSTANDARD
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void WriteUInt16(ushort value)
        {
            WriteUInt64((ulong)value);
        }

#if NETSTANDARD
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void WriteUInt32(uint value)
        {
            WriteUInt64((ulong)value);
        }

        public void WriteUInt64(ulong value)
        {
            offset += NumberConverter.WriteUInt64(ref buffer, offset, value);
        }

#if NETSTANDARD
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void WriteSByte(sbyte value)
        {
            WriteInt64((long)value);
        }

#if NETSTANDARD
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void WriteInt16(short value)
        {
            WriteInt64((long)value);
        }

#if NETSTANDARD
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void WriteInt32(int value)
        {
            WriteInt64((long)value);
        }

        public void WriteInt64(long value)
        {
            offset += NumberConverter.WriteInt64(ref buffer, offset, value);
        }

        public void WriteString(string value)
        {
            if (value == null)
            {
                WriteNull();
                return;
            }

            // single-path escape

            // nonescaped-ensure
            var startoffset = offset;
            var max = StringEncoding.UTF8.GetMaxByteCount(value.Length) + 2;
            BinaryUtil.EnsureCapacity(ref buffer, startoffset, max);

            var from = 0;
            var to = value.Length;

            buffer[offset++] = (byte)'\"';

            // for JIT Optimization, for-loop i < str.Length
            for (int i = 0; i < value.Length; i++)
            {
                bool outputEncodedChar = false;
                byte extendedChar1 = default(byte);
                byte extendedChar2 = default(byte);

                switch (value[i])
                {
                    case '"': extendedChar1 = (byte)'"'; break;
                    case '\\': extendedChar1 = (byte)'\\'; break;
                    case '\b': extendedChar1 = (byte)'b'; break;
                    case '\f': extendedChar1 = (byte)'f'; break;
                    case '\n': extendedChar1 = (byte)'n'; break;
                    case '\r': extendedChar1 = (byte)'r'; break;
                    case '\t': extendedChar1 = (byte)'t'; break;
                    case (char)0x00: outputEncodedChar = true; extendedChar1 = (byte)'0'; extendedChar2 = (byte)'0'; break;
                    case (char)0x01: outputEncodedChar = true; extendedChar1 = (byte)'0'; extendedChar2 = (byte)'1'; break;
                    case (char)0x02: outputEncodedChar = true; extendedChar1 = (byte)'0'; extendedChar2 = (byte)'2'; break;
                    case (char)0x03: outputEncodedChar = true; extendedChar1 = (byte)'0'; extendedChar2 = (byte)'3'; break;
                    case (char)0x04: outputEncodedChar = true; extendedChar1 = (byte)'0'; extendedChar2 = (byte)'4'; break;
                    case (char)0x05: outputEncodedChar = true; extendedChar1 = (byte)'0'; extendedChar2 = (byte)'5'; break;
                    case (char)0x06: outputEncodedChar = true; extendedChar1 = (byte)'0'; extendedChar2 = (byte)'6'; break;
                    case (char)0x07: outputEncodedChar = true; extendedChar1 = (byte)'0'; extendedChar2 = (byte)'7'; break;
                    case (char)0x0b: outputEncodedChar = true; extendedChar1 = (byte)'0'; extendedChar2 = (byte)'b'; break;
                    case (char)0x0e: outputEncodedChar = true; extendedChar1 = (byte)'0'; extendedChar2 = (byte)'e'; break;
                    case (char)0x0f: outputEncodedChar = true; extendedChar1 = (byte)'0'; extendedChar2 = (byte)'f'; break;
                    case (char)0x10: outputEncodedChar = true; extendedChar1 = (byte)'1'; extendedChar2 = (byte)'0'; break;
                    case (char)0x11: outputEncodedChar = true; extendedChar1 = (byte)'1'; extendedChar2 = (byte)'1'; break;
                    case (char)0x12: outputEncodedChar = true; extendedChar1 = (byte)'1'; extendedChar2 = (byte)'2'; break;
                    case (char)0x13: outputEncodedChar = true; extendedChar1 = (byte)'1'; extendedChar2 = (byte)'3'; break;
                    case (char)0x14: outputEncodedChar = true; extendedChar1 = (byte)'1'; extendedChar2 = (byte)'4'; break;
                    case (char)0x15: outputEncodedChar = true; extendedChar1 = (byte)'1'; extendedChar2 = (byte)'5'; break;
                    case (char)0x16: outputEncodedChar = true; extendedChar1 = (byte)'1'; extendedChar2 = (byte)'6'; break;
                    case (char)0x17: outputEncodedChar = true; extendedChar1 = (byte)'1'; extendedChar2 = (byte)'7'; break;
                    case (char)0x18: outputEncodedChar = true; extendedChar1 = (byte)'1'; extendedChar2 = (byte)'8'; break;
                    case (char)0x19: outputEncodedChar = true; extendedChar1 = (byte)'1'; extendedChar2 = (byte)'9'; break;
                    case (char)0x1a: outputEncodedChar = true; extendedChar1 = (byte)'1'; extendedChar2 = (byte)'a'; break;
                    case (char)0x1b: outputEncodedChar = true; extendedChar1 = (byte)'1'; extendedChar2 = (byte)'b'; break;
                    case (char)0x1c: outputEncodedChar = true; extendedChar1 = (byte)'1'; extendedChar2 = (byte)'c'; break;
                    case (char)0x1d: outputEncodedChar = true; extendedChar1 = (byte)'1'; extendedChar2 = (byte)'d'; break;
                    case (char)0x1e: outputEncodedChar = true; extendedChar1 = (byte)'1'; extendedChar2 = (byte)'e'; break;
                    case (char)0x1f: outputEncodedChar = true; extendedChar1 = (byte)'1'; extendedChar2 = (byte)'f'; break;
                    default:
                        continue;
                }

                if (outputEncodedChar)
                {
                    max += 6;
                    BinaryUtil.EnsureCapacity(ref buffer, startoffset, max); // check +escape capacity

                    offset += StringEncoding.UTF8.GetBytes(value, from, i - from, buffer, offset);
                    from = i + 1;

                    buffer[offset++] = (byte)'\\';
                    buffer[offset++] = (byte)'u';
                    buffer[offset++] = (byte)'0';
                    buffer[offset++] = (byte)'0';
                    buffer[offset++] = extendedChar1;
                    buffer[offset++] = extendedChar2;
                }
                else
                {
                    max += 2;
                    BinaryUtil.EnsureCapacity(ref buffer, startoffset, max); // check +escape capacity

                    offset += StringEncoding.UTF8.GetBytes(value, from, i - from, buffer, offset);
                    from = i + 1;
                    buffer[offset++] = (byte) '\\';
                    buffer[offset++] = extendedChar1;
                }
            }

            if (from != value.Length)
            {
                offset += StringEncoding.UTF8.GetBytes(value, from, value.Length - from, buffer, offset);
            }

            buffer[offset++] = (byte)'\"';
        }
    }
}