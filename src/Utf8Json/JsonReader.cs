﻿using System;
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
        static readonly ArraySegment<byte> nullTokenSegment = new ArraySegment<byte>(new byte[] { 110, 117, 108, 108 }, 0, 4);
        static readonly byte[] bom = Encoding.UTF8.GetPreamble();


        readonly byte[] bytes;
        int offset;

        public JsonReader(byte[] bytes)
            : this(bytes, 0)
        {

        }

        public JsonReader(byte[] bytes, int offset)
        {
            this.bytes = bytes;
            this.offset = offset;

            // skip bom
            if (bytes.Length >= 3)
            {
                if (bytes[offset] == bom[0] && bytes[offset + 1] == bom[1] && bytes[offset + 2] == bom[2])
                {
                    this.offset = offset += 3;
                }
            }
        }

        JsonParsingException CreateParsingException(string expected)
        {
            var actual = ((char)bytes[offset]).ToString();
            var pos = offset;

            try
            {
                var token = GetCurrentJsonToken();
                switch (token)
                {
                    case JsonToken.Number:
                        var ns = ReadNumberSegment();
                        actual = StringEncoding.UTF8.GetString(ns.Array, ns.Offset, ns.Count);
                        break;
                    case JsonToken.String:
                        actual = "\"" + ReadString() + "\"";
                        break;
                    case JsonToken.True:
                        actual = "true";
                        break;
                    case JsonToken.False:
                        actual = "false";
                        break;
                    case JsonToken.Null:
                        actual = "null";
                        break;
                    default:
                        break;
                }
            }
            catch { }

            return new JsonParsingException("expected:'" + expected + "', actual:'" + actual + "', at offset:" + pos, bytes, pos, offset, actual);
        }

        JsonParsingException CreateParsingExceptionMessage(string message)
        {
            var actual = ((char)bytes[offset]).ToString();
            var pos = offset;

            return new JsonParsingException(message, bytes, pos, pos, actual);
        }

        bool IsInRange
        {
            get
            {
                return offset < bytes.Length;
            }
        }

        public void AdvanceOffset(int offset)
        {
            this.offset += offset;
        }

        public byte[] GetBufferUnsafe()
        {
            return bytes;
        }

        public int GetCurrentOffsetUnsafe()
        {
            return offset;
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
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                    case 10:
                    case 11:
                    case 12:
                    case 13:
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
                    case 32:
                    case 33:
                    case 35:
                    case 36:
                    case 37:
                    case 38:
                    case 39:
                    case 40:
                    case 41:
                    case 42:
                    case 43:
                    case 46:
                    case 47:
                    case 59:
                    case 60:
                    case 61:
                    case 62:
                    case 63:
                    case 64:
                    case 65:
                    case 66:
                    case 67:
                    case 68:
                    case 69:
                    case 70:
                    case 71:
                    case 72:
                    case 73:
                    case 74:
                    case 75:
                    case 76:
                    case 77:
                    case 78:
                    case 79:
                    case 80:
                    case 81:
                    case 82:
                    case 83:
                    case 84:
                    case 85:
                    case 86:
                    case 87:
                    case 88:
                    case 89:
                    case 90:
                    case 92:
                    case 94:
                    case 95:
                    case 96:
                    case 97:
                    case 98:
                    case 99:
                    case 100:
                    case 101:
                    case 103:
                    case 104:
                    case 105:
                    case 106:
                    case 107:
                    case 108:
                    case 109:
                    case 111:
                    case 112:
                    case 113:
                    case 114:
                    case 115:
                    case 117:
                    case 118:
                    case 119:
                    case 120:
                    case 121:
                    case 122:
                    default:
                        return JsonToken.None;
                }
            }
            else
            {
                return JsonToken.None;
            }
        }

#if NETSTANDARD
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public void SkipWhiteSpace()
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
                    case (byte)'/': // BeginComment
                        i = ReadComment(bytes, i);
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
                    case 33:
                    case 34:
                    case 35:
                    case 36:
                    case 37:
                    case 38:
                    case 39:
                    case 40:
                    case 41:
                    case 42:
                    case 43:
                    case 44:
                    case 45:
                    case 46:
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
            if (!ReadIsBeginArray()) throw CreateParsingException("[");
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

        public void ReadIsEndArrayWithVerify()
        {
            if (!ReadIsEndArray()) throw CreateParsingException("]");
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

        /// <summary>
        /// Convinient pattern of ReadIsBeginArrayWithVerify + while(!ReadIsEndArrayWithSkipValueSeparator)
        /// </summary>
        public bool ReadIsInArray(ref int count)
        {
            if (count == 0)
            {
                ReadIsBeginArrayWithVerify();
                if (ReadIsEndArray())
                {
                    return false;
                }
            }
            else
            {
                if (ReadIsEndArray())
                {
                    return false;
                }
                else
                {
                    ReadIsValueSeparatorWithVerify();
                }
            }

            count++;
            return true;
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
        public void ReadIsEndObjectWithVerify()
        {
            if (!ReadIsEndObject()) throw CreateParsingException("}");
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

        /// <summary>
        /// Convinient pattern of ReadIsBeginObjectWithVerify + while(!ReadIsEndObjectWithSkipValueSeparator)
        /// </summary>
        public bool ReadIsInObject(ref int count)
        {
            if (count == 0)
            {
                ReadIsBeginObjectWithVerify();
                if (ReadIsEndObject())
                {
                    return false;
                }
            }
            else
            {
                if (ReadIsEndObject())
                {
                    return false;
                }
                else
                {
                    ReadIsValueSeparatorWithVerify();
                }
            }

            count++;
            return true;
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
            if (!ReadIsNameSeparator()) throw CreateParsingException(":");
        }

        void ReadStringSegmentCore(out byte[] resultBytes, out int resultOffset, out int resultLength)
        {
            // SkipWhiteSpace is already called from IsNull

            byte[] builder = null;
            var builderOffset = 0;
            char[] codePointStringBuffer = null;
            var codePointStringOffet = 0;

            if (bytes[offset] != '\"') throw CreateParsingException("String Begin Token");
            offset++;

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
                                escapeCharacter = bytes[i + 1];
                                goto COPY;
                            case 'b':
                                escapeCharacter = (byte)'\b';
                                goto COPY;
                            case 'f':
                                escapeCharacter = (byte)'\f';
                                goto COPY;
                            case 'n':
                                escapeCharacter = (byte)'\n';
                                goto COPY;
                            case 'r':
                                escapeCharacter = (byte)'\r';
                                goto COPY;
                            case 't':
                                escapeCharacter = (byte)'\t';
                                goto COPY;
                            case 'u':
                                if (codePointStringBuffer == null) codePointStringBuffer = StringBuilderCache.GetCodePointStringBuffer();

                                if (codePointStringOffet == 0)
                                {
                                    if (builder == null) builder = StringBuilderCache.GetBuffer();

                                    var copyCount = i - from;
                                    BinaryUtil.EnsureCapacity(ref builder, builderOffset, copyCount + 1); // require + 1
                                    Buffer.BlockCopy(bytes, from, builder, builderOffset, copyCount);
                                    builderOffset += copyCount;
                                }

                                if (codePointStringBuffer.Length == codePointStringOffet)
                                {
                                    Array.Resize(ref codePointStringBuffer, codePointStringBuffer.Length * 2);
                                }

                                var a = (char)bytes[i + 2];
                                var b = (char)bytes[i + 3];
                                var c = (char)bytes[i + 4];
                                var d = (char)bytes[i + 5];
                                var codepoint = GetCodePoint(a, b, c, d);
                                codePointStringBuffer[codePointStringOffet++] = (char)codepoint;
                                i += 5;
                                offset += 6;
                                from = offset;
                                continue;
                            default:
                                throw CreateParsingExceptionMessage("Bad JSON escape.");
                        }
                    case (byte)'"': // endtoken
                        offset++;
                        goto END;
                    default: // string
                        if (codePointStringOffet != 0)
                        {
                            if (builder == null) builder = StringBuilderCache.GetBuffer();
                            BinaryUtil.EnsureCapacity(ref builder, builderOffset, StringEncoding.UTF8.GetMaxByteCount(codePointStringOffet));
                            builderOffset += StringEncoding.UTF8.GetBytes(codePointStringBuffer, 0, codePointStringOffet, builder, builderOffset);
                            codePointStringOffet = 0;
                        }
                        offset++;
                        continue;
                }

                COPY:
                {
                    if (builder == null) builder = StringBuilderCache.GetBuffer();
                    if (codePointStringOffet != 0)
                    {
                        BinaryUtil.EnsureCapacity(ref builder, builderOffset, StringEncoding.UTF8.GetMaxByteCount(codePointStringOffet));
                        builderOffset += StringEncoding.UTF8.GetBytes(codePointStringBuffer, 0, codePointStringOffet, builder, builderOffset);
                        codePointStringOffet = 0;
                    }

                    var copyCount = i - from;
                    BinaryUtil.EnsureCapacity(ref builder, builderOffset, copyCount + 1); // require + 1!
                    Buffer.BlockCopy(bytes, from, builder, builderOffset, copyCount);
                    builderOffset += copyCount;
                    builder[builderOffset++] = escapeCharacter;
                    i += 1;
                    offset += 2;
                    from = offset;
                }
            }

            resultLength = 0;
            resultBytes = null;
            resultOffset = 0;
            throw CreateParsingException("String End Token");

            END:
            if (builderOffset == 0 && codePointStringOffet == 0) // no escape
            {
                resultBytes = bytes;
                resultOffset = from;
                resultLength = offset - 1 - from; // skip last quote
            }
            else
            {
                if (builder == null) builder = StringBuilderCache.GetBuffer();
                if (codePointStringOffet != 0)
                {
                    BinaryUtil.EnsureCapacity(ref builder, builderOffset, StringEncoding.UTF8.GetMaxByteCount(codePointStringOffet));
                    builderOffset += StringEncoding.UTF8.GetBytes(codePointStringBuffer, 0, codePointStringOffet, builder, builderOffset);
                    codePointStringOffet = 0;
                }

                var copyCount = offset - from - 1;
                BinaryUtil.EnsureCapacity(ref builder, builderOffset, copyCount);
                Buffer.BlockCopy(bytes, from, builder, builderOffset, copyCount);
                builderOffset += copyCount;

                resultBytes = builder;
                resultOffset = 0;
                resultLength = builderOffset;
            }
        }

#if NETSTANDARD
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        static int GetCodePoint(char a, char b, char c, char d)
        {
            return (((((ToNumber(a) * 16) + ToNumber(b)) * 16) + ToNumber(c)) * 16) + ToNumber(d);
        }

#if NETSTANDARD
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        static int ToNumber(char x)
        {
            if ('0' <= x && x <= '9')
            {
                return x - '0';
            }
            else if ('a' <= x && x <= 'f')
            {
                return x - 'a' + 10;
            }
            else if ('A' <= x && x <= 'F')
            {
                return x - 'A' + 10;
            }
            throw new JsonParsingException("Invalid Character" + x);
        }

        public ArraySegment<byte> ReadStringSegmentUnsafe()
        {
            if (ReadIsNull()) return nullTokenSegment;

            byte[] bytes;
            int offset;
            int length;
            ReadStringSegmentCore(out bytes, out offset, out length);
            return new ArraySegment<byte>(bytes, offset, length);
        }

        public string ReadString()
        {
            if (ReadIsNull()) return null;

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

        /// <summary>Get raw string-span(do not unescape)</summary>
        public ArraySegment<byte> ReadStringSegmentRaw()
        {
            ArraySegment<byte> key = default(ArraySegment<byte>);
            if (ReadIsNull())
            {
                key = nullTokenSegment;
            }
            else
            {
                // SkipWhiteSpace is already called from IsNull
                if (bytes[offset++] != '\"') throw CreateParsingException("\"");

                var from = offset;

                for (int i = offset; i < bytes.Length; i++)
                {
                    if (bytes[i] == (char)'\"')
                    {
                        // is escape?
                        if (bytes[i - 1] == (char)'\\')
                        {
                            continue;
                        }
                        else
                        {
                            offset = i + 1;
                            goto OK;
                        }
                    }
                }
                throw CreateParsingExceptionMessage("not found end string.");

                OK:
                key = new ArraySegment<byte>(bytes, from, offset - from - 1); // remove \"
            }

            return key;
        }

        /// <summary>Get raw string-span(do not unescape) + ReadIsNameSeparatorWithVerify</summary>
        public ArraySegment<byte> ReadPropertyNameSegmentRaw()
        {
            var key = ReadStringSegmentRaw();
            ReadIsNameSeparatorWithVerify();
            return key;
        }

        public bool ReadBoolean()
        {
            SkipWhiteSpace();
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
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
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
                case 33:
                case 35:
                case 36:
                case 37:
                case 38:
                case 39:
                case 40:
                case 41:
                case 42:
                case 43:
                case 45:
                case 46:
                case 47:
                case 48:
                case 49:
                case 50:
                case 51:
                case 52:
                case 53:
                case 54:
                case 55:
                case 56:
                case 57:
                case 59:
                case 60:
                case 61:
                case 62:
                case 63:
                case 64:
                case 65:
                case 66:
                case 67:
                case 68:
                case 69:
                case 70:
                case 71:
                case 72:
                case 73:
                case 74:
                case 75:
                case 76:
                case 77:
                case 78:
                case 79:
                case 80:
                case 81:
                case 82:
                case 83:
                case 84:
                case 85:
                case 86:
                case 87:
                case 88:
                case 89:
                case 90:
                case 92:
                case 94:
                case 95:
                case 96:
                case 97:
                case 98:
                case 99:
                case 100:
                case 101:
                case 102:
                case 103:
                case 104:
                case 105:
                case 106:
                case 107:
                case 108:
                case 109:
                case 110:
                case 111:
                case 112:
                case 113:
                case 114:
                case 115:
                case 116:
                case 117:
                case 118:
                case 119:
                case 120:
                case 121:
                case 122:
                default:
                    return false;
            }
        }

        public void ReadNext()
        {
            var token = GetCurrentJsonToken();
            ReadNextCore(token);
        }

#if NETSTANDARD
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        void ReadNextCore(JsonToken token)
        {
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
                    offset += 1; // position is "\"";
                    for (int i = offset; i < bytes.Length; i++)
                    {
                        if (bytes[i] == (char)'\"')
                        {
                            // is escape?
                            if (bytes[i - 1] == (char)'\\')
                            {
                                continue;
                            }
                            else
                            {
                                offset = i + 1;
                                return; // end
                            }
                        }
                    }
                    throw CreateParsingExceptionMessage("not found end string.");
                case JsonToken.Number:
                    for (int i = offset; i < bytes.Length; i++)
                    {
                        if (IsWordBreak(bytes[i]))
                        {
                            offset = i;
                            return;
                        }
                    }
                    offset = bytes.Length;
                    break;
                case JsonToken.None:
                default:
                    break;
            }
        }

        public void ReadNextBlock()
        {
            var stack = 0;

            AGAIN:
            var token = GetCurrentJsonToken();
            switch (token)
            {
                case JsonToken.BeginObject:
                case JsonToken.BeginArray:
                    offset++;
                    stack++;
                    goto AGAIN;
                case JsonToken.EndObject:
                case JsonToken.EndArray:
                    offset++;
                    stack--;
                    if (stack != 0)
                    {
                        goto AGAIN;
                    }
                    break;
                case JsonToken.True:
                case JsonToken.False:
                case JsonToken.Null:
                case JsonToken.String:
                case JsonToken.Number:
                case JsonToken.NameSeparator:
                case JsonToken.ValueSeparator:
                    do
                    {
                        ReadNextCore(token);
                        token = GetCurrentJsonToken();
                    } while (stack != 0 && !((int)token < 5)); // !(None, Begin/EndObject, Begin/EndArray)

                    if (stack != 0)
                    {
                        goto AGAIN;
                    }
                    break;
                case JsonToken.None:
                default:
                    break;
            }
        }

        public ArraySegment<byte> ReadNextBlockSegment()
        {
            var startOffset = offset;
            ReadNextBlock();
            return new ArraySegment<byte>(bytes, startOffset, offset - startOffset);
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
            if (readCount == 0)
            {
                throw CreateParsingException("Number Token");
            }

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
            if (readCount == 0)
            {
                throw CreateParsingException("Number Token");
            }
            offset += readCount;
            return v;
        }

        public Single ReadSingle()
        {
            SkipWhiteSpace();
            int readCount;
            var v = Utf8Json.Internal.DoubleConversion.StringToDoubleConverter.ToSingle(bytes, offset, out readCount);
            if (readCount == 0)
            {
                throw CreateParsingException("Number Token");
            }
            offset += readCount;
            return v;
        }

        public Double ReadDouble()
        {
            SkipWhiteSpace();
            int readCount;
            var v = Utf8Json.Internal.DoubleConversion.StringToDoubleConverter.ToDouble(bytes, offset, out readCount);
            if (readCount == 0)
            {
                throw CreateParsingException("Number Token");
            }
            offset += readCount;
            return v;
        }

        public ArraySegment<byte> ReadNumberSegment()
        {
            SkipWhiteSpace();
            var initialOffset = offset;
            for (int i = offset; i < bytes.Length; i++)
            {
                if (!NumberConverter.IsNumberRepresentation(bytes[i]))
                {
                    offset = i;
                    goto END;
                }
            }
            offset = bytes.Length;

            END:
            return new ArraySegment<byte>(bytes, initialOffset, offset - initialOffset);
        }

        // return last offset.
        static int ReadComment(byte[] bytes, int offset)
        {
            // current token is '/'
            if (bytes[offset + 1] == '/')
            {
                // single line
                offset += 2;
                for (int i = offset; i < bytes.Length; i++)
                {
                    if (bytes[i] == '\r' || bytes[i] == '\n' || bytes[i] == '\0')
                    {
                        return i;
                    }
                }

                throw new JsonParsingException("Can not find end token of single line comment(\r or \n).");
            }
            else if (bytes[offset + 1] == '*')
            {

                offset += 2; // '/' + '*';
                for (int i = offset; i < bytes.Length; i++)
                {
                    if (bytes[i] == '*' && bytes[i + 1] == '/')
                    {
                        return i + 1;
                    }
                }
                throw new JsonParsingException("Can not find end token of multi line comment(*/).");
            }

            return offset;
        }

        internal static class StringBuilderCache
        {
            [ThreadStatic]
            static byte[] buffer;

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
        WeakReference underyingBytes;
        int limit;
        public int Offset { get; private set; }
        public string ActualChar { get; set; }

        public JsonParsingException(string message)
            : base(message)
        {

        }

        public JsonParsingException(string message, byte[] underlyingBytes, int offset, int limit, string actualChar)
            : base(message)
        {
            this.underyingBytes = new WeakReference(underlyingBytes);
            this.Offset = offset;
            this.ActualChar = actualChar;
            this.limit = limit;
        }

        /// <summary>
        /// Underlying bytes is may be a pooling buffer, be careful to use it. If lost reference or can not handled byte[], return null.
        /// </summary>
        public byte[] GetUnderlyingByteArrayUnsafe()
        {
            return underyingBytes.Target as byte[];
        }

        /// <summary>
        /// Underlying bytes is may be a pooling buffer, be careful to use it. If lost reference or can not handled byte[], return null.
        /// </summary>
        public string GetUnderlyingStringUnsafe()
        {
            var bytes = underyingBytes.Target as byte[];
            if (bytes != null)
            {
                return StringEncoding.UTF8.GetString(bytes, 0, limit) + "...";
            }
            return null;
        }
    }
}
