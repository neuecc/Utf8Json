using System;
using System.Collections.Generic;
using Utf8Json.Internal;

namespace Utf8Json.Formatters
{
    public class ArrayFormatter<T> : IJsonFormatter<T[]>
    {
        static readonly ArrayPool<T> arrayPool = new ArrayPool<T>(99);

        public void Serialize(ref JsonWriter writer, T[] value, IJsonFormatterResolver formatterResolver)
        {
            writer.WriteBeginArray();
            var formatter = formatterResolver.GetFormatter<T>();
            if (value.Length != 0)
            {
                formatter.Serialize(ref writer, value[0], formatterResolver);
            }
            for (int i = 1; i < value.Length; i++)
            {
                writer.WriteValueSeparator();
                formatter.Serialize(ref writer, value[i], formatterResolver);
            }
            writer.WriteEndArray();
        }

        public T[] Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull()) return null;

            var count = 0;
            var formatter = formatterResolver.GetFormatter<T>();

            var workingArea = arrayPool.Rent();
            try
            {
                var array = workingArea;
                reader.ReadIsBeginArrayWithVerify();
                while (!reader.ReadIsEndArrayWithSkipValueSeparator(ref count))
                {
                    if (array.Length < count)
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
            finally
            {
                arrayPool.Return(workingArea);
            }
        }
    }



    public abstract class CollectionFormatterBase<TElement, TIntermediate, TEnumerator, TCollection> : IJsonFormatter<TCollection>
            where TCollection : class, IEnumerable<TElement>
            where TEnumerator : IEnumerator<TElement>
    {
        public void Serialize(ref JsonWriter writer, TCollection value, IJsonFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteBeginArray();
                var formatter = formatterResolver.GetFormatterWithVerify<TElement>();

                // Unity's foreach struct enumerator causes boxing so iterate manually.
                var e = GetSourceEnumerator(value);
                try
                {
                    var isFirst = true;
                    while (e.MoveNext())
                    {
                        if (isFirst)
                        {
                            isFirst = false;
                        }
                        else
                        {
                            writer.WriteValueSeparator();
                        }
                        formatter.Serialize(ref writer, e.Current, formatterResolver);
                    }
                }
                finally
                {
                    e.Dispose();
                }

                writer.WriteEndArray();
            }
        }

        public TCollection Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull())
            {
                return null;
            }
            else
            {
                var formatter = formatterResolver.GetFormatterWithVerify<TElement>();
                var builder = Create();

                var count = 0;
                reader.ReadIsBeginArrayWithVerify();
                while (!reader.ReadIsEndArrayWithSkipValueSeparator(ref count))
                {
                    Add(ref builder, count - 1, formatter.Deserialize(ref reader, formatterResolver));
                }

                return Complete(ref builder);
            }
        }

        // Some collections can use struct iterator, this is optimization path
        protected abstract TEnumerator GetSourceEnumerator(TCollection source);

        // abstraction for deserialize
        protected abstract TIntermediate Create();
        protected abstract void Add(ref TIntermediate collection, int index, TElement value);
        protected abstract TCollection Complete(ref TIntermediate intermediateCollection);
    }

    public abstract class CollectionFormatterBase<TElement, TIntermediate, TCollection> : CollectionFormatterBase<TElement, TIntermediate, IEnumerator<TElement>, TCollection>
        where TCollection : class, IEnumerable<TElement>
    {
        protected override IEnumerator<TElement> GetSourceEnumerator(TCollection source)
        {
            return source.GetEnumerator();
        }
    }

    public abstract class CollectionFormatterBase<TElement, TCollection> : CollectionFormatterBase<TElement, TCollection, TCollection>
        where TCollection : class, IEnumerable<TElement>
    {
        protected sealed override TCollection Complete(ref TCollection intermediateCollection)
        {
            return intermediateCollection;
        }
    }


    // should deserialize reverse order.
    public sealed class StackFormatter<T> : CollectionFormatterBase<T, ArrayBuffer<T>, Stack<T>.Enumerator, Stack<T>>
    {
        protected override void Add(ref ArrayBuffer<T> collection, int index, T value)
        {
            collection.Add(value);
        }

        protected override ArrayBuffer<T> Create()
        {
            return new ArrayBuffer<T>(4);
        }

        protected override Stack<T>.Enumerator GetSourceEnumerator(Stack<T> source)
        {
            return source.GetEnumerator();
        }

        protected override Stack<T> Complete(ref ArrayBuffer<T> intermediateCollection)
        {
            var bufArray = intermediateCollection.Buffer;
            var stack = new Stack<T>(intermediateCollection.Size);
            for (int i = intermediateCollection.Size - 1; i >= 0; i--)
            {
                stack.Push(bufArray[i]);
            }
            return stack;
        }
    }


    public sealed class InterfaceEnumerableFormatter<T> : CollectionFormatterBase<T, ArrayBuffer<T>, IEnumerable<T>>
    {
        protected override void Add(ref ArrayBuffer<T> collection, int index, T value)
        {
            collection.Add(value);
        }

        protected override ArrayBuffer<T> Create()
        {
            return new ArrayBuffer<T>(4);
        }

        protected override IEnumerable<T> Complete(ref ArrayBuffer<T> intermediateCollection)
        {
            return intermediateCollection.ToArray();
        }
    }


}
