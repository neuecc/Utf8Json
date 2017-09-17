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
            throw new NotImplementedException();
        }

        // https://github.com/google/double-conversion
        public void WriteDouble(double value)
        {
            // TODO:more efficient way?



            // throw new NotImplementedException();
            var s = value.ToString();
            // Double.
            // Encoding.UTF8.GetBytes(s, 0, s.Length, bytes

        }

        public void WriteByte(byte value)
        {
            int num1 = value, div;

            if (num1 < 10)
            {
                BinaryUtil.EnsureCapacity(ref buffer, offset, 1);
                goto L1;
            }
            if (num1 < 100)
            {
                BinaryUtil.EnsureCapacity(ref buffer, offset, 2);
                goto L2;
            }

            BinaryUtil.EnsureCapacity(ref buffer, offset, 3);
            goto L3;

            L3:
            buffer[offset++] = (byte)('0' + (div = (num1 * 5243) >> 19));
            num1 -= div * 100;

            L2:
            buffer[offset++] = (byte)('0' + (div = (num1 * 6554) >> 16));
            num1 -= div * 10;

            L1:
            buffer[offset++] = (byte)('0' + (num1));
        }

        public void WriteUInt16(ushort value)
        {
            throw new NotImplementedException();
        }
        public void WriteUInt32(uint value)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void WriteInt16(short value)
        {
            throw new NotImplementedException();
        }

        // TODO:Offset?Safe?
        public unsafe void WriteInt32(int value)
        {
            fixed (byte* ptr = buffer)
            {
                byte* p = ptr;
                byte* start = p;

                int num1 = value, num2, num3, div;
                if (value < 0)
                {
                    *p++ = (byte)'-';
                    if (value == -2147483648)
                    {
                        *p++ = (byte)2;
                        *p++ = (byte)1;
                        *p++ = (byte)4;
                        *p++ = (byte)7;
                        *p++ = (byte)4;
                        *p++ = (byte)8;
                        *p++ = (byte)3;
                        *p++ = (byte)6;
                        *p++ = (byte)4;
                        *p++ = (byte)8;
                        //buffer.AddOffset(11);
                        return;
                    }
                    num1 = -num1;
                }
                if (num1 < 10000)
                {
                    if (num1 < 10) goto L1;
                    if (num1 < 100) goto L2;
                    if (num1 < 1000) goto L3;
                }
                else
                {
                    num2 = num1 / 10000;
                    num1 -= num2 * 10000;
                    if (num2 < 10000)
                    {
                        if (num2 < 10) goto L5;
                        if (num2 < 100) goto L6;
                        if (num2 < 1000) goto L7;
                    }
                    else
                    {
                        num3 = num2 / 10000;
                        num2 -= num3 * 10000;
                        if (num3 >= 10)
                        {
                            *p++ = (byte)('0' + (div = (num3 * 6554) >> 16));
                            num3 -= div * 10;
                        }
                        *p++ = (byte)('0' + (num3));
                    }
                    *p++ = (byte)('0' + (div = (num2 * 8389) >> 23));
                    num2 -= div * 1000;
                    L7:
                    *p++ = (byte)('0' + (div = (num2 * 5243) >> 19));
                    num2 -= div * 100;
                    L6:
                    *p++ = (byte)('0' + (div = (num2 * 6554) >> 16));
                    num2 -= div * 10;
                    L5:
                    *p++ = (byte)('0' + (num2));
                }
                *p++ = (byte)('0' + (div = (num1 * 8389) >> 23));
                num1 -= div * 1000;
                L3:
                *p++ = (byte)('0' + (div = (num1 * 5243) >> 19));
                num1 -= div * 100;
                L2:
                *p++ = (byte)('0' + (div = (num1 * 6554) >> 16));
                num1 -= div * 10;
                L1:
                *p++ = (byte)('0' + (num1));
            }
            //buffer.AddOffset((int)(p - start));
        }

        public void WriteInt64(long value)
        {
            throw new NotImplementedException();
        }

        public unsafe void WriteString(string value)
        {
            var max = Encoding.UTF8.GetMaxByteCount(value.Length);
            // TODO:...ensure capacity...
            fixed (byte* p = buffer)
            fixed (char* c = value)
            {
                // escape...

                // nonescape range...

                var count = Encoding.UTF8.GetBytes(c, value.Length, p, max);
                //TODO:addoffset...
            }
        }
    }
}
