using System;
using System.Collections.Generic;

namespace Utf8Json.CodeGenerator.Generator.StringKey
{
    public class StringKeySorter<T> : IComparer<ValueTuple<byte[], T>>
    {
        public int Compare((byte[], T) x, (byte[], T) y)
        {
            return Compare(x.Item1, y.Item1);
        }

        private static int Compare(byte[] x, byte[] y)
        {
            var c = x.Length.CompareTo(y.Length);
            if (c != 0)
            {
                return c;
            }

            var length = x.Length;
            var rest = length % 8;
            c = CompareShorten(x, y, length - rest);
            if (c != 0)
            {
                return c;
            }

            if (rest == 0)
            {
                return 0;
            }

            ulong xValue = x[length - 1];
            ulong yValue = y[length - 1];

            for (var i = 1; i < rest; i++)
            {
                xValue <<= 8;
                xValue |= x[length - 1 - i];
                yValue <<= 8;
                yValue |= y[length - 1 - i];
            }

            return xValue.CompareTo(yValue);
        }

        private static int CompareShorten(byte[] x, byte[] y, int count)
        {
            int offset = default;
            while (count != 0)
            {
                var xValue = ReadUInt64LittleEndian(x, offset);
                var yValue = ReadUInt64LittleEndian(y, offset);
                var c = xValue.CompareTo(yValue);
                if (c != 0)
                {
                    return c;
                }

                offset += 8;
                count -= 8;
            }

            return 0;
        }

        private static ulong ReadUInt64LittleEndian(byte[] array, int offset)
        {
            ulong value = array[offset + 7];
            value <<= 8;
            value |= array[offset + 6];
            value <<= 8;
            value |= array[offset + 5];
            value <<= 8;
            value |= array[offset + 4];
            value <<= 8;
            value |= array[offset + 3];
            value <<= 8;
            value |= array[offset + 2];
            value <<= 8;
            value |= array[offset + 1];
            value <<= 8;
            value |= array[offset];

            return value;
        }
    }
}