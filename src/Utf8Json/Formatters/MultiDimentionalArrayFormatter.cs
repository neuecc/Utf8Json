using System;
using System.Collections.Generic;
using System.Text;

namespace Utf8Json.Formatters
{
    // multi dimentional array serialize to [[seq], [seq]]

    public sealed class TwoDimentionalArrayFormatter<T> : IJsonFormatter<T[,]>
    {
        public void Serialize(ref JsonWriter writer, T[,] value, IJsonFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                var formatter = formatterResolver.GetFormatterWithVerify<T>();

                var iLength = value.GetLength(0);
                var jLength = value.GetLength(1);

                writer.WriteBeginArray();
                for (int i = 0; i < iLength; i++)
                {
                    if (i != 0) writer.WriteValueSeparator();
                    writer.WriteBeginArray();
                    for (int j = 0; j < jLength; j++)
                    {
                        if (j != 0) writer.WriteValueSeparator();
                        formatter.Serialize(ref writer, value[i, j], formatterResolver);
                    }
                    writer.WriteEndArray();
                }
                writer.WriteEndArray();
            }
        }

        public T[,] Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            throw new NotImplementedException();
            //if (reader.ReadIsNull())
            //{
            //    return null;
            //}
            //else
            //{
            //    var startOffset = offset;
            //    var formatter = formatterResolver.GetFormatterWithVerify<T>();

            //    var len = MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            //    offset += readSize;
            //    if (len != ArrayLength) throw new InvalidOperationException("Invalid T[,] format");

            //    var iLength = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
            //    offset += readSize;

            //    var jLength = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
            //    offset += readSize;

            //    var maxLen = MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
            //    offset += readSize;

            //    var array = new T[iLength, jLength];

            //    var i = 0;
            //    var j = -1;
            //    for (int loop = 0; loop < maxLen; loop++)
            //    {
            //        if (j < jLength - 1)
            //        {
            //            j++;
            //        }
            //        else
            //        {
            //            j = 0;
            //            i++;
            //        }

            //        array[i, j] = formatter.Deserialize(bytes, offset, formatterResolver, out readSize);
            //        offset += readSize;
            //    }

            //    readSize = offset - startOffset;
            //    return array;
            //}
        }
    }

    //public sealed class ThreeDimentionalArrayFormatter<T> : IJsonFormatter<T[,,]>
    //{
    //    const int ArrayLength = 4;

    //    public void Serialize(ref JsonWriter writer, T[,,] value, IJsonFormatterResolver formatterResolver)
    //    {
    //        if (value == null)
    //        {
    //            writer.WriteNull();
    //        }
    //        else
    //        {
    //            var i = value.GetLength(0);
    //            var j = value.GetLength(1);
    //            var k = value.GetLength(2);

    //            var startOffset = offset;
    //            var formatter = formatterResolver.GetFormatterWithVerify<T>();

    //            offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, ArrayLength);
    //            offset += MessagePackBinary.WriteInt32(ref bytes, offset, i);
    //            offset += MessagePackBinary.WriteInt32(ref bytes, offset, j);
    //            offset += MessagePackBinary.WriteInt32(ref bytes, offset, k);

    //            offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, value.Length);
    //            foreach (var item in value)
    //            {
    //                offset += formatter.Serialize(ref bytes, offset, item, formatterResolver);
    //            }

    //            return offset - startOffset;
    //        }
    //    }

    //    public T[,,] Deserialize(JsonReader reader, IJsonFormatterResolver formatterResolver)
    //    {
    //        if (reader.ReadIsNull())
    //        {
    //            readSize = 1;
    //            return null;
    //        }
    //        else
    //        {
    //            var startOffset = offset;
    //            var formatter = formatterResolver.GetFormatterWithVerify<T>();

    //            var len = MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
    //            offset += readSize;
    //            if (len != ArrayLength) throw new InvalidOperationException("Invalid T[,,] format");

    //            var iLength = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
    //            offset += readSize;

    //            var jLength = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
    //            offset += readSize;

    //            var kLength = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
    //            offset += readSize;

    //            var maxLen = MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
    //            offset += readSize;

    //            var array = new T[iLength, jLength, kLength];

    //            var i = 0;
    //            var j = 0;
    //            var k = -1;
    //            for (int loop = 0; loop < maxLen; loop++)
    //            {
    //                if (k < kLength - 1)
    //                {
    //                    k++;
    //                }
    //                else if (j < jLength - 1)
    //                {
    //                    k = 0;
    //                    j++;
    //                }
    //                else
    //                {
    //                    k = 0;
    //                    j = 0;
    //                    i++;
    //                }

    //                array[i, j, k] = formatter.Deserialize(bytes, offset, formatterResolver, out readSize);
    //                offset += readSize;
    //            }

    //            readSize = offset - startOffset;
    //            return array;
    //        }
    //    }
    //}

    //public sealed class FourDimentionalArrayFormatter<T> : IJsonFormatter<T[,,,]>
    //{
    //    const int ArrayLength = 5;

    //    public void Serialize(ref JsonWriter writer, T[,,,] value, IJsonFormatterResolver formatterResolver)
    //    {
    //        if (value == null)
    //        {
    //            writer.WriteNull();
    //        }
    //        else
    //        {
    //            var i = value.GetLength(0);
    //            var j = value.GetLength(1);
    //            var k = value.GetLength(2);
    //            var l = value.GetLength(3);

    //            var startOffset = offset;
    //            var formatter = formatterResolver.GetFormatterWithVerify<T>();

    //            offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, ArrayLength);
    //            offset += MessagePackBinary.WriteInt32(ref bytes, offset, i);
    //            offset += MessagePackBinary.WriteInt32(ref bytes, offset, j);
    //            offset += MessagePackBinary.WriteInt32(ref bytes, offset, k);
    //            offset += MessagePackBinary.WriteInt32(ref bytes, offset, l);

    //            offset += MessagePackBinary.WriteArrayHeader(ref bytes, offset, value.Length);
    //            foreach (var item in value)
    //            {
    //                offset += formatter.Serialize(ref bytes, offset, item, formatterResolver);
    //            }

    //            return offset - startOffset;
    //        }
    //    }

    //    public T[,,,] Deserialize(JsonReader reader, IJsonFormatterResolver formatterResolver)
    //    {
    //        if (reader.ReadIsNull())
    //        {
    //            readSize = 1;
    //            return null;
    //        }
    //        else
    //        {
    //            var startOffset = offset;
    //            var formatter = formatterResolver.GetFormatterWithVerify<T>();

    //            var len = MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
    //            offset += readSize;
    //            if (len != ArrayLength) throw new InvalidOperationException("Invalid T[,,,] format");

    //            var iLength = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
    //            offset += readSize;

    //            var jLength = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
    //            offset += readSize;

    //            var kLength = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
    //            offset += readSize;

    //            var lLength = MessagePackBinary.ReadInt32(bytes, offset, out readSize);
    //            offset += readSize;

    //            var maxLen = MessagePackBinary.ReadArrayHeader(bytes, offset, out readSize);
    //            offset += readSize;

    //            var array = new T[iLength, jLength, kLength, lLength];

    //            var i = 0;
    //            var j = 0;
    //            var k = 0;
    //            var l = -1;
    //            for (int loop = 0; loop < maxLen; loop++)
    //            {
    //                if (l < lLength - 1)
    //                {
    //                    l++;
    //                }
    //                else if (k < kLength - 1)
    //                {
    //                    l = 0;
    //                    k++;
    //                }
    //                else if (j < jLength - 1)
    //                {
    //                    l = 0;
    //                    k = 0;
    //                    j++;
    //                }
    //                else
    //                {
    //                    l = 0;
    //                    k = 0;
    //                    j = 0;
    //                    i++;
    //                }

    //                array[i, j, k, l] = formatter.Deserialize(bytes, offset, formatterResolver, out readSize);
    //                offset += readSize;
    //            }

    //            readSize = offset - startOffset;
    //            return array;
    //        }
    //    }
    //}
}