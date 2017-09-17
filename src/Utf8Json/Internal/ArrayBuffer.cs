using System;

namespace Utf8Json.Internal
{
    public struct ArrayBuffer<T>
    {
        public T[] Buffer;
        public int Size;

        public ArrayBuffer(int initialSize = 4)
        {
            this.Buffer = new T[initialSize];
            this.Size = 0;
        }

        public void Add(T value)
        {
            if (this.Buffer.Length > Size)
            {
                Array.Resize(ref Buffer, Size * 2);
            }

            Buffer[Size++] = value;
        }

        public T[] ToArray()
        {
            var result = new T[Size];
            Array.Copy(Buffer, result, Size);
            return result;
        }

        public void Clear(int clearLength)
        {
            Array.Clear(Buffer, 0, Math.Min(Buffer.Length, clearLength));
            this.Size = 0;
        }
    }
}