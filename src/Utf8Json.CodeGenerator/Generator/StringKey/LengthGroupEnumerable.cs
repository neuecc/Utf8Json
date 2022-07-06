﻿using System;
using System.Collections;
using System.Collections.Generic;
#if UNIVERSAL
using Utf8Json.UniversalCodeGenerator;
#endif

namespace Utf8Json.CodeGenerator.Generator.StringKey
{
    public struct LengthGroupEnumerable : IEnumerable<ReadOnlyMemory<ValueTuple<byte[], MemberSerializationInfo>>>
    {
        private readonly ReadOnlyMemory<ValueTuple<byte[], MemberSerializationInfo>> memory;

        public LengthGroupEnumerable(ReadOnlyMemory<(byte[], MemberSerializationInfo)> memory)
        {
            this.memory = memory;
        }

        public struct Enumerator : IEnumerator<ReadOnlyMemory<ValueTuple<byte[], MemberSerializationInfo>>>
        {
            private ReadOnlyMemory<ValueTuple<byte[], MemberSerializationInfo>> rest;

            public Enumerator(ReadOnlyMemory<ValueTuple<byte[], MemberSerializationInfo>> enumerable)
            {
                Current = ReadOnlyMemory<(byte[], MemberSerializationInfo)>.Empty;
                rest = enumerable;
            }

            public bool MoveNext()
            {
                if (rest.IsEmpty)
                {
                    return false;
                }

                var restSpan = rest.Span;
                var length = restSpan[0].Item1.Length;
                for (var index = 1; index < restSpan.Length; index++)
                {
                    ref readonly var valueTuple = ref restSpan[index];
                    if (length == valueTuple.Item1.Length)
                    {
                        continue;
                    }

                    Current = rest.Slice(0, index);
                    rest = rest.Slice(index);
                    return true;
                }

                Current = rest;
                rest = ReadOnlyMemory<(byte[], MemberSerializationInfo)>.Empty;
                return true;
            }

            public void Reset()
            {
                throw new InvalidOperationException();
            }

            public ReadOnlyMemory<(byte[], MemberSerializationInfo)> Current { get; private set; }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }
        }

        public Enumerator GetEnumerator() => new Enumerator(memory);

        IEnumerator<ReadOnlyMemory<(byte[], MemberSerializationInfo)>> IEnumerable<ReadOnlyMemory<(byte[], MemberSerializationInfo)>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}