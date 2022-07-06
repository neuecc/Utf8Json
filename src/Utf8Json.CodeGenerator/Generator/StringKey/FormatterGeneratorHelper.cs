using System;
using System.Buffers.Binary;
using System.Text;
#if UNIVERSAL
using Utf8Json.UniversalCodeGenerator;
#endif

namespace Utf8Json.CodeGenerator.Generator.StringKey
{
    public static class FormatterGeneratorHelper
    {
        public static ValueTuple<byte[], MemberSerializationInfo>[] GetSortedStringKeys(MemberSerializationInfo[] infos)
        {
            var answer = new ValueTuple<byte[], MemberSerializationInfo>[infos.Length];
            for (var i = 0; i < infos.Length; i++)
            {
                var info = infos[i];
                var deserializeHeader = JsonWriter.GetEncodedPropertyNameWithoutQuotation(info.Name);
                answer[i] = (deserializeHeader, info);
            }

            Array.Sort(answer, new StringKeySorter<MemberSerializationInfo>());

            return answer;
        }

        public static string ToStringNewByteArray(byte[] bytes)
        {
            if (bytes is null)
            {
                throw new ArgumentNullException();
            }

            var builder = new StringBuilder((6 * bytes.Length) + 13).Append("new byte[] { ");
            foreach (var b in bytes)
            {
                builder.Append("0x").Append(b.ToString("X2")).Append(", ");
            }

            builder.Append("}");
            return builder.ToString();
        }

        private const string CalcKey = "(ulong)array[offset++] | ((ulong)array[offset++] << 8) | ((ulong)array[offset++] << 16) | ((ulong)array[offset++] << 24) | ((ulong)array[offset++] << 32) | ((ulong)array[offset++] << 40) | ((ulong)array[offset++] << 48) | ((ulong)array[offset++] << 56)";

        public static string EmbedSwitch(this ReadOnlyMemory<ValueTuple<byte[], MemberSerializationInfo>> range, int count, string indent, int lengthUlong)
        {
            var builder = new StringBuilder();
            if (count == 0)
            {
                builder.EmbedSwitch0(range, indent, 0, lengthUlong);
            }
            else if (count <= 4)
            {
                builder.EmbedSwitch(range, count, indent, 0, lengthUlong, "uint", "U");
            }
            else
            {
                builder.EmbedSwitch(range, count, indent, 0, lengthUlong, "ulong", "UL");
            }

            return builder.ToString();
        }

        public static string EmbedSwitchLast(this ReadOnlySpan<(byte[], MemberSerializationInfo)> span, int count, string indent)
        {
            var builder = new StringBuilder();
            if (count <= 4)
            {
                builder.EmbedSwitchLast(span, count, indent, "uint", "U");
            }
            else
            {
                builder.EmbedSwitchLast(span, count, indent, "ulong", "UL");
            }

            return builder.ToString();
        }

        private static void EmbedSwitchLast(this StringBuilder builder, ReadOnlySpan<(byte[], MemberSerializationInfo)> span, int count, string indent, string typeName, string suffix)
        {
            var nextIndent = indent + "    ";
            builder.Append(indent).Append("switch ((").Append(typeName).Append(")array[offset++]");

            for (var i = 1; i < count; i++)
            {
                builder.Append(" | ((").Append(typeName).Append(")array[offset++] << ").Append(i << 3).Append(")");
            }

            builder.Append(")\r\n").Append(indent).Append("{\r\n").Append(nextIndent).Append("default: goto FAIL;\r\n");
            for (var index = 0; index < span.Length; index++)
            {
                var memorySpan = span[index].Item1.AsSpan();

                ulong last = memorySpan[memorySpan.Length - 1];

                for (var i = 1; i < count; i++)
                {
                    last <<= 8;
                    last |= memorySpan[memorySpan.Length - 1 - i];
                }

                var memberInfo = span[index].Item2;
                var thirdIndent = nextIndent + "    ";
                builder.Append(nextIndent).Append("case 0x").Append(last.ToString("X")).Append(suffix)
                    .Append(":\r\n").Append(thirdIndent).EmbedMemberAssignment(memberInfo)
                    .Append("\r\n").Append(thirdIndent).Append("continue;\r\n");
            }

            builder.Append(indent).Append("}");
        }

        private static void EmbedSwitch(this StringBuilder builder, ReadOnlyMemory<(byte[], MemberSerializationInfo)> range, int count, string indent, int indexOfUlongs, int lengthUlong, string typeName, string suffix)
        {
            if (range.Length == 1)
            {
                builder.EmbedSwitchSingle(range.Span[0], count, indent, indexOfUlongs, lengthUlong, typeName, suffix);
                return;
            }

            var nextIndent = indent + "    ";
            var stringKeyULongGroupEnumerable = new ULongGroupEnumerable(range, indexOfUlongs);

            if (!stringKeyULongGroupEnumerable.HasSingleGroup)
            {
                builder.EmbedSwitchMultiGroup(count, indent, indexOfUlongs, lengthUlong, typeName, suffix, nextIndent, stringKeyULongGroupEnumerable);
            }
            else
            {
                builder.EmbedSwitchSingleGroup(range, count, indent, indexOfUlongs, lengthUlong, typeName, suffix);
            }
        }

        private static void EmbedSwitchMultiGroup(this StringBuilder builder, int count, string indent, int indexOfUlongs, int lengthUlong, string typeName, string suffix, string nextIndent, ULongGroupEnumerable stringKeyULongGroupEnumerable)
        {
            builder
                .Append(indent).Append("switch (" + CalcKey + ")\r\n")
                .Append(indent).Append("{\r\n").Append(nextIndent).Append("default: goto FAIL;");

            foreach (var memory in stringKeyULongGroupEnumerable)
            {
                builder.EmbedSwitchMultiGroupEach(count, indexOfUlongs, lengthUlong, typeName, suffix, nextIndent, memory);
            }

            builder.Append("\r\n").Append(indent).Append("}");
        }

        private static void EmbedSwitchMultiGroupEach(this StringBuilder builder, int count, int indexOfUlongs, int lengthUlong, string typeName, string suffix, string nextIndent, ReadOnlyMemory<(byte[], MemberSerializationInfo)> memory)
        {
            var readOnlySpan = memory.Span[0].Item1.AsSpan();
            var value = BinaryPrimitives.ReadUInt64LittleEndian(readOnlySpan.Slice(indexOfUlongs << 3));
            builder.Append("\r\n").Append(nextIndent).EmbedCase(value);

            if (indexOfUlongs == lengthUlong - 1)
            {
                var thirdIndent = nextIndent + "    ";
                builder.EmbedSwitchLast(memory.Span, count, thirdIndent, typeName, suffix);
            }
            else
            {
                builder.EmbedSwitch(memory, count, nextIndent + "    ", indexOfUlongs + 1, lengthUlong, typeName, suffix);
            }
        }

        private static void EmbedSwitchSingleGroup(this StringBuilder builder, ReadOnlyMemory<(byte[], MemberSerializationInfo)> range, int count, string indent, int indexOfUlongs, int lengthUlong, string typeName, string suffix)
        {
            var key = BinaryPrimitives.ReadUInt64LittleEndian(range.Span[0].Item1.AsSpan(indexOfUlongs << 3));
            builder.Append(indent).Append("if ((" + CalcKey + ") != ").EmbedNumberUlong(key)
                .Append(") goto FAIL; ").EmbedComment(key).Append("\r\n");

            if (indexOfUlongs == lengthUlong - 1)
            {
                builder.EmbedSwitchLast(range.Span, count, indent, typeName, suffix);
            }
            else
            {
                builder.EmbedSwitch(range, count, indent, indexOfUlongs + 1, lengthUlong, typeName, suffix);
            }
        }

        private static void EmbedSwitchSingle(this StringBuilder builder, (byte[], MemberSerializationInfo) valueTuple, int count, string indent, int index, int lengthUlong, string typeName, string suffix)
        {
            builder.Append(indent).Append("if (");
            var span = valueTuple.Item1.AsSpan();
            var tempSpan = span.Slice(index << 3);
            for (; index < lengthUlong; index++)
            {
                var value = BinaryPrimitives.ReadUInt64LittleEndian(tempSpan);
                tempSpan = tempSpan.Slice(8);
                builder.Append("\r\n    ").Append(indent).Append("(" + CalcKey + ") != ").EmbedNumberUlong(value);

                builder.Append(" || ").EmbedComment(value);
            }

            ulong last = span[span.Length - 1];
            for (var i = 1; i < count; i++)
            {
                last <<= 8;
                last |= span[span.Length - 1 - i];
            }

            var memberInfo = valueTuple.Item2;

            builder.Append("\r\n").Append(indent).Append("((").Append(typeName).Append(")array[offset++]");

            for (var i = 0; i < count; i++)
            {
                builder.Append(" | ((").Append(typeName).Append(")array[offset++] << ").Append(i << 3).Append(")");
            }

            builder
                .Append(") != 0x").Append(last.ToString("X"))
                .Append(suffix).Append(") goto FAIL;\r\n    ")
                .Append(indent).EmbedMemberAssignment(memberInfo)
                .Append("\r\n").Append(indent).Append("continue;");
        }

        private static void EmbedSwitch0(this StringBuilder builder, ReadOnlyMemory<(byte[], MemberSerializationInfo)> memory, string indent, int index, int lengthUlong)
        {
            if (memory.Length == 1)
            {
                builder.EmbedSwitch0Single(memory.Span[0], indent, index, lengthUlong);
                return;
            }

            var nextIndent = "    " + indent;
            builder.Append(indent).Append("switch (" + CalcKey + "){\r\n").Append(nextIndent).Append("default: goto FAIL;");

            if (index == lengthUlong - 1)
            {
                builder.EmbedSwitch0Last(memory, index, nextIndent);
            }
            else
            {
                builder.EmbedSwitch0NotLast(memory, index, lengthUlong, nextIndent);
            }

            builder.Append("\r\n").Append(indent).Append("}");
        }

        private static void EmbedSwitch0NotLast(this StringBuilder builder, ReadOnlyMemory<(byte[], MemberSerializationInfo)> memories, int index, int lengthUlong, string nextIndent)
        {
            foreach (var memory in new ULongGroupEnumerable(memories, index))
            {
                var (readOnlyMemory, _) = memory.Span[0];
                var value = BinaryPrimitives.ReadUInt64LittleEndian(readOnlyMemory.AsSpan(index << 3));
                builder.Append("\r\n").Append(nextIndent).EmbedCase(value);

                EmbedSwitch0(builder, memory, nextIndent, index + 1, lengthUlong);
            }
        }

        private static void EmbedSwitch0Last(this StringBuilder builder, ReadOnlyMemory<(byte[], MemberSerializationInfo)> memories, int index, string nextIndent)
        {
            foreach (var memory in new ULongGroupEnumerable(memories, index))
            {
                if (memory.Length != 1)
                {
                    throw new ArgumentException();
                }

                var (readOnlyMemory, memberInfo) = memory.Span[0];
                var last = BinaryPrimitives.ReadUInt64LittleEndian(readOnlyMemory.AsSpan(index << 3));
                builder.Append("\r\n").Append(nextIndent).EmbedCase(last)
                    .Append("    ").EmbedMemberAssignment(memberInfo)
                    .Append("\r\n    ").Append(nextIndent).Append("continue;");
            }
        }

        private static void EmbedSwitch0Single(this StringBuilder builder, (byte[], MemberSerializationInfo) valueTuple, string indent, int index, int lengthUlong)
        {
            builder.Append(indent).Append("if (");
            var span = valueTuple.Item1.AsSpan(index << 3);
            for (; index < lengthUlong; index++)
            {
                var value = BinaryPrimitives.ReadUInt64LittleEndian(span);
                span = span.Slice(8);
                builder.Append("\r\n    ").Append(indent).Append("(" + CalcKey + ") != ").EmbedNumberUlong(value);

                if (index == lengthUlong - 1)
                {
                    builder.Append(' ').EmbedComment(value);
                    break;
                }

                builder.Append(" || ").EmbedComment(value);
            }

            var memberInfo = valueTuple.Item2;

            builder
                .Append("\r\n").Append(indent)
                .Append(") goto FAIL;\r\n\r\n").Append(indent)
                .EmbedMemberAssignment(memberInfo)
                .Append("\r\n").Append(indent).Append("continue;");
        }

        private static StringBuilder EmbedComment(this StringBuilder builder, ulong keyLittleEndian)
        {
            var character0 = keyLittleEndian & 0xffUL;
            if (character0 > 0x7fUL)
            {
                return builder;
            }

            keyLittleEndian >>= 8;
            var character1 = keyLittleEndian & 0xffUL;
            if (character1 > 0x7fUL)
            {
                return builder;
            }

            keyLittleEndian >>= 8;
            var character2 = keyLittleEndian & 0xffUL;
            if (character2 > 0x7fUL)
            {
                return builder;
            }

            keyLittleEndian >>= 8;
            var character3 = keyLittleEndian & 0xffUL;
            if (character3 > 0x7fUL)
            {
                return builder;
            }

            keyLittleEndian >>= 8;
            var character4 = keyLittleEndian & 0xffUL;
            if (character4 > 0x7fUL)
            {
                return builder;
            }

            keyLittleEndian >>= 8;
            var character5 = keyLittleEndian & 0xffUL;
            if (character5 > 0x7fUL)
            {
                return builder;
            }

            keyLittleEndian >>= 8;
            var character6 = keyLittleEndian & 0xffUL;
            if (character6 > 0x7fUL)
            {
                return builder;
            }

            keyLittleEndian >>= 8;
            var character7 = keyLittleEndian & 0xffUL;
            if (character7 > 0x7fUL)
            {
                return builder;
            }

            return builder.Append("// ")
                .Append((char)character0)
                .Append((char)character1)
                .Append((char)character2)
                .Append((char)character3)
                .Append((char)character4)
                .Append((char)character5)
                .Append((char)character6)
                .Append((char)character7);
        }

        private static StringBuilder EmbedNumberUlong(this StringBuilder builder, ulong keyLittleEndian)
        {
            return builder.Append("0x").Append(keyLittleEndian.ToString("X")).Append("UL");
        }

        private static StringBuilder EmbedCase(this StringBuilder builder, ulong keyLittleEndian)
        {
            return builder.Append("case ").EmbedNumberUlong(keyLittleEndian).Append(": ").EmbedComment(keyLittleEndian).Append("\r\n");
        }

        private static StringBuilder EmbedMemberAssignment(this StringBuilder builder, MemberSerializationInfo member)
        {
            return builder.Append("__").Append(member.MemberName).Append("__ = ").Append(member.GetDeserializeMethodString()).Append("; ").Append("__").Append(member.MemberName).Append("__b__ = true;");
        }
    }
}