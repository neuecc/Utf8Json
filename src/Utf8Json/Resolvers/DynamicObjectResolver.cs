using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using Utf8Json.Internal.Emit;
using Utf8Json.Internal;
using Utf8Json.Formatters;
using System.Text.RegularExpressions;
using System.Threading;
using System.Reflection.Emit;

namespace Utf8Json.Resolvers
{
    /// <summary>
    /// ObjectResolver by dynamic code generation.
    /// </summary>
    public sealed class DynamicObjectResolver : IJsonFormatterResolver
    {
        public static readonly DynamicObjectResolver Instance = new DynamicObjectResolver();

        const string ModuleName = "Utf8Json.Resolvers.DynamicObjectResolver";

        internal static readonly DynamicAssembly assembly;

        DynamicObjectResolver()
        {

        }

        static DynamicObjectResolver()
        {
            assembly = new DynamicAssembly(ModuleName);
        }

#if NET_35
        public AssemblyBuilder Save()
        {
            return assembly.Save();
        }
#endif

        public IJsonFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.formatter;
        }

        static class FormatterCache<T>
        {
            public static readonly IJsonFormatter<T> formatter;

            static FormatterCache()
            {
                var ti = typeof(T).GetTypeInfo();

                if (ti.IsInterface)
                {
                    return;
                }

                if (ti.IsNullable())
                {
                    ti = ti.GenericTypeArguments[0].GetTypeInfo();

                    var innerFormatter = DynamicObjectResolver.Instance.GetFormatterDynamic(ti.AsType());
                    if (innerFormatter == null)
                    {
                        return;
                    }
                    formatter = (IJsonFormatter<T>)Activator.CreateInstance(typeof(StaticNullableFormatter<>).MakeGenericType(ti.AsType()), new object[] { innerFormatter });
                    return;
                }

                if (ti.IsAnonymous())
                {
                    // TODO:dynamic_private...?
                    // formatter = (IJsonFormatter<T>)DynamicPrivateFormatterBuilder.BuildFormatter(typeof(T));
                    return;
                }

                // TODO:build
                //var formatterTypeInfo = DynamicObjectTypeBuilder.BuildType(assembly, typeof(T), false, false);
                //if (formatterTypeInfo == null) return;

                // formatter = (IJsonFormatter<T>)Activator.CreateInstance(formatterTypeInfo.AsType());
            }
        }
    }
}

namespace Utf8Json.Resolvers.Internal
{
    internal static class DynamicObjectTypeBuilder
    {
#if NETSTANDARD
        static readonly Regex SubtractFullNameRegex = new Regex(@", Version=\d+.\d+.\d+.\d+, Culture=\w+, PublicKeyToken=\w+", RegexOptions.Compiled);
#else
        static readonly Regex SubtractFullNameRegex = new Regex(@", Version=\d+.\d+.\d+.\d+, Culture=\w+, PublicKeyToken=\w+");
#endif

        static int nameSequence = 0;

        static HashSet<Type> ignoreTypes = new HashSet<Type>
        {
            {typeof(object)},
            {typeof(short)},
            {typeof(int)},
            {typeof(long)},
            {typeof(ushort)},
            {typeof(uint)},
            {typeof(ulong)},
            {typeof(float)},
            {typeof(double)},
            {typeof(bool)},
            {typeof(byte)},
            {typeof(sbyte)},
            {typeof(decimal)},
            {typeof(char)},
            {typeof(string)},
            {typeof(System.Guid)},
            {typeof(System.TimeSpan)},
            {typeof(System.DateTime)},
            {typeof(System.DateTimeOffset)},
        };

        public static TypeInfo BuildType(DynamicAssembly assembly, Type type, Func<string, string> nameMutator)
        {
            if (ignoreTypes.Contains(type)) return null;

            var serializationInfo = new MetaType(type, nameMutator);

            var formatterType = typeof(IJsonFormatter<>).MakeGenericType(type);
            var typeBuilder = assembly.ModuleBuilder.DefineType("Utf8Json.Formatters." + SubtractFullNameRegex.Replace(type.FullName, "").Replace(".", "_") + "Formatter" + Interlocked.Increment(ref nameSequence), TypeAttributes.Public | TypeAttributes.Sealed, null, new[] { formatterType });

            FieldBuilder stringByteKeysField = null;
            // Dictionary<MetaMember, FieldInfo> customFormatterLookup = null;

            // for serialize, bake cache.
            {
                var method = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
                stringByteKeysField = typeBuilder.DefineField("stringByteKeys", typeof(byte[][]), FieldAttributes.Private | FieldAttributes.InitOnly);

                var il = method.GetILGenerator();
                BuildConstructor(type, serializationInfo, method, stringByteKeysField, il);
                // customFormatterLookup = BuildCustomFormatterField(typeBuilder, serializationInfo, il);
                il.Emit(OpCodes.Ret);
            }

            //{
            //    var method = typeBuilder.DefineMethod("Serialize", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual,
            //        typeof(int),
            //        new Type[] { typeof(byte[]).MakeByRefType(), typeof(int), type, typeof(IFormatterResolver) });

            //    var il = method.GetILGenerator();
            //    BuildSerialize(type, serializationInfo, method, stringByteKeysField, dict, il);
            //}

            //{
            //    var method = typeBuilder.DefineMethod("Deserialize", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual,
            //        type,
            //        new Type[] { typeof(byte[]), typeof(int), typeof(IFormatterResolver), typeof(int).MakeByRefType() });

            //    var il = method.GetILGenerator();
            //    BuildDeserialize(type, serializationInfo, method, il, dict);
            //}

            return typeBuilder.CreateTypeInfo();
        }

        // BuildAnonymous?

        static void BuildConstructor(Type type, MetaType info, ConstructorInfo method, FieldBuilder stringByteKeysField, ILGenerator il)
        {
            il.EmitLdarg(0);
            il.Emit(OpCodes.Call, EmitInfo.ObjectCtor);

            var writeCount = info.Members.Count(x => x.IsReadable);
            il.EmitLdarg(0);
            il.EmitLdc_I4(writeCount);
            il.Emit(OpCodes.Newarr, typeof(byte[]));

            var i = 0;
            foreach (var item in info.Members.Where(x => x.IsReadable))
            {
                il.Emit(OpCodes.Dup);
                il.EmitLdc_I4(i);
                il.Emit(OpCodes.Ldstr, item.Name);
                if (i == 0)
                {
                    il.EmitCall(EmitInfo.JsonWriter.GetEncodedPropertyNameWithBeginObject);
                }
                else
                {
                    il.EmitCall(EmitInfo.JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator);
                }

                il.Emit(OpCodes.Stelem_Ref);
                i++;
            }

            il.Emit(OpCodes.Stfld, stringByteKeysField);
        }

        //        static Dictionary<ObjectSerializationInfo.EmittableMember, FieldInfo> BuildCustomFormatterField(TypeBuilder builder, ObjectSerializationInfo info, ILGenerator il)
        //        {
        //            Dictionary<ObjectSerializationInfo.EmittableMember, FieldInfo> dict = new Dictionary<ObjectSerializationInfo.EmittableMember, FieldInfo>();
        //            foreach (var item in info.Members.Where(x => x.IsReadable || x.IsWritable))
        //            {
        //                var attr = item.GetMessagePackFormatterAttribtue();
        //                if (attr != null)
        //                {
        //                    var f = builder.DefineField(item.Name + "_formatter", attr.FormatterType, FieldAttributes.Private | FieldAttributes.InitOnly);

        //                    il.EmitLdarg(0);
        //                    il.Emit(OpCodes.Ldtoken, f.FieldType);
        //                    var getTypeFromHandle = ExpressionUtility.GetMethodInfo(() => Type.GetTypeFromHandle(default(RuntimeTypeHandle)));
        //                    il.Emit(OpCodes.Call, getTypeFromHandle);

        //                    if (attr.Arguments == null || attr.Arguments.Length == 0)
        //                    {
        //                        var mi = ExpressionUtility.GetMethodInfo(() => Activator.CreateInstance(default(Type)));
        //                        il.Emit(OpCodes.Call, mi);
        //                    }
        //                    else
        //                    {
        //                        il.EmitLdc_I4(attr.Arguments.Length);
        //                        il.Emit(OpCodes.Newarr, typeof(object));

        //                        var ii = 0;
        //                        foreach (var item2 in attr.Arguments)
        //                        {
        //                            il.Emit(OpCodes.Dup);
        //                            il.EmitLdc_I4(ii);
        //                            il.EmitConstant(item2);
        //                            if (item2.GetType().GetTypeInfo().IsValueType)
        //                            {
        //                                il.Emit(OpCodes.Box, item2.GetType());
        //                            }
        //                            il.Emit(OpCodes.Stelem_Ref);
        //                            ii++;
        //                        }

        //                        var mi = ExpressionUtility.GetMethodInfo(() => Activator.CreateInstance(default(Type), default(object[])));
        //                        il.Emit(OpCodes.Call, mi);
        //                    }

        //                    il.Emit(OpCodes.Castclass, attr.FormatterType);
        //                    il.Emit(OpCodes.Stfld, f);

        //                    dict.Add(item, f);
        //                }
        //            }

        //            return dict;
        //        }

        //        // int Serialize([arg:1]ref byte[] bytes, [arg:2]int offset, [arg:3]T value, [arg:4]IFormatterResolver formatterResolver);
        //        static void BuildSerialize(Type type, ObjectSerializationInfo info, MethodInfo method, FieldBuilder stringByteKeysField, Dictionary<ObjectSerializationInfo.EmittableMember, FieldInfo> customFormatterLookup, ILGenerator il)
        //        {
        //            // if(value == null) return WriteNil
        //            if (type.GetTypeInfo().IsClass)
        //            {
        //                var elseBody = il.DefineLabel();

        //                il.EmitLdarg(3);
        //                il.Emit(OpCodes.Brtrue_S, elseBody);
        //                il.EmitLdarg(1);
        //                il.EmitLdarg(2);
        //                il.EmitCall(MessagePackBinaryTypeInfo.WriteNil);
        //                il.Emit(OpCodes.Ret);

        //                il.MarkLabel(elseBody);
        //            }

        //            // IMessagePackSerializationCallbackReceiver.OnBeforeSerialize()
        //            if (type.GetTypeInfo().ImplementedInterfaces.Any(x => x == typeof(IMessagePackSerializationCallbackReceiver)))
        //            {
        //                // call directly
        //                var runtimeMethods = type.GetRuntimeMethods().Where(x => x.Name == "OnBeforeSerialize").ToArray();
        //                if (runtimeMethods.Length == 1)
        //                {
        //                    if (info.IsStruct)
        //                    {
        //                        il.EmitLdarga(3);
        //                    }
        //                    else
        //                    {
        //                        il.EmitLdarg(3);
        //                    }
        //                    il.Emit(OpCodes.Call, runtimeMethods[0]); // don't use EmitCall helper(must use 'Call')
        //                }
        //                else
        //                {
        //                    il.EmitLdarg(3);
        //                    if (info.IsStruct)
        //                    {
        //                        il.Emit(OpCodes.Box, type);
        //                    }
        //                    il.EmitCall(onBeforeSerialize);
        //                }
        //            }

        //            // var startOffset = offset;
        //            var startOffsetLocal = il.DeclareLocal(typeof(int)); // [loc:0]
        //            il.EmitLdarg(2);
        //            il.EmitStloc(startOffsetLocal);

        //            if (info.IsIntKey)
        //            {
        //                // use Array
        //                var maxKey = info.Members.Where(x => x.IsReadable).Select(x => x.IntKey).DefaultIfEmpty(-1).Max();
        //                var intKeyMap = info.Members.Where(x => x.IsReadable).ToDictionary(x => x.IntKey);

        //                EmitOffsetPlusEqual(il, null, () =>
        //                {
        //                    var len = maxKey + 1;
        //                    il.EmitLdc_I4(len);
        //                    if (len <= MessagePackRange.MaxFixArrayCount)
        //                    {
        //                        il.EmitCall(MessagePackBinaryTypeInfo.WriteFixedArrayHeaderUnsafe);
        //                    }
        //                    else
        //                    {
        //                        il.EmitCall(MessagePackBinaryTypeInfo.WriteArrayHeader);
        //                    }
        //                });

        //                for (int i = 0; i <= maxKey; i++)
        //                {
        //                    ObjectSerializationInfo.EmittableMember member;
        //                    if (intKeyMap.TryGetValue(i, out member))
        //                    {
        //                        // offset += serialzie
        //                        EmitSerializeValue(il, type.GetTypeInfo(), member, customFormatterLookup);
        //                    }
        //                    else
        //                    {
        //                        // Write Nil as Blanc
        //                        EmitOffsetPlusEqual(il, null, () =>
        //                        {
        //                            il.EmitCall(MessagePackBinaryTypeInfo.WriteNil);
        //                        });
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                // use Map
        //                var writeCount = info.Members.Count(x => x.IsReadable);

        //                EmitOffsetPlusEqual(il, null, () =>
        //                {
        //                    il.EmitLdc_I4(writeCount);
        //                    if (writeCount <= MessagePackRange.MaxFixMapCount)
        //                    {
        //                        il.EmitCall(MessagePackBinaryTypeInfo.WriteFixedMapHeaderUnsafe);
        //                    }
        //                    else
        //                    {
        //                        il.EmitCall(MessagePackBinaryTypeInfo.WriteMapHeader);
        //                    }
        //                });

        //                var index = 0;
        //                foreach (var item in info.Members.Where(x => x.IsReadable))
        //                {
        //                    // offset += writekey
        //                    EmitOffsetPlusEqual(il, null, () =>
        //                    {
        //                        il.EmitLdarg(0);
        //                        il.EmitLdfld(stringByteKeysField);
        //                        il.EmitLdc_I4(index);
        //                        il.Emit(OpCodes.Ldelem_Ref);

        //                        // Optimize, WriteRaw(Unity, large) or UnsafeMemory32/64.WriteRawX
        //#if NETSTANDARD1_4
        //                        var valueLen = MessagePackBinary.GetEncodedStringBytes(item.StringKey).Length;
        //                        if (valueLen <= MessagePackRange.MaxFixStringLength)
        //                        {
        //                            if (UnsafeMemory.Is32Bit)
        //                            {
        //                                il.EmitCall(typeof(UnsafeMemory32).GetRuntimeMethod("WriteRaw" + valueLen, new[] { refByte, typeof(int), typeof(byte[]) }));
        //                            }
        //                            else
        //                            {
        //                                il.EmitCall(typeof(UnsafeMemory64).GetRuntimeMethod("WriteRaw" + valueLen, new[] { refByte, typeof(int), typeof(byte[]) }));
        //                            }
        //                        }
        //                        else
        //#endif
        //                        {
        //                            il.EmitCall(MessagePackBinaryTypeInfo.WriteRaw);
        //                        }
        //                        index++;
        //                    });

        //                    // offset += serialzie
        //                    EmitSerializeValue(il, type.GetTypeInfo(), item, customFormatterLookup);
        //                }
        //            }

        //            // return startOffset- offset;
        //            il.EmitLdarg(2);
        //            il.EmitLdloc(startOffsetLocal);
        //            il.Emit(OpCodes.Sub);
        //            il.Emit(OpCodes.Ret);
        //        }

        //        // offset += ***(ref bytes, offset....
        //        static void EmitOffsetPlusEqual(ILGenerator il, Action loadEmit, Action emit)
        //        {
        //            il.EmitLdarg(2);

        //            if (loadEmit != null) loadEmit();

        //            il.EmitLdarg(1);
        //            il.EmitLdarg(2);

        //            emit();

        //            il.Emit(OpCodes.Add);
        //            il.EmitStarg(2);
        //        }

        //        static void EmitSerializeValue(ILGenerator il, TypeInfo type, ObjectSerializationInfo.EmittableMember member, Dictionary<ObjectSerializationInfo.EmittableMember, FieldInfo> customFormatterLookup)
        //        {
        //            var t = member.Type;
        //            FieldInfo customFormatter;
        //            if (customFormatterLookup.TryGetValue(member, out customFormatter))
        //            {
        //                EmitOffsetPlusEqual(il, () =>
        //                {
        //                    il.Emit(OpCodes.Ldarg_0);
        //                    il.EmitLdfld(customFormatter);
        //                }, () =>
        //                {
        //                    il.EmitLoadArg(type, 3);
        //                    member.EmitLoadValue(il);
        //                    il.EmitLdarg(4);
        //                    il.EmitCall(customFormatter.FieldType.GetRuntimeMethod("Serialize", new[] { refByte, typeof(int), t, typeof(IFormatterResolver) }));
        //                });
        //            }
        //            else if (IsOptimizeTargetType(t))
        //            {
        //                EmitOffsetPlusEqual(il, null, () =>
        //                {
        //                    il.EmitLoadArg(type, 3);
        //                    member.EmitLoadValue(il);
        //                    if (t == typeof(byte[]))
        //                    {
        //                        il.EmitCall(MessagePackBinaryTypeInfo.WriteBytes);
        //                    }
        //                    else
        //                    {
        //                        il.EmitCall(MessagePackBinaryTypeInfo.TypeInfo.GetDeclaredMethods("Write" + t.Name).OrderByDescending(x => x.GetParameters().Length).First());
        //                    }
        //                });
        //            }
        //            else
        //            {
        //                EmitOffsetPlusEqual(il, () =>
        //                {
        //                    il.EmitLdarg(4);
        //                    il.Emit(OpCodes.Call, getFormatterWithVerify.MakeGenericMethod(t));
        //                }, () =>
        //                {
        //                    il.EmitLoadArg(type, 3);
        //                    member.EmitLoadValue(il);
        //                    il.EmitLdarg(4);
        //                    il.EmitCall(getSerialize(t));
        //                });
        //            }
        //        }

        //        // T Deserialize([arg:1]byte[] bytes, [arg:2]int offset, [arg:3]IFormatterResolver formatterResolver, [arg:4]out int readSize);
        //        static void BuildDeserialize(Type type, ObjectSerializationInfo info, MethodBuilder method, ILGenerator il, Dictionary<ObjectSerializationInfo.EmittableMember, FieldInfo> customFormatterLookup)
        //        {
        //            // if(MessagePackBinary.IsNil) readSize = 1, return null;
        //            var falseLabel = il.DefineLabel();
        //            il.EmitLdarg(1);
        //            il.EmitLdarg(2);
        //            il.EmitCall(MessagePackBinaryTypeInfo.IsNil);
        //            il.Emit(OpCodes.Brfalse_S, falseLabel);
        //            if (type.GetTypeInfo().IsClass)
        //            {
        //                il.EmitLdarg(4);
        //                il.EmitLdc_I4(1);
        //                il.Emit(OpCodes.Stind_I4);
        //                il.Emit(OpCodes.Ldnull);
        //                il.Emit(OpCodes.Ret);
        //            }
        //            else
        //            {
        //                il.Emit(OpCodes.Ldstr, "typecode is null, struct not supported");
        //                il.Emit(OpCodes.Newobj, invalidOperationExceptionConstructor);
        //                il.Emit(OpCodes.Throw);
        //            }

        //            // var startOffset = offset;
        //            il.MarkLabel(falseLabel);
        //            var startOffsetLocal = il.DeclareLocal(typeof(int)); // [loc:0]
        //            il.EmitLdarg(2);
        //            il.EmitStloc(startOffsetLocal);

        //            // var length = ReadMapHeader
        //            var length = il.DeclareLocal(typeof(int)); // [loc:1]
        //            il.EmitLdarg(1);
        //            il.EmitLdarg(2);
        //            il.EmitLdarg(4);

        //            if (info.IsIntKey)
        //            {
        //                il.EmitCall(MessagePackBinaryTypeInfo.ReadArrayHeader);
        //            }
        //            else
        //            {
        //                il.EmitCall(MessagePackBinaryTypeInfo.ReadMapHeader);
        //            }
        //            il.EmitStloc(length);
        //            EmitOffsetPlusReadSize(il);

        //            // make local fields
        //            Label? gotoDefault = null;
        //            DeserializeInfo[] infoList;
        //            if (info.IsIntKey)
        //            {
        //                var maxKey = info.Members.Select(x => x.IntKey).DefaultIfEmpty(-1).Max();
        //                var len = maxKey + 1;
        //                var intKeyMap = info.Members.ToDictionary(x => x.IntKey);

        //                infoList = Enumerable.Range(0, len)
        //                    .Select(x =>
        //                    {
        //                        ObjectSerializationInfo.EmittableMember member;
        //                        if (intKeyMap.TryGetValue(x, out member))
        //                        {
        //                            return new DeserializeInfo
        //                            {
        //                                MemberInfo = member,
        //                                LocalField = il.DeclareLocal(member.Type),
        //                                SwitchLabel = il.DefineLabel()
        //                            };
        //                        }
        //                        else
        //                        {
        //                            // return null MemberInfo, should filter null
        //                            if (gotoDefault == null)
        //                            {
        //                                gotoDefault = il.DefineLabel();
        //                            }
        //                            return new DeserializeInfo
        //                            {
        //                                MemberInfo = null,
        //                                LocalField = null,
        //                                SwitchLabel = gotoDefault.Value,
        //                            };
        //                        }
        //                    })
        //                    .ToArray();
        //            }
        //            else
        //            {
        //                infoList = info.Members
        //                    .Select(item => new DeserializeInfo
        //                    {
        //                        MemberInfo = item,
        //                        LocalField = il.DeclareLocal(item.Type),
        //                        // SwitchLabel = il.DefineLabel()
        //                    })
        //                    .ToArray();
        //            }

        //            // Read Loop(for var i = 0; i< length; i++)
        //            if (info.IsStringKey)
        //            {
        //                var automata = new AutomataDictionary();
        //                for (int i = 0; i < info.Members.Length; i++)
        //                {
        //                    automata.Add(info.Members[i].StringKey, i);
        //                }

        //                var buffer = il.DeclareLocal(typeof(byte).MakeByRefType(), true);
        //                var keyArraySegment = il.DeclareLocal(typeof(ArraySegment<byte>));
        //                var longKey = il.DeclareLocal(typeof(ulong));
        //                var p = il.DeclareLocal(typeof(byte*));
        //                var rest = il.DeclareLocal(typeof(int));

        //                // fixed (byte* buffer = &bytes[0]) {
        //                il.EmitLdarg(1);
        //                il.EmitLdc_I4(0);
        //                il.Emit(OpCodes.Ldelema, typeof(byte));
        //                il.EmitStloc(buffer);

        //                // for (int i = 0; i < len; i++)
        //                il.EmitIncrementFor(length, forILocal =>
        //                {
        //                    var readNext = il.DefineLabel();
        //                    var loopEnd = il.DefineLabel();

        //                    il.EmitLdarg(1);
        //                    il.EmitLdarg(2);
        //                    il.EmitLdarg(4);
        //                    il.EmitCall(MessagePackBinaryTypeInfo.ReadStringSegment);
        //                    il.EmitStloc(keyArraySegment);
        //                    EmitOffsetPlusReadSize(il);

        //                    // p = buffer + arraySegment.Offset
        //                    il.EmitLdloc(buffer);
        //                    il.Emit(OpCodes.Conv_I);
        //                    il.EmitLdloca(keyArraySegment);
        //                    il.EmitCall(typeof(ArraySegment<byte>).GetRuntimeProperty("Offset").GetGetMethod());
        //                    il.Emit(OpCodes.Add);
        //                    il.EmitStloc(p);

        //                    // rest = arraySegment.Count
        //                    il.EmitLdloca(keyArraySegment);
        //                    il.EmitCall(typeof(ArraySegment<byte>).GetRuntimeProperty("Count").GetGetMethod());
        //                    il.EmitStloc(rest);

        //                    // if(rest == 0) goto End
        //                    il.EmitLdloc(rest);
        //                    il.Emit(OpCodes.Brfalse, readNext);

        //                    // gen automata name lookup
        //                    automata.EmitMatch(il, p, rest, longKey, x =>
        //                    {
        //                        var i = x.Value;
        //                        if (infoList[i].MemberInfo != null)
        //                        {
        //                            EmitDeserializeValue(il, infoList[i], customFormatterLookup);
        //                            il.Emit(OpCodes.Br, loopEnd);
        //                        }
        //                        else
        //                        {
        //                            il.Emit(OpCodes.Br, readNext);
        //                        }
        //                    }, () =>
        //                    {
        //                        il.Emit(OpCodes.Br, readNext);
        //                    });

        //                    il.MarkLabel(readNext);
        //                    il.EmitLdarg(4);
        //                    il.EmitLdarg(1);
        //                    il.EmitLdarg(2);
        //                    il.EmitCall(MessagePackBinaryTypeInfo.ReadNextBlock);
        //                    il.Emit(OpCodes.Stind_I4);

        //                    il.MarkLabel(loopEnd);
        //                    EmitOffsetPlusReadSize(il);
        //                });

        //                // end fixed
        //                il.Emit(OpCodes.Ldc_I4_0);
        //                il.Emit(OpCodes.Conv_U);
        //                il.EmitStloc(buffer);
        //            }
        //            else
        //            {
        //                var key = il.DeclareLocal(typeof(int));
        //                var switchDefault = il.DefineLabel();

        //                il.EmitIncrementFor(length, forILocal =>
        //                {
        //                    var loopEnd = il.DefineLabel();

        //                    il.EmitLdloc(forILocal);
        //                    il.EmitStloc(key);

        //                    // switch... local = Deserialize
        //                    il.EmitLdloc(key);

        //                    il.Emit(OpCodes.Switch, infoList.Select(x => x.SwitchLabel).ToArray());

        //                    il.MarkLabel(switchDefault);
        //                    // default, only read. readSize = MessagePackBinary.ReadNextBlock(bytes, offset);
        //                    il.EmitLdarg(4);
        //                    il.EmitLdarg(1);
        //                    il.EmitLdarg(2);
        //                    il.EmitCall(MessagePackBinaryTypeInfo.ReadNextBlock);
        //                    il.Emit(OpCodes.Stind_I4);
        //                    il.Emit(OpCodes.Br, loopEnd);

        //                    if (gotoDefault != null)
        //                    {
        //                        il.MarkLabel(gotoDefault.Value);
        //                        il.Emit(OpCodes.Br, switchDefault);
        //                    }

        //                    foreach (var item in infoList)
        //                    {
        //                        if (item.MemberInfo != null)
        //                        {
        //                            il.MarkLabel(item.SwitchLabel);
        //                            EmitDeserializeValue(il, item, customFormatterLookup);
        //                            il.Emit(OpCodes.Br, loopEnd);
        //                        }
        //                    }

        //                    // offset += readSize
        //                    il.MarkLabel(loopEnd);
        //                    EmitOffsetPlusReadSize(il);
        //                });
        //            }

        //            // finish readSize: readSize = offset - startOffset;
        //            il.EmitLdarg(4);
        //            il.EmitLdarg(2);
        //            il.EmitLdloc(startOffsetLocal);
        //            il.Emit(OpCodes.Sub);
        //            il.Emit(OpCodes.Stind_I4);

        //            // create result object
        //            var structLocal = EmitNewObject(il, type, info, infoList);

        //            // IMessagePackSerializationCallbackReceiver.OnAfterDeserialize()
        //            if (type.GetTypeInfo().ImplementedInterfaces.Any(x => x == typeof(IMessagePackSerializationCallbackReceiver)))
        //            {
        //                // call directly
        //                var runtimeMethods = type.GetRuntimeMethods().Where(x => x.Name == "OnAfterDeserialize").ToArray();
        //                if (runtimeMethods.Length == 1)
        //                {
        //                    if (info.IsClass)
        //                    {
        //                        il.Emit(OpCodes.Dup);
        //                    }
        //                    else
        //                    {
        //                        il.EmitLdloca(structLocal);
        //                    }

        //                    il.Emit(OpCodes.Call, runtimeMethods[0]); // don't use EmitCall helper(must use 'Call')
        //                }
        //                else
        //                {
        //                    if (info.IsStruct)
        //                    {
        //                        il.EmitLdloc(structLocal);
        //                        il.Emit(OpCodes.Box, type);
        //                    }
        //                    else
        //                    {
        //                        il.Emit(OpCodes.Dup);
        //                    }
        //                    il.EmitCall(onAfterDeserialize);
        //                }
        //            }

        //            if (info.IsStruct)
        //            {
        //                il.Emit(OpCodes.Ldloc, structLocal);
        //            }


        //            il.Emit(OpCodes.Ret);
        //        }

        //        static void EmitOffsetPlusReadSize(ILGenerator il)
        //        {
        //            il.EmitLdarg(2);
        //            il.EmitLdarg(4);
        //            il.Emit(OpCodes.Ldind_I4);
        //            il.Emit(OpCodes.Add);
        //            il.EmitStarg(2);
        //        }

        //        static void EmitDeserializeValue(ILGenerator il, DeserializeInfo info, Dictionary<ObjectSerializationInfo.EmittableMember, FieldInfo> customFormatterLookup)
        //        {
        //            var member = info.MemberInfo;
        //            var t = member.Type;
        //            FieldInfo customFormatter;
        //            if (customFormatterLookup.TryGetValue(member, out customFormatter))
        //            {
        //                il.Emit(OpCodes.Ldarg_0);
        //                il.EmitLdfld(customFormatter);
        //                il.EmitLdarg(1);
        //                il.EmitLdarg(2);
        //                il.EmitLdarg(3);
        //                il.EmitLdarg(4);
        //                il.EmitCall(customFormatter.FieldType.GetRuntimeMethod("Deserialize", new[] { typeof(byte[]), typeof(int), typeof(IFormatterResolver), refInt }));
        //            }
        //            else if (IsOptimizeTargetType(t))
        //            {
        //                il.EmitLdarg(1);
        //                il.EmitLdarg(2);
        //                il.EmitLdarg(4);
        //                if (t == typeof(byte[]))
        //                {
        //                    il.EmitCall(MessagePackBinaryTypeInfo.ReadBytes);
        //                }
        //                else
        //                {
        //                    il.EmitCall(MessagePackBinaryTypeInfo.TypeInfo.GetDeclaredMethods("Read" + t.Name).OrderByDescending(x => x.GetParameters().Length).First());
        //                }
        //            }
        //            else
        //            {
        //                il.EmitLdarg(3);
        //                il.EmitCall(getFormatterWithVerify.MakeGenericMethod(t));
        //                il.EmitLdarg(1);
        //                il.EmitLdarg(2);
        //                il.EmitLdarg(3);
        //                il.EmitLdarg(4);
        //                il.EmitCall(getDeserialize(t));
        //            }

        //            il.EmitStloc(info.LocalField);
        //        }

        //        static LocalBuilder EmitNewObject(ILGenerator il, Type type, ObjectSerializationInfo info, DeserializeInfo[] members)
        //        {
        //            if (info.IsClass)
        //            {
        //                foreach (var item in info.ConstructorParameters)
        //                {
        //                    var local = members.First(x => x.MemberInfo == item);
        //                    il.EmitLdloc(local.LocalField);
        //                }
        //                il.Emit(OpCodes.Newobj, info.BestmatchConstructor);

        //                foreach (var item in members.Where(x => x.MemberInfo != null && x.MemberInfo.IsWritable))
        //                {
        //                    il.Emit(OpCodes.Dup);
        //                    il.EmitLdloc(item.LocalField);
        //                    item.MemberInfo.EmitStoreValue(il);
        //                }

        //                return null;
        //            }
        //            else
        //            {
        //                var result = il.DeclareLocal(type);
        //                if (info.BestmatchConstructor == null)
        //                {
        //                    il.Emit(OpCodes.Ldloca, result);
        //                    il.Emit(OpCodes.Initobj, type);
        //                }
        //                else
        //                {
        //                    foreach (var item in info.ConstructorParameters)
        //                    {
        //                        var local = members.First(x => x.MemberInfo == item);
        //                        il.EmitLdloc(local.LocalField);
        //                    }
        //                    il.Emit(OpCodes.Newobj, info.BestmatchConstructor);
        //                    il.Emit(OpCodes.Stloc, result);
        //                }

        //                foreach (var item in members.Where(x => x.MemberInfo != null && x.MemberInfo.IsWritable))
        //                {
        //                    il.EmitLdloca(result);
        //                    il.EmitLdloc(item.LocalField);
        //                    item.MemberInfo.EmitStoreValue(il);
        //                }

        //                return result; // struct returns local result field
        //            }
        //        }

        //        static bool IsOptimizeTargetType(Type type)
        //        {
        //            if (type == typeof(Int16)
        //             || type == typeof(Int32)
        //             || type == typeof(Int64)
        //             || type == typeof(UInt16)
        //             || type == typeof(UInt32)
        //             || type == typeof(UInt64)
        //             || type == typeof(Single)
        //             || type == typeof(Double)
        //             || type == typeof(bool)
        //             || type == typeof(byte)
        //             || type == typeof(sbyte)
        //             || type == typeof(char)
        //             // not includes DateTime and String and Binary.
        //             //|| type == typeof(DateTime)
        //             //|| type == typeof(string)
        //             //|| type == typeof(byte[])
        //             )
        //            {
        //                return true;
        //            }
        //            return false;
        //        }

        //        // EmitInfos...

        //        static readonly Type refByte = typeof(byte[]).MakeByRefType();
        //        static readonly Type refInt = typeof(int).MakeByRefType();
        //        static readonly MethodInfo getFormatterWithVerify = typeof(FormatterResolverExtensions).GetRuntimeMethods().First(x => x.Name == "GetFormatterWithVerify");
        //        static readonly Func<Type, MethodInfo> getSerialize = t => typeof(IMessagePackFormatter<>).MakeGenericType(t).GetRuntimeMethod("Serialize", new[] { refByte, typeof(int), t, typeof(IFormatterResolver) });
        //        static readonly Func<Type, MethodInfo> getDeserialize = t => typeof(IMessagePackFormatter<>).MakeGenericType(t).GetRuntimeMethod("Deserialize", new[] { typeof(byte[]), typeof(int), typeof(IFormatterResolver), refInt });
        //        // static readonly ConstructorInfo dictionaryConstructor = typeof(ByteArrayStringHashTable).GetTypeInfo().DeclaredConstructors.First(x => { var p = x.GetParameters(); return p.Length == 1 && p[0].ParameterType == typeof(int); });
        //        // static readonly MethodInfo dictionaryAdd = typeof(ByteArrayStringHashTable).GetRuntimeMethod("Add", new[] { typeof(string), typeof(int) });
        //        // static readonly MethodInfo dictionaryTryGetValue = typeof(ByteArrayStringHashTable).GetRuntimeMethod("TryGetValue", new[] { typeof(ArraySegment<byte>), refInt });
        //        static readonly ConstructorInfo invalidOperationExceptionConstructor = typeof(System.InvalidOperationException).GetTypeInfo().DeclaredConstructors.First(x => { var p = x.GetParameters(); return p.Length == 1 && p[0].ParameterType == typeof(string); });

        //        static readonly MethodInfo onBeforeSerialize = typeof(IMessagePackSerializationCallbackReceiver).GetRuntimeMethod("OnBeforeSerialize", Type.EmptyTypes);
        //        static readonly MethodInfo onAfterDeserialize = typeof(IMessagePackSerializationCallbackReceiver).GetRuntimeMethod("OnAfterDeserialize", Type.EmptyTypes);

        //        static readonly ConstructorInfo objectCtor = typeof(object).GetTypeInfo().DeclaredConstructors.First(x => x.GetParameters().Length == 0);

        //        internal static class MessagePackBinaryTypeInfo
        //        {
        //            public static TypeInfo TypeInfo = typeof(MessagePackBinary).GetTypeInfo();

        //            public static readonly MethodInfo GetEncodedStringBytes = typeof(MessagePackBinary).GetRuntimeMethod("GetEncodedStringBytes", new[] { typeof(string) });
        //            public static MethodInfo WriteFixedMapHeaderUnsafe = typeof(MessagePackBinary).GetRuntimeMethod("WriteFixedMapHeaderUnsafe", new[] { refByte,
        //typeof(int), typeof(int) });
        //            public static MethodInfo WriteFixedArrayHeaderUnsafe = typeof(MessagePackBinary).GetRuntimeMethod("WriteFixedArrayHeaderUnsafe", new[] { refByte, typeof(int), typeof(int) });
        //            public static MethodInfo WriteMapHeader = typeof(MessagePackBinary).GetRuntimeMethod("WriteMapHeader", new[] { refByte, typeof(int), typeof(int) });
        //            public static MethodInfo WriteArrayHeader = typeof(MessagePackBinary).GetRuntimeMethod("WriteArrayHeader", new[] { refByte, typeof(int), typeof(int) });
        //            public static MethodInfo WritePositiveFixedIntUnsafe = typeof(MessagePackBinary).GetRuntimeMethod("WritePositiveFixedIntUnsafe", new[] { refByte, typeof(int), typeof(int) });
        //            public static MethodInfo WriteInt32 = typeof(MessagePackBinary).GetRuntimeMethod("WriteInt32", new[] { refByte, typeof(int), typeof(int) });
        //            public static MethodInfo WriteBytes = typeof(MessagePackBinary).GetRuntimeMethod("WriteBytes", new[] { refByte, typeof(int), typeof(byte[]) });
        //            public static MethodInfo WriteNil = typeof(MessagePackBinary).GetRuntimeMethod("WriteNil", new[] { refByte, typeof(int) });
        //            public static MethodInfo ReadBytes = typeof(MessagePackBinary).GetRuntimeMethod("ReadBytes", new[] { typeof(byte[]), typeof(int), refInt });
        //            public static MethodInfo ReadInt32 = typeof(MessagePackBinary).GetRuntimeMethod("ReadInt32", new[] { typeof(byte[]), typeof(int), refInt });
        //            public static MethodInfo ReadString = typeof(MessagePackBinary).GetRuntimeMethod("ReadString", new[] { typeof(byte[]), typeof(int), refInt });
        //            public static MethodInfo ReadStringSegment = typeof(MessagePackBinary).GetRuntimeMethod("ReadStringSegment", new[] { typeof(byte[]), typeof(int), refInt });
        //            public static MethodInfo IsNil = typeof(MessagePackBinary).GetRuntimeMethod("IsNil", new[] { typeof(byte[]), typeof(int) });
        //            public static MethodInfo ReadNextBlock = typeof(MessagePackBinary).GetRuntimeMethod("ReadNextBlock", new[] { typeof(byte[]), typeof(int) });
        //            public static MethodInfo WriteStringUnsafe = typeof(MessagePackBinary).GetRuntimeMethod("WriteStringUnsafe", new[] { refByte, typeof(int), typeof(string), typeof(int) });
        //            public static MethodInfo WriteStringBytes = typeof(MessagePackBinary).GetRuntimeMethod("WriteStringBytes", new[] { refByte, typeof(int), typeof(byte[]) });
        //            public static MethodInfo WriteRaw = typeof(MessagePackBinary).GetRuntimeMethod("WriteRaw", new[] { refByte, typeof(int), typeof(byte[]) });

        //            public static MethodInfo ReadArrayHeader = typeof(MessagePackBinary).GetRuntimeMethod("ReadArrayHeader", new[] { typeof(byte[]), typeof(int), refInt });
        //            public static MethodInfo ReadMapHeader = typeof(MessagePackBinary).GetRuntimeMethod("ReadMapHeader", new[] { typeof(byte[]), typeof(int), refInt });

        //            static MessagePackBinaryTypeInfo()
        //            {
        //            }
        //        }

        //        class DeserializeInfo
        //        {
        //            public ObjectSerializationInfo.EmittableMember MemberInfo { get; set; }
        //            public LocalBuilder LocalField { get; set; }
        //            public Label SwitchLabel { get; set; }
        //        }
    }

    internal static class EmitInfo
    {
        public static readonly ConstructorInfo ObjectCtor = typeof(object).GetTypeInfo().DeclaredConstructors.First(x => x.GetParameters().Length == 0);

        internal static class JsonWriter
        {
            public static readonly MethodInfo GetEncodedPropertyNameWithBeginObject = ExpressionUtility.GetMethodInfo(() => Utf8Json.JsonWriter.GetEncodedPropertyNameWithBeginObject(default(string)));

            public static readonly MethodInfo GetEncodedPropertyNameWithPrefixValueSeparator = ExpressionUtility.GetMethodInfo(() => Utf8Json.JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator(default(string)));

            public static readonly MethodInfo GetEncodedPropertyNameWithoutQuotation = ExpressionUtility.GetMethodInfo(() => Utf8Json.JsonWriter.GetEncodedPropertyNameWithoutQuotation(default(string)));

            public static readonly MethodInfo GetEncodedPropertyName = ExpressionUtility.GetMethodInfo(() => Utf8Json.JsonWriter.GetEncodedPropertyName(default(string)));
        }
    }

    internal class Utf8JsonDynamicObjectResolverException : Exception
    {
        public Utf8JsonDynamicObjectResolverException(string message)
            : base(message)
        {

        }
    }
}