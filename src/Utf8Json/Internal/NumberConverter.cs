using System;
using System.Collections.Generic;
using System.Text;
using Utf8Json.Internal.DoubleConversion;

namespace Utf8Json.Internal
{
    /// <summary>
    /// zero-allocate itoa, dtoa, atoi, atod converters.
    /// </summary>
    public static class NumberConverter
    {
        public static bool IsNumber(byte c)
        {
            return (byte)'0' <= c && c <= (byte)'9';
        }

        public static sbyte ReadSByte(byte[] bytes, int offset, out int readCount)
        {
            return checked((sbyte)ReadInt64(bytes, offset, out readCount));
        }

        public static short ReadInt16(byte[] bytes, int offset, out int readCount)
        {
            return checked((short)ReadInt64(bytes, offset, out readCount));
        }

        public static int ReadInt32(byte[] bytes, int offset, out int readCount)
        {
            return checked((int)ReadInt64(bytes, offset, out readCount));
        }

        public static long ReadInt64(byte[] bytes, int offset, out int readCount)
        {
            var value = 0L;
            var sign = 1;

            if (bytes[offset] == '-')
            {
                sign = -1;
                offset++;
            }

            for (int i = offset; i < bytes.Length; i++)
            {
                if (!IsNumber(bytes[i]))
                {
                    readCount = i - offset;
                    goto END;
                }

                // long.MinValue causes overflow so use unchecked.
                value = unchecked(value * 10 + (bytes[i] - '0'));
            }
            readCount = bytes.Length - offset;

            END:
            return unchecked(value * sign);
        }

        public static byte ReadByte(byte[] bytes, int offset, out int readCount)
        {
            return checked((byte)ReadUInt64(bytes, offset, out readCount));
        }

        public static ushort ReadUInt16(byte[] bytes, int offset, out int readCount)
        {
            return checked((ushort)ReadUInt64(bytes, offset, out readCount));
        }

        public static uint ReadUInt32(byte[] bytes, int offset, out int readCount)
        {
            return checked((uint)ReadUInt64(bytes, offset, out readCount));
        }

        public static ulong ReadUInt64(byte[] bytes, int offset, out int readCount)
        {
            var value = 0UL;

            for (int i = offset; i < bytes.Length; i++)
            {
                if (!IsNumber(bytes[i]))
                {
                    readCount = i - offset;
                    goto END;
                }

                value = checked(value * 10 + (ulong)(bytes[i] - '0'));
            }
            readCount = bytes.Length - offset;

            END:
            return value;
        }


        public static float ReadSingle(byte[] bytes, int offset, out int readCount)
        {
            return StringToDoubleConverter.ToSingle(bytes, offset, out readCount);
        }

        public static double ReadDouble(byte[] bytes, int offset, out int readCount)
        {
            return StringToDoubleConverter.ToDouble(bytes, offset, out readCount);
        }

        // TODO:write integer api

        public static int WriteSingle(ref byte[] bytes, int offset, float value)
        {
            return DoubleToStringConverter.GetBytes(ref bytes, offset, value);
        }

        public static int WriteDouble(ref byte[] bytes, int offset, double value)
        {
            return DoubleToStringConverter.GetBytes(ref bytes, offset, value);
        }
    }
}
