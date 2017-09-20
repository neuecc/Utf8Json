using System;
using System.Collections.Generic;
using System.Text;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
    // TODO...
    public class ArrayFormatter<T> : IJsonFormatter<T[]>
    {
        [ThreadStatic]
        static T[] workingArea;

        public void Serialize(ref JsonWriter writer, ref T[] value, IFormatterResolver formatterResolver)
        {
            writer.WriteBeginArray();
            var formatter = formatterResolver.GetFormatter<T>();
            for (int i = 0; i < value.Length; i++)
            {
                if (i != 0) writer.WriteValueSeparator();
                formatter.Serialize(ref writer, ref value[i], formatterResolver);
            }
            writer.WriteEndArray();
        }

        public T[] Deserialize(ref JsonReader reader, IFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull()) return null;

            var count = 0;
            var formatter = formatterResolver.GetFormatter<T>();

            if (workingArea == null)
            {
                workingArea = new T[99];
            }

            var array = workingArea;
            reader.ReadIsBeginArrayWithVerify();
            while (!reader.ReadIsEndArrayWithSkipValueSeparator(ref count))
            {
                if (array.Length > count)
                {
                    Array.Resize<T>(ref array, array.Length * 2);
                }

                array[count - 1] = formatter.Deserialize(ref reader, formatterResolver);
            }

            var result = new T[count];
            Array.Copy(array, result, count);
            Array.Clear(workingArea, 0, Math.Min(count, workingArea.Length));
            return array;
        }
    }
}
