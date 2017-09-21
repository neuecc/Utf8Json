using System;
using System.Collections.Generic;
using System.Text;
using Utf8Json.Internal;

#if NETSTANDARD
using System.Runtime.CompilerServices;
#endif

namespace Utf8Json
{
    // JSON RFC: https://www.ietf.org/rfc/rfc4627.txt

    public struct JsonReader
    {
        readonly byte[] bytes;
        int offset;

        public JsonReader(byte[] bytes, int offset)
        {
            this.bytes = bytes;
            this.offset = offset;
        }

        JsonParsingException CreateParsingException(string expected)
        {
            return new JsonParsingException("expected:" + expected + ", actual:" + (char)bytes[offset] + " at:" + offset);
        }

        bool IsInRange
        {
            get
            {
                return offset < bytes.Length;
            }
        }

        // double or long
        public bool IsInteger()
        {
            throw new NotImplementedException();
        }

        public JsonToken GetCurrentJsonToken()
        {
            SkipWhiteSpace();
            if (offset < bytes.Length)
            {
                var c = bytes[offset];
                switch (c)
                {
                    case (byte)'{': return JsonToken.BeginObject;
                    case (byte)'}': return JsonToken.EndObject;
                    case (byte)'[': return JsonToken.BeginArray;
                    case (byte)']': return JsonToken.EndArray;
                    case (byte)'t': return JsonToken.True;
                    case (byte)'f': return JsonToken.False;
                    case (byte)'n': return JsonToken.Null;
                    case (byte)',': return JsonToken.ValueSeparator;
                    case (byte)':': return JsonToken.NameSeparator;
                    case (byte)'-': return JsonToken.Number;
                    case (byte)'0': return JsonToken.Number;
                    case (byte)'1': return JsonToken.Number;
                    case (byte)'2': return JsonToken.Number;
                    case (byte)'3': return JsonToken.Number;
                    case (byte)'4': return JsonToken.Number;
                    case (byte)'5': return JsonToken.Number;
                    case (byte)'6': return JsonToken.Number;
                    case (byte)'7': return JsonToken.Number;
                    case (byte)'8': return JsonToken.Number;
                    case (byte)'9': return JsonToken.Number;
                    case (byte)'\"': return JsonToken.String;
                    default:
                        return JsonToken.None;
                }
            }
            else
            {
                return JsonToken.None;
            }
        }

        void SkipWhiteSpace()
        {
            // eliminate array bound check
            for (int i = offset; i < bytes.Length; i++)
            {
                switch (bytes[i])
                {
                    case 0x20: // Space
                    case 0x09: // Horizontal tab
                    case 0x0A: // Line feed or New line
                    case 0x0D: // Carriage return
                        continue;
                    // optimize skip jumptable
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 11:
                    case 12:
                    case 14:
                    case 15:
                    case 16:
                    case 17:
                    case 18:
                    case 19:
                    case 20:
                    case 21:
                    case 22:
                    case 23:
                    case 24:
                    case 25:
                    case 26:
                    case 27:
                    case 28:
                    case 29:
                    case 30:
                    case 31:
                    default:
                        offset = i;
                        return; // end
                }
            }

            offset = bytes.Length;
        }

        public bool ReadIsNull()
        {
            SkipWhiteSpace();
            if (IsInRange && bytes[offset] == 'n')
            {
                if (bytes[offset + 1] != 'u') goto ERROR;
                if (bytes[offset + 2] != 'l') goto ERROR;
                if (bytes[offset + 3] != 'l') goto ERROR;
                offset += 4;
                return true;
            }
            else
            {
                return false;
            }

            ERROR:
            throw CreateParsingException("null");
        }

        public bool ReadIsBeginArray()
        {
            SkipWhiteSpace();
            if (IsInRange && bytes[offset] == '[')
            {
                offset += 1;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ReadIsBeginArrayWithVerify()
        {
            if (!ReadIsBeginArray()) throw CreateParsingException("{");
        }

        public bool ReadIsEndArray()
        {
            SkipWhiteSpace();
            if (IsInRange && bytes[offset] == ']')
            {
                offset += 1;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool ReadIsEndArrayWithSkipValueSeparator(ref int count)
        {
            SkipWhiteSpace();
            if (IsInRange && bytes[offset] == ']')
            {
                offset += 1;
                return true;
            }
            else
            {
                if (count++ != 0)
                {
                    ReadIsValueSeparatorWithVerify();
                }
                return false;
            }
        }

        public bool ReadIsBeginObject()
        {
            SkipWhiteSpace();
            if (IsInRange && bytes[offset] == '{')
            {
                offset += 1;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ReadIsBeginObjectWithVerify()
        {
            if (!ReadIsBeginObject()) throw CreateParsingException("{");
        }

        public bool ReadIsEndObject()
        {
            SkipWhiteSpace();
            if (IsInRange && bytes[offset] == '}')
            {
                offset += 1;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool ReadIsEndObjectWithSkipValueSeparator(ref int count)
        {
            SkipWhiteSpace();
            if (IsInRange && bytes[offset] == '}')
            {
                offset += 1;
                return true;
            }
            else
            {
                if (count++ != 0)
                {
                    ReadIsValueSeparatorWithVerify();
                }
                return false;
            }
        }

        public bool ReadIsValueSeparator()
        {
            SkipWhiteSpace();
            if (IsInRange && bytes[offset] == ',')
            {
                offset += 1;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ReadIsValueSeparatorWithVerify()
        {
            if (!ReadIsValueSeparator()) throw CreateParsingException(",");
        }

        public bool ReadIsNameSeparator()
        {
            SkipWhiteSpace();
            if (IsInRange && bytes[offset] == ':')
            {
                offset += 1;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ReadIsNameSeparatorWithVerify()
        {
            if (!ReadIsNameSeparator()) throw CreateParsingException(".");
        }

        void ReadStringSegmentCore(out byte[] resultBytes, out int resultOffset, out int resultLength)
        {
            byte[] builder = null; // StringBuilderCache.GetBuffer();
            var builderOffset = 0;
            char[] codePointBuffer = null; // StringBuilderCache.GetForCodePoint();
            char[] codePointStringBuffer = null; // StringBuilderCache.GetCodePointStringBuffer();
            var codePointStringOffet = 0;

            if (bytes[offset++] != '\"') throw CreateParsingException("\"");

            var from = offset;

            // eliminate array-bound check
            for (int i = offset; i < bytes.Length; i++)
            {
                byte escapeCharacter = 0;
                switch (bytes[i])
                {
                    case (byte)'\\': // escape character
                        switch ((char)bytes[i + 1])
                        {
                            case '"':
                            case '\\':
                            case '/':
                                escapeCharacter = bytes[i];
                                offset += 2;
                                goto COPY;
                            case 'b':
                                escapeCharacter = (byte)'\b';
                                offset += 2;
                                goto COPY;
                            case 'f':
                                escapeCharacter = (byte)'\f';
                                offset += 2;
                                goto COPY;
                            case 'n':
                                escapeCharacter = (byte)'\n';
                                offset += 2;
                                goto COPY;
                            case 'r':
                                escapeCharacter = (byte)'\r';
                                offset += 2;
                                goto COPY;
                            case 't':
                                escapeCharacter = (byte)'\t';
                                offset += 2;
                                goto COPY;
                            case 'u':
                                if (codePointBuffer == null) codePointBuffer = StringBuilderCache.GetForCodePoint();
                                if (codePointStringBuffer == null) codePointStringBuffer = StringBuilderCache.GetCodePointStringBuffer();

                                i++; // \\
                                i++; // \u
                                codePointBuffer[0] = (char)bytes[i++];
                                codePointBuffer[1] = (char)bytes[i++];
                                codePointBuffer[2] = (char)bytes[i++];
                                codePointBuffer[3] = (char)bytes[i];
                                offset += 5;

                                var codepoint = (char)Convert.ToInt32(new string(codePointBuffer), 16);
                                codePointStringBuffer[codePointStringOffet++] = codepoint;
                                break;
                            default:
                                throw new JsonParsingException("Bad JSON escape.");
                        }
                        break;
                    case (byte)'"': // endtoken
                        offset++;
                        goto END;
                    default: // string
                        offset++;
                        continue;
                }

                // TODO:copy codepoint
                if (codePointStringOffet != 0) { }

                COPY:
                if (builder == null) builder = StringBuilderCache.GetBuffer();

                var copyCount = i - from;
                Buffer.BlockCopy(bytes, from, builder, builderOffset, copyCount);
                builderOffset += copyCount;
                builder[builderOffset++] = escapeCharacter;
                from = i + 2;
            }

            throw CreateParsingException("\"");

            END:
            if (builderOffset == 0) // no escape
            {
                resultBytes = bytes;
                resultOffset = from;
                resultLength = offset - 1 - from; // skip last quote
            }
            else
            {
                // copy last
                var copyCount = offset - from - 2;
                Buffer.BlockCopy(bytes, from, builder, builderOffset, copyCount);
                builderOffset += copyCount;

                resultBytes = builder;
                resultOffset = 0;
                resultLength = builderOffset;
            }
        }

        public ArraySegment<byte> ReadStringSegmentUnsafe()
        {
            byte[] bytes;
            int offset;
            int length;
            ReadStringSegmentCore(out bytes, out offset, out length);
            return new ArraySegment<byte>(bytes, offset, length);
        }

        public string ReadString()
        {
            byte[] bytes;
            int offset;
            int length;
            ReadStringSegmentCore(out bytes, out offset, out length);

            return Encoding.UTF8.GetString(bytes, offset, length);
        }

        /// <summary>ReadString + ReadIsNameSeparatorWithVerify</summary>
        public string ReadPropertyName()
        {
            var key = ReadString();
            ReadIsNameSeparatorWithVerify();
            return key;
        }

        /// <summary>ReadStringSegmentUnsafe + ReadIsNameSeparatorWithVerify</summary>
        public ArraySegment<byte> ReadPropertyNameSegmentUnsafe()
        {
            var key = ReadStringSegmentUnsafe();
            ReadIsNameSeparatorWithVerify();
            return key;
        }

        public bool ReadBoolean()
        {
            if (bytes[offset] == 't')
            {
                if (bytes[offset + 1] != 'r') goto ERROR_TRUE;
                if (bytes[offset + 2] != 'u') goto ERROR_TRUE;
                if (bytes[offset + 3] != 'e') goto ERROR_TRUE;
                offset += 4;
                return true;
            }
            else if (bytes[offset] == 'f')
            {
                if (bytes[offset + 1] != 'a') goto ERROR_FALSE;
                if (bytes[offset + 2] != 'l') goto ERROR_FALSE;
                if (bytes[offset + 3] != 's') goto ERROR_FALSE;
                if (bytes[offset + 4] != 'e') goto ERROR_FALSE;
                offset += 5;
                return false;
            }
            else
            {
                throw CreateParsingException("true | false");
            }

            ERROR_TRUE:
            throw CreateParsingException("true");
            ERROR_FALSE:
            throw CreateParsingException("false");
        }

        // TODO:Optimize
        static bool IsWordBreak(byte c)
        {
            switch (c)
            {
                case (byte)' ':
                case (byte)'{':
                case (byte)'}':
                case (byte)'[':
                case (byte)']':
                case (byte)',':
                case (byte)':':
                case (byte)'\"':
                    return true;
                default:
                    return false;
            }
        }

        static bool IsNumber(byte c)
        {
            return (byte)'0' <= c && c <= (byte)'9';
        }

        public void ReadNext()
        {
            var token = GetCurrentJsonToken();
            switch (token)
            {
                case JsonToken.BeginObject:
                case JsonToken.BeginArray:
                case JsonToken.ValueSeparator:
                case JsonToken.NameSeparator:
                case JsonToken.EndObject:
                case JsonToken.EndArray:
                    offset += 1;
                    break;
                case JsonToken.True:
                case JsonToken.Null:
                    offset += 4;
                    break;
                case JsonToken.False:
                    offset += 5;
                    break;
                case JsonToken.String:
                    // TODO:Improve?

                    offset += 1; // position is "\"";
                    while (IsInRange)
                    {
                        if (bytes[offset] == (char)'\"')
                        {
                            // is escape?
                            if (bytes[offset - 1] == (char)'\\')
                            {
                                // but not escaped slush?
                                if (bytes[offset - 2] == (char)'\\')
                                {
                                    offset++;
                                    break;
                                }
                                else
                                {
                                    offset++;
                                    continue;
                                }
                            }
                            else
                            {
                                offset++;
                                break;
                            }
                        }
                        else
                        {
                            offset++;
                        }
                    }
                    break;
                case JsonToken.Number:
                    while (IsInRange && !IsWordBreak(bytes[offset++]))
                    {
                        // read number(float, signed, unsigned).
                    }
                    break;
                case JsonToken.None:
                default:
                    break;
            }
        }

        public void ReadNextBlock()
        {
            ReadNextBlockCore(0);
        }

        void ReadNextBlockCore(int stack)
        {
            var token = GetCurrentJsonToken();
            switch (token)
            {
                case JsonToken.BeginObject:
                case JsonToken.BeginArray:
                    offset++;
                    ReadNextBlockCore(stack + 1);
                    break;
                case JsonToken.EndObject:
                case JsonToken.EndArray:
                    offset++;
                    ReadNextBlockCore(stack - 1);
                    break;
                case JsonToken.True:
                case JsonToken.False:
                case JsonToken.Null:
                case JsonToken.String:
                case JsonToken.Number:
                case JsonToken.ValueSeparator:
                case JsonToken.NameSeparator:
                    ReadNext();
                    if (stack != 0)
                    {
                        ReadNextBlockCore(stack);
                    }
                    break;
                case JsonToken.None:
                default:
                    break;
            }
        }

        public sbyte ReadSByte()
        {
            return checked((sbyte)ReadInt64());
        }

        public short ReadInt16()
        {
            return checked((short)ReadInt64());
        }

        public int ReadInt32()
        {
            return checked((int)ReadInt64());
        }

        public long ReadInt64()
        {
            SkipWhiteSpace();

            int readCount;
            var v = NumberConverter.ReadInt64(bytes, offset, out readCount);
            offset += readCount;
            return v;
        }

        public byte ReadByte()
        {
            return checked((byte)ReadUInt64());
        }

        public ushort ReadUInt16()
        {
            return checked((ushort)ReadUInt64());
        }

        public uint ReadUInt32()
        {
            return checked((uint)ReadUInt64());
        }

        public ulong ReadUInt64()
        {
            SkipWhiteSpace();

            int readCount;
            var v = NumberConverter.ReadUInt64(bytes, offset, out readCount);
            offset += readCount;
            return v;
        }

        public Single ReadSingle()
        {
            int readCount;
            var v = Utf8Json.Internal.DoubleConversion.StringToDoubleConverter.ToSingle(bytes, offset, out readCount);
            offset += readCount;
            return v;
        }

        public Double ReadDouble()
        {
            int readCount;
            var v = Utf8Json.Internal.DoubleConversion.StringToDoubleConverter.ToDouble(bytes, offset, out readCount);
            offset += readCount;
            return v;
        }

        internal static class StringBuilderCache
        {
            [ThreadStatic]
            static byte[] buffer;

            [ThreadStatic]
            static char[] codePoint;

            [ThreadStatic]
            static char[] codePointStringBuffer;

            public static byte[] GetBuffer()
            {
                if (buffer == null)
                {
                    buffer = new byte[65535];
                }
                return buffer;
            }

            public static char[] GetForCodePoint()
            {
                if (codePoint == null)
                {
                    codePoint = new char[4];
                }
                return codePoint;
            }

            public static char[] GetCodePointStringBuffer()
            {
                if (codePointStringBuffer == null)
                {
                    codePointStringBuffer = new char[65535];
                }
                return codePointStringBuffer;
            }
        }
    }

    public class JsonParsingException : Exception
    {
        public JsonParsingException(string message)
            : base(message)
        {

        }
    }
}
