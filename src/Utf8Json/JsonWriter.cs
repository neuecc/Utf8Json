using System;
using System.Collections.Generic;
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
        byte[] buffer;
        int offset;

        public JsonWriter(byte[] initialBuffer)
        {
            this.buffer = initialBuffer;
            this.offset = 0;
        }

        public ArraySegment<byte> GetBuffer()
        {
            return new ArraySegment<byte>(buffer, 0, offset);
        }

        public byte[] ToUtf8ByteArray()
        {
            return BinaryUtil.FastCloneWithResize(buffer, offset);
        }

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
        public void WriteRawUnsafe(byte rawValue)
        {
            buffer[offset++] = rawValue;
        }

        public void WriteBeginArray()
        {
            BinaryUtil.EnsureCapacity(ref buffer, offset, 1);
            buffer[offset++] = (byte)'[';
        }

        public void WriteEndArray()
        {
            BinaryUtil.EnsureCapacity(ref buffer, offset, 1);
            buffer[offset++] = (byte)']';
        }

        public void WriteBeginObject()
        {
            BinaryUtil.EnsureCapacity(ref buffer, offset, 1);
            buffer[offset++] = (byte)'{';
        }

        public void WriteEndObject()
        {
            BinaryUtil.EnsureCapacity(ref buffer, offset, 1);
            buffer[offset++] = (byte)'}';
        }

        public void WriteValueSeparator()
        {
            BinaryUtil.EnsureCapacity(ref buffer, offset, 1);
            buffer[offset++] = (byte)',';
        }

        public void WriteNameSeparator()
        {
            BinaryUtil.EnsureCapacity(ref buffer, offset, 1);
            buffer[offset++] = (byte)':';
        }

        /// <summary>WriteString + WriteNameSeparator</summary>
        public void WritePropertyName(string propertyName)
        {
            WriteString(propertyName);
            WriteNameSeparator();
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

        public void WriteSingle(Single value)
        {
            Utf8Json.Internal.DoubleConversion.DoubleToStringConverter.GetBytes(ref buffer, offset, value);
        }

        public void WriteDouble(double value)
        {
            Utf8Json.Internal.DoubleConversion.DoubleToStringConverter.GetBytes(ref buffer, offset, value);
        }

        public void WriteByte(byte value)
        {
            WriteUInt64((ulong)value);
        }

        public void WriteUInt16(ushort value)
        {
            WriteUInt64((ulong)value);
        }
        public void WriteUInt32(uint value)
        {
            WriteUInt64((ulong)value);
        }

        public void WriteUInt64(ulong value)
        {
            ulong num1 = value, num2, num3, num4, num5, div;

            if (num1 < 10000)
            {
                if (num1 < 10) { EnsureCapacity(1); goto L1; }
                if (num1 < 100) { EnsureCapacity(2); goto L2; }
                if (num1 < 1000) { EnsureCapacity(3); goto L3; }
                EnsureCapacity(4); goto L4;
            }
            else
            {
                num2 = num1 / 10000;
                num1 -= num2 * 10000;
                if (num2 < 10000)
                {
                    if (num2 < 10) { EnsureCapacity(5); goto L5; }
                    if (num2 < 100) { EnsureCapacity(6); goto L6; }
                    if (num2 < 1000) { EnsureCapacity(7); goto L7; }
                    EnsureCapacity(8); goto L8;
                }
                else
                {
                    num3 = num2 / 10000;
                    num2 -= num3 * 10000;
                    if (num3 < 10000)
                    {
                        if (num3 < 10) { EnsureCapacity(9); goto L9; }
                        if (num3 < 100) { EnsureCapacity(10); goto L10; }
                        if (num3 < 1000) { EnsureCapacity(11); goto L11; }
                        EnsureCapacity(12); goto L12;
                    }
                    else
                    {
                        num4 = num3 / 10000;
                        num3 -= num4 * 10000;
                        if (num4 < 10000)
                        {
                            if (num4 < 10) { EnsureCapacity(13); goto L13; }
                            if (num4 < 100) { EnsureCapacity(14); goto L14; }
                            if (num4 < 1000) { EnsureCapacity(15); goto L15; }
                            EnsureCapacity(16); goto L16;
                        }
                        else
                        {
                            num5 = num4 / 10000;
                            num4 -= num5 * 10000;
                            if (num5 < 10000)
                            {
                                if (num5 < 10) { EnsureCapacity(17); goto L17; }
                                if (num5 < 100) { EnsureCapacity(18); goto L18; }
                                if (num5 < 1000) { EnsureCapacity(19); goto L19; }
                                EnsureCapacity(20); goto L20;
                            }
                            L20:
                            buffer[offset++] = (byte)('0' + (div = (num5 * 8389UL) >> 23));
                            num5 -= div * 1000;
                            L19:
                            buffer[offset++] = (byte)('0' + (div = (num5 * 5243UL) >> 19));
                            num5 -= div * 100;
                            L18:
                            buffer[offset++] = (byte)('0' + (div = (num5 * 6554UL) >> 16));
                            num5 -= div * 10;
                            L17:
                            buffer[offset++] = (byte)('0' + (num5));
                        }
                        L16:
                        buffer[offset++] = (byte)('0' + (div = (num4 * 8389UL) >> 23));
                        num4 -= div * 1000;
                        L15:
                        buffer[offset++] = (byte)('0' + (div = (num4 * 5243UL) >> 19));
                        num4 -= div * 100;
                        L14:
                        buffer[offset++] = (byte)('0' + (div = (num4 * 6554UL) >> 16));
                        num4 -= div * 10;
                        L13:
                        buffer[offset++] = (byte)('0' + (num4));
                    }
                    L12:
                    buffer[offset++] = (byte)('0' + (div = (num3 * 8389UL) >> 23));
                    num3 -= div * 1000;
                    L11:
                    buffer[offset++] = (byte)('0' + (div = (num3 * 5243UL) >> 19));
                    num3 -= div * 100;
                    L10:
                    buffer[offset++] = (byte)('0' + (div = (num3 * 6554UL) >> 16));
                    num3 -= div * 10;
                    L9:
                    buffer[offset++] = (byte)('0' + (num3));
                }
                L8:
                buffer[offset++] = (byte)('0' + (div = (num2 * 8389UL) >> 23));
                num2 -= div * 1000;
                L7:
                buffer[offset++] = (byte)('0' + (div = (num2 * 5243UL) >> 19));
                num2 -= div * 100;
                L6:
                buffer[offset++] = (byte)('0' + (div = (num2 * 6554UL) >> 16));
                num2 -= div * 10;
                L5:
                buffer[offset++] = (byte)('0' + (num2));
            }
            L4:
            buffer[offset++] = (byte)('0' + (div = (num1 * 8389UL) >> 23));
            num1 -= div * 1000;
            L3:
            buffer[offset++] = (byte)('0' + (div = (num1 * 5243UL) >> 19));
            num1 -= div * 100;
            L2:
            buffer[offset++] = (byte)('0' + (div = (num1 * 6554UL) >> 16));
            num1 -= div * 10;
            L1:
            buffer[offset++] = (byte)('0' + (num1));
        }

        public void WriteSByte(sbyte value)
        {
            WriteInt64((long)value);
        }

        public void WriteInt16(short value)
        {
            WriteInt64((long)value);
        }

        public void WriteInt32(int value)
        {
            WriteInt64((long)value);
        }

        public void WriteInt64(long value)
        {
            long num1 = value, num2, num3, num4, num5, div;

            if (value < 0)
            {
                if (value == long.MinValue) // -9223372036854775808
                {
                    EnsureCapacity(20);
                    buffer[offset++] = (byte)'-';
                    buffer[offset++] = (byte)'9';
                    buffer[offset++] = (byte)'2';
                    buffer[offset++] = (byte)'2';
                    buffer[offset++] = (byte)'3';
                    buffer[offset++] = (byte)'3';
                    buffer[offset++] = (byte)'7';
                    buffer[offset++] = (byte)'2';
                    buffer[offset++] = (byte)'0';
                    buffer[offset++] = (byte)'3';
                    buffer[offset++] = (byte)'6';
                    buffer[offset++] = (byte)'8';
                    buffer[offset++] = (byte)'5';
                    buffer[offset++] = (byte)'4';
                    buffer[offset++] = (byte)'7';
                    buffer[offset++] = (byte)'7';
                    buffer[offset++] = (byte)'5';
                    buffer[offset++] = (byte)'8';
                    buffer[offset++] = (byte)'0';
                    buffer[offset++] = (byte)'8';
                    return;
                }

                EnsureCapacity(1);
                buffer[offset++] = (byte)'-';
                num1 = unchecked(-value);
            }

            // WriteUInt64(inlined)

            if (num1 < 10000)
            {
                if (num1 < 10) { EnsureCapacity(1); goto L1; }
                if (num1 < 100) { EnsureCapacity(2); goto L2; }
                if (num1 < 1000) { EnsureCapacity(3); goto L3; }
                EnsureCapacity(4); goto L4;
            }
            else
            {
                num2 = num1 / 10000;
                num1 -= num2 * 10000;
                if (num2 < 10000)
                {
                    if (num2 < 10) { EnsureCapacity(5); goto L5; }
                    if (num2 < 100) { EnsureCapacity(6); goto L6; }
                    if (num2 < 1000) { EnsureCapacity(7); goto L7; }
                    EnsureCapacity(8); goto L8;
                }
                else
                {
                    num3 = num2 / 10000;
                    num2 -= num3 * 10000;
                    if (num3 < 10000)
                    {
                        if (num3 < 10) { EnsureCapacity(9); goto L9; }
                        if (num3 < 100) { EnsureCapacity(10); goto L10; }
                        if (num3 < 1000) { EnsureCapacity(11); goto L11; }
                        EnsureCapacity(12); goto L12;
                    }
                    else
                    {
                        num4 = num3 / 10000;
                        num3 -= num4 * 10000;
                        if (num4 < 10000)
                        {
                            if (num4 < 10) { EnsureCapacity(13); goto L13; }
                            if (num4 < 100) { EnsureCapacity(14); goto L14; }
                            if (num4 < 1000) { EnsureCapacity(15); goto L15; }
                            EnsureCapacity(16); goto L16;
                        }
                        else
                        {
                            num5 = num4 / 10000;
                            num4 -= num5 * 10000;
                            if (num5 < 10000)
                            {
                                if (num5 < 10) { EnsureCapacity(17); goto L17; }
                                if (num5 < 100) { EnsureCapacity(18); goto L18; }
                                if (num5 < 1000) { EnsureCapacity(19); goto L19; }
                                EnsureCapacity(20); goto L20;
                            }
                            L20:
                            buffer[offset++] = (byte)('0' + (div = (num5 * 8389L) >> 23));
                            num5 -= div * 1000;
                            L19:
                            buffer[offset++] = (byte)('0' + (div = (num5 * 5243L) >> 19));
                            num5 -= div * 100;
                            L18:
                            buffer[offset++] = (byte)('0' + (div = (num5 * 6554L) >> 16));
                            num5 -= div * 10;
                            L17:
                            buffer[offset++] = (byte)('0' + (num5));
                        }
                        L16:
                        buffer[offset++] = (byte)('0' + (div = (num4 * 8389L) >> 23));
                        num4 -= div * 1000;
                        L15:
                        buffer[offset++] = (byte)('0' + (div = (num4 * 5243L) >> 19));
                        num4 -= div * 100;
                        L14:
                        buffer[offset++] = (byte)('0' + (div = (num4 * 6554L) >> 16));
                        num4 -= div * 10;
                        L13:
                        buffer[offset++] = (byte)('0' + (num4));
                    }
                    L12:
                    buffer[offset++] = (byte)('0' + (div = (num3 * 8389L) >> 23));
                    num3 -= div * 1000;
                    L11:
                    buffer[offset++] = (byte)('0' + (div = (num3 * 5243L) >> 19));
                    num3 -= div * 100;
                    L10:
                    buffer[offset++] = (byte)('0' + (div = (num3 * 6554L) >> 16));
                    num3 -= div * 10;
                    L9:
                    buffer[offset++] = (byte)('0' + (num3));
                }
                L8:
                buffer[offset++] = (byte)('0' + (div = (num2 * 8389L) >> 23));
                num2 -= div * 1000;
                L7:
                buffer[offset++] = (byte)('0' + (div = (num2 * 5243L) >> 19));
                num2 -= div * 100;
                L6:
                buffer[offset++] = (byte)('0' + (div = (num2 * 6554L) >> 16));
                num2 -= div * 10;
                L5:
                buffer[offset++] = (byte)('0' + (num2));
            }
            L4:
            buffer[offset++] = (byte)('0' + (div = (num1 * 8389L) >> 23));
            num1 -= div * 1000;
            L3:
            buffer[offset++] = (byte)('0' + (div = (num1 * 5243L) >> 19));
            num1 -= div * 100;
            L2:
            buffer[offset++] = (byte)('0' + (div = (num1 * 6554L) >> 16));
            num1 -= div * 10;
            L1:
            buffer[offset++] = (byte)('0' + (num1));
        }

        public void WriteString(string value)
        {
            // single-path escape

            // nonescaped-ensure
            var startoffset = offset;
            var max = StringEncoding.UTF8.GetMaxByteCount(value.Length) + 2;
            BinaryUtil.EnsureCapacity(ref buffer, startoffset, max);

            var from = 0;
            var to = value.Length;

            buffer[offset++] = (byte)'\"';

            // for JIT Optimization, for-loop str.Length)
            for (int i = 0; i < value.Length; i++)
            {
                byte escapeChar = default(byte);
                switch ((byte)value[i])
                {
                    case (byte)'"':
                        escapeChar = (byte)'"';
                        break;
                    case (byte)'\\':
                        escapeChar = (byte)'\\';
                        break;
                    case (byte)'\b':
                        escapeChar = (byte)'b';
                        break;
                    case (byte)'\f':
                        escapeChar = (byte)'f';
                        break;
                    case (byte)'\n':
                        escapeChar = (byte)'n';
                        break;
                    case (byte)'\r':
                        escapeChar = (byte)'r';
                        break;
                    case (byte)'\t':
                        escapeChar = (byte)'t';
                        break;
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 11:
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
                    case 48:
                    case 49:
                    case 50:
                    case 51:
                    case 52:
                    case 53:
                    case 54:
                    case 55:
                    case 56:
                    case 57:
                    case 58:
                    case 59:
                    case 60:
                    case 61:
                    case 62:
                    case 63:
                    case 64:
                    case 65:
                    case 66:
                    case 67:
                    case 68:
                    case 69:
                    case 70:
                    case 71:
                    case 72:
                    case 73:
                    case 74:
                    case 75:
                    case 76:
                    case 77:
                    case 78:
                    case 79:
                    case 80:
                    case 81:
                    case 82:
                    case 83:
                    case 84:
                    case 85:
                    case 86:
                    case 87:
                    case 88:
                    case 89:
                    case 90:
                    case 91:
                    default:
                        continue;
                }

                max += 2;
                BinaryUtil.EnsureCapacity(ref buffer, startoffset, max); // check +escape capacity

                offset += StringEncoding.UTF8.GetBytes(value, from, i - from, buffer, offset);
                from = i + 1;
                buffer[offset++] = (byte)'\\';
                buffer[offset++] = escapeChar;
            }

            if (from != value.Length)
            {
                offset += StringEncoding.UTF8.GetBytes(value, from, value.Length - from, buffer, offset);
            }

            buffer[offset++] = (byte)'\"';
        }
    }
}