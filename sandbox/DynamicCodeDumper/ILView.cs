﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DynamicCodeDumper
{
    public class ILStreamReader : BinaryReader
    {
        static readonly Dictionary<short, OpCode> opCodes = new Dictionary<short, OpCode>();

        int endPosition;

        public int CurrentPosition { get { return (int)BaseStream.Position; } }

        public bool EndOfStream { get { return !((int)BaseStream.Position < endPosition); } }

        static ILStreamReader()
        {
            foreach (var fieldInfo in typeof(OpCodes).GetFields())
            {
                var opCode = (OpCode)fieldInfo.GetValue(null);
                opCodes.Add(opCode.Value, opCode);
            }
        }

        public ILStreamReader(byte[] ilByteArray)
            : base(new MemoryStream(ilByteArray))
        {
            this.endPosition = ilByteArray.Length;
        }

        public OpCode ReadOpCode()
        {
            short value = ReadByte();
            if (value == 0xfe)
            {
                value = (short)((value << 8) | ReadByte());
            }
            return opCodes[value];
        }

        public int ReadMetadataToken()
        {
            return ReadInt32();
        }
    }

    public static class ILView
    {
        public static string ToPrettyPrintInstruction(MethodBase method)
        {
            var sb = new StringBuilder();

            foreach (var item in ToOpCodes(method))
            {
                sb.AppendLine(item.ToString());
            }

            return sb.ToString();
        }

        public static IEnumerable<Instruction> ToOpCodes(MethodBase method)
        {
            var body = method.GetMethodBody();
            if (body == null) yield break;

            var il = body.GetILAsByteArray();

            using (var reader = new ILStreamReader(il))
            {
                while (!reader.EndOfStream)
                {

                    object data = null;
                    var position = reader.CurrentPosition;

                    var opCode = reader.ReadOpCode();
                    switch (opCode.OperandType)
                    {
                        case OperandType.ShortInlineI:
                            data = reader.ReadByte();
                            break;
                        case OperandType.ShortInlineBrTarget:
                        case OperandType.ShortInlineVar:
                            // data =
                            reader.ReadByte();
                            break;
                        case OperandType.InlineVar:
                            reader.ReadUInt16();
                            break;
                        case OperandType.InlineSig:
                        case OperandType.InlineType:
                            reader.ReadUInt32();
                            break;
                        case OperandType.InlineNone:
                            break;
                        case OperandType.InlineBrTarget:
                            // data =
                            reader.ReadUInt32();
                            break;
                        case OperandType.InlineSwitch:
                            {
                                var count = reader.ReadUInt32();
                                for (var i = 0; i < count; i++)
                                {
                                    // data =...
                                    reader.ReadInt32();
                                }
                            }
                            break;
                        case OperandType.InlineTok:
                        case OperandType.InlineI:
                        case OperandType.InlineString:
                        case OperandType.InlineMethod:
                        case OperandType.InlineField:
                            var metaDataToken = reader.ReadMetadataToken();
                            Type[] genericMethodArguments = null;
                            if (method.IsGenericMethod)
                            {
                                genericMethodArguments = method.GetGenericArguments();
                            }

                            if (opCode.OperandType == OperandType.InlineMethod)
                            {
                                data = method.Module.ResolveMethod(metaDataToken, method.DeclaringType.GetGenericArguments(), genericMethodArguments);
                            }
                            else if (opCode.OperandType == OperandType.InlineField)
                            {
                                data = method.Module.ResolveField(metaDataToken, method.DeclaringType.GetGenericArguments(), genericMethodArguments);
                            }
                            else if (opCode.OperandType == OperandType.InlineString)
                            {
                                data = method.Module.ResolveString(metaDataToken);
                            }
                            else if (opCode.OperandType == OperandType.InlineI)
                            {
                                data = metaDataToken;
                            }
                            else if (opCode.OperandType == OperandType.InlineTok)
                            {
                                data = method.Module.ResolveType(metaDataToken);
                            }
                            break;
                        case OperandType.ShortInlineR:
                            data = reader.ReadSingle();
                            break;
                        case OperandType.InlineI8: // more dump?
                        case OperandType.InlineR:
                            if (opCode.OperandType == OperandType.InlineI8)
                            {
                                data = reader.ReadInt64();
                            }
                            else
                            {
                                data = reader.ReadDouble();
                            }
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    yield return new Instruction(position, opCode, data);
                }
            }
        }
        public struct Instruction
        {
            public readonly int Offset;
            public readonly OpCode OpCode;
            public readonly object Data;

            static readonly Regex TrimVersion = new Regex(", Version=.+, Culture=.+, PublicKeyToken=[0-9a-z]+", RegexOptions.Compiled);

            public Instruction(int offset, OpCode opCode, object data)
            {
                Offset = offset;
                OpCode = opCode;
                Data = data;
            }

            public override string ToString()
            {
                // format like LINQPad IL
                var addition = "";
                if (Data is int && OpCode == OpCodes.Switch) // switch
                {
                    // var offset = Offset;
                    // addition = "(" + string.Join(", ", Enumerable.Range(0, (int)Data).Select(x => "IL_" + (offset + x * 4).ToString("X4")).ToArray()) + ")";
                }
                else if ((OpCode.OperandType == OperandType.InlineBrTarget) && (Data is int))
                {
                    // note:jump position
                    // addition = "IL_" + (Offset + (int)Data).ToString("X4");
                }
                else if ((OpCode.OperandType == OperandType.ShortInlineBrTarget) && (Data is byte))
                {
                    // addition = "IL_" + (Offset + (byte)Data).ToString("X4");
                }
                else if (Data is byte)
                {
                    addition = ((byte)Data).ToString(); // I don't like hex format:)
                }
                else if (Data is int)
                {
                    addition = ((int)Data).ToString();
                }
                else if (Data is long)
                {
                    addition = ((long)Data).ToString();
                }
                else if (Data is double)
                {
                    addition = ((double)Data).ToString();
                }
                else if (Data is float)
                {
                    addition = ((float)Data).ToString();
                }
                else if (Data is MethodInfo)
                {
                    var info = Data as MethodInfo;
                    addition = TrimVersion.Replace(info.DeclaringType.FullName, "") + "." + info.Name;
                }
                else if (Data is ConstructorInfo)
                {
                    var info = Data as ConstructorInfo;
                    addition = TrimVersion.Replace(info.DeclaringType.FullName, "") + ".ctor";
                }
                else if (Data is FieldInfo)
                {
                    var info = Data as FieldInfo;
                    addition = TrimVersion.Replace(info.DeclaringType.FullName, "") + "." + info.Name;
                }
                else if (Data is string)
                {
                    addition = (string)Data;
                }
                else if (Data is Type)
                {
                    addition = TrimVersion.Replace(((Type)Data).FullName, "");
                }

                return string.Format("IL_{0,4:X4}:  {1, -11} {2}", Offset, OpCode, addition);
            }
        }
    }
}
