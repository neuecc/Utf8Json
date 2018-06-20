#if NETSTANDARD

using System.Runtime.CompilerServices;

namespace Utf8Json.Internal
{
    // for string key property name write optimization.

    public static class UnsafeMemory
    {
        // public static readonly bool Is32Bit = (IntPtr.Size == 4);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteRaw(ref JsonWriter writer, byte[] src)
        {
            BinaryUtil.EnsureCapacity(ref writer.buffer, writer.offset, src.Length);
            if (src.Length > 0) { Unsafe.CopyBlockUnaligned(ref writer.buffer[writer.offset], ref src[0], (uint)src.Length); }
            writer.offset += src.Length;
        }
    }
}

#endif