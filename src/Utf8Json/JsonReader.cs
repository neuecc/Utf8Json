using System;
using System.Collections.Generic;
using System.Text;

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

        JsonParsingException CreateException(string expected)
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
                        return;
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
            throw CreateException("null");
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
            if (!ReadIsBeginArray()) throw CreateException("{");
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
            if (!ReadIsBeginObject()) throw CreateException("{");
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
            if (!ReadIsValueSeparator()) throw CreateException(",");
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
            if (!ReadIsNameSeparator()) throw CreateException(".");
        }

        void ReadStringSegmentCore(out byte[] resultBytes, out int resultOffset)
        {
            var builder = StringBuilderCache.GetBuffer();
            var builderOffset = 0;

            if (bytes[offset++] != '\"') throw CreateException("\"");

            // eliminate array-bound check
            for (int i = offset; i < bytes.Length; i++)
            {
                switch (bytes[i])
                {
                    case (byte)'"': // endtoken
                        offset = i + 1;
                        goto END;
                    case (byte)'\\': // escape character
                                     // TODO
                                     //c = (char)bytes[offset++];
                                     //switch (c)
                                     //{
                                     //    case '"':
                                     //    case '\\':
                                     //    case '/':
                                     //        // p[offset] = c;
                                     //        break;
                                     //    case 'b':
                                     //        //sb.Append('\b');
                                     //        break;
                                     //    case 'f':
                                     //        //sb.Append('\f');
                                     //        break;
                                     //    case 'n':
                                     //        //sb.Append('\n');
                                     //        break;
                                     //    case 'r':
                                     //        //sb.Append('\r');
                                     //        break;
                                     //    case 't':
                                     //        //sb.Append('\t');
                                     //        break;
                                     //    case 'u':
                                     //        //var hex = new char[4];
                                     //        //hex[0] = ReadChar();
                                     //        //hex[1] = ReadChar();
                                     //        //hex[2] = ReadChar();
                                     //        //hex[3] = ReadChar();
                                     //        //sb.Append((char)Convert.ToInt32(new string(hex), 16));
                                     //        break;
                                     //}
                        break;
                    default: // string
                             // TODO:EnsureCapacity
                        builder[builderOffset++] = bytes[i];
                        break;
                }
            }

            throw CreateException("\"");

            END:
            resultBytes = builder;
            resultOffset = builderOffset;
        }

        public ArraySegment<byte> ReadStringSegmentUnsafe()
        {
            byte[] bytes;
            int offset;
            ReadStringSegmentCore(out bytes, out offset);
            return new ArraySegment<byte>(bytes, 0, offset);
        }

        public string ReadString()
        {
            byte[] bytes;
            int offset;
            ReadStringSegmentCore(out bytes, out offset);

            return Encoding.UTF8.GetString(bytes, 0, offset);
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
                throw CreateException("true | false");
            }

            ERROR_TRUE:
            throw CreateException("true");
            ERROR_FALSE:
            throw CreateException("false");
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

        // TODO:Optimize
        static bool IsNumber(byte c)
        {
            switch (c)
            {
                case (byte)'0':
                case (byte)'1':
                case (byte)'2':
                case (byte)'3':
                case (byte)'4':
                case (byte)'5':
                case (byte)'6':
                case (byte)'7':
                case (byte)'8':
                case (byte)'9':
                    return true;
                default:
                    return false;
            }
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

            var value = 0L;
            var sign = 1;

            if (bytes[offset] == '-')
            {
                sign = -1;
                offset++;
            }

            for (int i = offset; i < bytes.Length; i++)
            {
                if (!IsNumber(bytes[i]))
                {
                    offset = i;
                    goto END;
                }

                // long.MinValue causes overflow so use unchecked.
                value = unchecked(value * 10 + (bytes[i] - '0'));
            }
            offset = bytes.Length;

            END:
            return unchecked(value * sign);
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

            var value = 0UL;

            for (int i = offset; i < bytes.Length; i++)
            {
                if (!IsNumber(bytes[i]))
                {
                    offset = i;
                    goto END;
                }

                value = checked(value * 10 + (ulong)(bytes[i] - '0'));
            }
            offset = bytes.Length;

            END:
            return value;
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

            public static byte[] GetBuffer()
            {
                if (buffer == null)
                {
                    buffer = new byte[65535];
                }
                return buffer;

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
