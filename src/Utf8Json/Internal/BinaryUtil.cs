using System;
using System.Collections.Generic;
using System.Text;

#if NETSTANDARD
using System.Runtime.CompilerServices;
#endif

namespace Utf8Json.Internal
{
    public static class BinaryUtil
    {
        const int ArrayMaxSize = 0x7FFFFFC7; // https://msdn.microsoft.com/en-us/library/system.array

#if NETSTANDARD
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void EnsureCapacity(ref byte[] bytes, int offset, int appendLength)
        {
            var newLength = offset + appendLength;

            // If null(most case fisrt time) fill byte.
            if (bytes == null)
            {
                bytes = new byte[newLength];
                return;
            }

            // like MemoryStream.EnsureCapacity
            var current = bytes.Length;
            if (newLength > current)
            {
                EnsureCapacitySlow(ref bytes, offset, appendLength);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void EnsureCapacitySlow(ref byte[] bytes, int offset, int appendLength)
        {
            var newLength = offset + appendLength;
            var current = bytes.Length;
            int num = newLength;
            if (num < 256)
            {
                num = 256;
                FastResize(ref bytes, num);
                return;
            }

            if (current == ArrayMaxSize)
            {
                ThrowMaxCapacity();
            }

            var newSize = unchecked((current * 2));
            if (newSize < 0) // overflow
            {
                num = ArrayMaxSize;
            }
            else
            {
                if (num < newSize)
                {
                    num = newSize;
                }
            }

            FastResize(ref bytes, num);
        }

        private static void ThrowMaxCapacity()
        {
            throw new InvalidOperationException("byte[] size reached maximum size of array(0x7FFFFFC7), can not write to single byte[]. Details: https://msdn.microsoft.com/en-us/library/system.array");

        }

#if NETSTANDARD
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static void FastResize(ref byte[] array, int newSize)
        {
            if (newSize < 0) throw new ArgumentOutOfRangeException("newSize");

            byte[] array2 = array;
            if (array2 == null)
            {
                array = new byte[newSize];
                return;
            }

            if (array2.Length != newSize)
            {
                byte[] array3 = new byte[newSize];
                Unsafe.CopyBlockUnaligned(ref array3[0], ref array2[0], (uint)((array2.Length > newSize) ? newSize : array2.Length));
                array = array3;
            }
        }

#if NETSTANDARD
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public static
#if NETSTANDARD
            unsafe
#endif
            byte[] FastCloneWithResize(byte[] src, int newSize)
        {
            if (newSize < 0) throw new ArgumentOutOfRangeException("newSize");
            if (src.Length < newSize) throw new ArgumentException("length < newSize");

            if (src == null) return new byte[newSize];

            if (newSize == 0)
            {
                return Array.Empty<byte>();
            }

            byte[] dst = new byte[newSize];

            Unsafe.CopyBlockUnaligned(ref dst[0], ref src[0], (uint)newSize);

            return dst;
        }
    }
}
