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
using Utf8Json.Resolvers.Internal;

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

                var formatterTypeInfo = DynamicObjectTypeBuilder.BuildType(assembly, typeof(T), x => x); // TODO:name mutator?
                if (formatterTypeInfo == null) return;

                formatter = (IJsonFormatter<T>)Activator.CreateInstance(formatterTypeInfo.AsType());
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

        static HashSet<Type> jsonPrimitiveTypes = new HashSet<Type>
        {
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
            {typeof(string)},
        };

        public static TypeInfo BuildType(DynamicAssembly assembly, Type type, Func<string, string> nameMutator)
        {
            if (ignoreTypes.Contains(type)) return null;

            var serializationInfo = new MetaType(type, nameMutator);

            var formatterType = typeof(IJsonFormatter<>).MakeGenericType(type);
            var typeBuilder = assembly.ModuleBuilder.DefineType("Utf8Json.Formatters." + SubtractFullNameRegex.Replace(type.FullName, "").Replace(".", "_") + "Formatter" + Interlocked.Increment(ref nameSequence), TypeAttributes.Public | TypeAttributes.Sealed, null, new[] { formatterType });

            FieldBuilder stringByteKeysField = null;
            Dictionary<MetaMember, FieldInfo> customFormatterLookup = new Dictionary<MetaMember, FieldInfo>();

            // for serialize, bake cache.
            {
                var method = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
                stringByteKeysField = typeBuilder.DefineField("stringByteKeys", typeof(byte[][]), FieldAttributes.Private | FieldAttributes.InitOnly);

                var il = method.GetILGenerator();
                BuildConstructor(type, serializationInfo, method, stringByteKeysField, il);
                // TODO:...
                // customFormatterLookup = BuildCustomFormatterField(typeBuilder, serializationInfo, il);
            }

            {
                var method = typeBuilder.DefineMethod("Serialize", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual,
                    null,
                    new Type[] { typeof(JsonWriter).MakeByRefType(), type, typeof(IJsonFormatterResolver) });

                var il = method.GetILGenerator();
                BuildSerialize(type, serializationInfo, method, il, stringByteKeysField, customFormatterLookup);
            }

            {
                var method = typeBuilder.DefineMethod("Deserialize", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual,
                    type,
                    new Type[] { typeof(JsonReader).MakeByRefType(), typeof(IJsonFormatterResolver) });

                var il = method.GetILGenerator();
                BuildDeserialize(type, serializationInfo, method, il, customFormatterLookup);
            }

            return typeBuilder.CreateTypeInfo();
        }

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
            il.Emit(OpCodes.Ret);
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


        // public void Serialize(ref JsonWriter writer, SimplePerson value, IJsonFormatterResolver formatterResolver)
        // delegate void Serialize(byte[][] stringByteKeysField, ref JsonWriter writer, SimplePerson value, IJsonFormatterResolver formatterResolver)
        static void BuildSerialize(Type type, MetaType info, MethodInfo method, ILGenerator il, FieldBuilder stringByteKeysField, Dictionary<MetaMember, FieldInfo> customFormatterLookup)
        {
            var argWriter = new ArgumentField(il, 1);
            var argValue = new ArgumentField(il, 2, type);
            var argResolver = new ArgumentField(il, 3);

            var typeInfo = type.GetTypeInfo();

            // if(value == null) { writer.WriteNull(); return; }
            if (typeInfo.IsClass)
            {
                var elseBody = il.DefineLabel();

                argValue.EmitLoad();
                il.Emit(OpCodes.Brtrue_S, elseBody);

                argWriter.EmitLoad();
                il.EmitCall(EmitInfo.JsonWriter.WriteNull);
                il.Emit(OpCodes.Ret); // return;

                il.MarkLabel(elseBody);
            }

            // for-loop WriteRaw -> WriteValue, EndObject

            var index = 0;
            foreach (var item in info.Members.Where(x => x.IsReadable))
            {
                byte[] rawField = (index == 0) ? JsonWriter.GetEncodedPropertyNameWithBeginObject(item.Name) : JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator(item.Name);

                // WriteRaw
                argWriter.EmitLoad();
                il.EmitLoadThis();
                il.EmitLdfld(stringByteKeysField);
                il.EmitLdc_I4(index);
                il.Emit(OpCodes.Ldelem_Ref);
                if (rawField.Length < 32)
                {
                    if (UnsafeMemory.Is32Bit)
                    {
                        il.EmitCall(typeof(UnsafeMemory32).GetRuntimeMethod("WriteRaw" + rawField.Length, new[] { typeof(JsonWriter).MakeByRefType(), typeof(byte[]) }));
                    }
                    else
                    {
                        il.EmitCall(typeof(UnsafeMemory64).GetRuntimeMethod("WriteRaw" + rawField.Length, new[] { typeof(JsonWriter).MakeByRefType(), typeof(byte[]) }));
                    }
                }
                else
                {
                    il.EmitCall(EmitInfo.UnsafeMemory_MemoryCopy);
                }

                // EmitValue
                EmitSerializeValue(typeInfo, item, il, customFormatterLookup, argWriter, argValue, argResolver);

                index++;
            }

            argWriter.EmitLoad();
            il.EmitCall(EmitInfo.JsonWriter.WriteEndObject);
            il.Emit(OpCodes.Ret);
        }


        static void EmitSerializeValue(TypeInfo type, MetaMember member, ILGenerator il, Dictionary<MetaMember, FieldInfo> customFormatterLookup, ArgumentField writer, ArgumentField argValue, ArgumentField argResolver)
        {
            var t = member.Type;
            FieldInfo customFormatter;
            if (customFormatterLookup.TryGetValue(member, out customFormatter))
            {
                throw new NotImplementedException();
                // TODO:write custom serialize
                //EmitOffsetPlusEqual(il, () =>
                //{
                //    il.Emit(OpCodes.Ldarg_0);
                //    il.EmitLdfld(customFormatter);
                //}, () =>
                //{
                //    il.EmitLoadArg(type, 3);
                //    member.EmitLoadValue(il);
                //    il.EmitLdarg(4);
                //    il.EmitCall(customFormatter.FieldType.GetRuntimeMethod("Serialize", new[] { refByte, typeof(int), t, typeof(IFormatterResolver) }));
                //});
            }
            else if (jsonPrimitiveTypes.Contains(t))
            {
                writer.EmitLoad();
                argValue.EmitLoad();
                member.EmitLoadValue(il);
                il.EmitCall(typeof(JsonWriter).GetTypeInfo().GetDeclaredMethods("Write" + t.Name).OrderByDescending(x => x.GetParameters().Length).First());
            }
            else
            {
                argResolver.EmitLoad();
                il.Emit(OpCodes.Call, EmitInfo.GetFormatterWithVerify.MakeGenericMethod(t));
                writer.EmitLoad();
                argValue.EmitLoad();
                member.EmitLoadValue(il);
                argResolver.EmitLoad();
                il.EmitCall(EmitInfo.Serialize(t));
            }
        }

        // T Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver);
        // delegate T Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        static void BuildDeserialize(Type type, MetaType info, MethodBuilder method, ILGenerator il, Dictionary<MetaMember, FieldInfo> customFormatterLookup)
        {
            var argReader = new ArgumentField(il, 1);
            var argResolver = new ArgumentField(il, 2);

            var typeInfo = type.GetTypeInfo();

            // if (reader.ReadIsNull()) return null;
            {
                var elseBody = il.DefineLabel();

                argReader.EmitLoad();
                il.EmitCall(EmitInfo.JsonReader.ReadIsNull);
                il.Emit(OpCodes.Brfalse_S, elseBody);

                if (typeInfo.IsClass)
                {
                    il.Emit(OpCodes.Ldnull);
                    il.Emit(OpCodes.Ret); // return;
                }
                else
                {
                    il.Emit(OpCodes.Ldstr, "json value is null, struct is not supported");
                    il.Emit(OpCodes.Newobj, EmitInfo.InvalidOperationExceptionConstructor);
                    il.Emit(OpCodes.Throw);
                }

                il.MarkLabel(elseBody);
            }

            // read '{'
            argReader.EmitLoad();
            il.EmitCall(EmitInfo.JsonReader.ReadIsBeginObjectWithVerify);

            // make local fields
            var infoList = info.Members
                .Select(item => new DeserializeInfo
                {
                    MemberInfo = item,
                    LocalField = il.DeclareLocal(item.Type),
                })
                .ToArray();

            var countField = il.DeclareLocal(typeof(int));

            // read member loop
            {
                var automata = new AutomataDictionary();
                for (int i = 0; i < info.Members.Length; i++)
                {
                    automata.Add(info.Members[i].Name, i);
                }

                var baseBytes = il.DeclareLocal(typeof(byte[]));
                var buffer = il.DeclareLocal(typeof(byte).MakeByRefType(), true);
                var keyArraySegment = il.DeclareLocal(typeof(ArraySegment<byte>));
                var longKey = il.DeclareLocal(typeof(ulong));
                var p = il.DeclareLocal(typeof(byte*));
                var rest = il.DeclareLocal(typeof(int));

                // baseBytes = reader.GetBufferUnsafe();
                // fixed (byte* buffer = &baseBytes[0]) {
                argReader.EmitLoad();
                il.EmitCall(EmitInfo.JsonReader.GetBufferUnsafe);
                il.EmitStloc(baseBytes);

                il.EmitLdloc(baseBytes);
                il.EmitLdc_I4(0);
                il.Emit(OpCodes.Ldelema, typeof(byte));
                il.EmitStloc(buffer);

                // while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref count)) // "}", skip "," when count != 0
                var continueWhile = il.DefineLabel();
                var breakWhile = il.DefineLabel();
                var readNext = il.DefineLabel();

                il.MarkLabel(continueWhile);

                argReader.EmitLoad();
                il.EmitLdloca(countField); // ref count field(use ldloca)
                il.EmitCall(EmitInfo.JsonReader.ReadIsEndObjectWithSkipValueSeparator);
                il.Emit(OpCodes.Brtrue, breakWhile); // found '}', break

                argReader.EmitLoad();
                il.EmitCall(EmitInfo.JsonReader.ReadPropertyNameSegmentUnsafe);
                il.EmitStloc(keyArraySegment);

                // p = buffer + arraySegment.Offset
                il.EmitLdloc(buffer);
                il.Emit(OpCodes.Conv_I);
                il.EmitLdloca(keyArraySegment);
                il.EmitCall(typeof(ArraySegment<byte>).GetRuntimeProperty("Offset").GetGetMethod());
                il.Emit(OpCodes.Add);
                il.EmitStloc(p);

                // rest = arraySegment.Count
                il.EmitLdloca(keyArraySegment);
                il.EmitCall(typeof(ArraySegment<byte>).GetRuntimeProperty("Count").GetGetMethod());
                il.EmitStloc(rest);

                // if(rest == 0) goto End
                il.EmitLdloc(rest);
                il.Emit(OpCodes.Brfalse, readNext);

                //// gen automata name lookup
                automata.EmitMatch(il, p, rest, longKey, x =>
                {
                    var i = x.Value;
                    if (infoList[i].MemberInfo != null)
                    {
                        EmitDeserializeValue(il, infoList[i], customFormatterLookup, argReader, argResolver);
                        il.Emit(OpCodes.Br, continueWhile);
                    }
                    else
                    {
                        il.Emit(OpCodes.Br, readNext);
                    }
                }, () =>
                {
                    il.Emit(OpCodes.Br, readNext);
                });

                il.MarkLabel(readNext);
                argReader.EmitLoad();
                il.EmitCall(EmitInfo.JsonReader.ReadNextBlock);

                il.Emit(OpCodes.Br, continueWhile); // loop again

                il.MarkLabel(breakWhile);

                // end fixed
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Conv_U);
                il.EmitStloc(buffer);
            }

            // create result object
            var structLocal = EmitNewObject(il, type, info, infoList);

            if (info.IsStruct)
            {
                il.Emit(OpCodes.Ldloc, structLocal);
            }

            il.Emit(OpCodes.Ret);
        }

        static void EmitDeserializeValue(ILGenerator il, DeserializeInfo info, Dictionary<MetaMember, FieldInfo> customFormatterLookup, ArgumentField reader, ArgumentField argResolver)
        {
            var member = info.MemberInfo;
            var t = member.Type;
            FieldInfo customFormatter;
            if (customFormatterLookup.TryGetValue(member, out customFormatter))
            {
                throw new NotImplementedException();
                //il.Emit(OpCodes.Ldarg_0);
                //il.EmitLdfld(customFormatter);
                //il.EmitLdarg(1);
                //il.EmitLdarg(2);
                //il.EmitLdarg(3);
                //il.EmitLdarg(4);
                //il.EmitCall(customFormatter.FieldType.GetRuntimeMethod("Deserialize", new[] { typeof(byte[]), typeof(int), typeof(IFormatterResolver), refInt }));
            }
            else if (jsonPrimitiveTypes.Contains(t))
            {
                reader.EmitLoad();
                il.EmitCall(typeof(JsonReader).GetTypeInfo().GetDeclaredMethods("Read" + t.Name).OrderByDescending(x => x.GetParameters().Length).First());
            }
            else
            {
                argResolver.EmitLoad();
                il.Emit(OpCodes.Call, EmitInfo.GetFormatterWithVerify.MakeGenericMethod(t));
                reader.EmitLoad();
                argResolver.EmitLoad();
                il.EmitCall(EmitInfo.Deserialize(t));
            }

            il.EmitStloc(info.LocalField);
        }

        static LocalBuilder EmitNewObject(ILGenerator il, Type type, MetaType info, DeserializeInfo[] members)
        {
            if (info.IsClass)
            {
                foreach (var item in info.ConstructorParameters)
                {
                    var local = members.First(x => x.MemberInfo == item);
                    il.EmitLdloc(local.LocalField);
                }
                il.Emit(OpCodes.Newobj, info.BestmatchConstructor);

                foreach (var item in members.Where(x => x.MemberInfo != null && x.MemberInfo.IsWritable))
                {
                    il.Emit(OpCodes.Dup);
                    il.EmitLdloc(item.LocalField);
                    item.MemberInfo.EmitStoreValue(il);
                }

                return null;
            }
            else
            {
                var result = il.DeclareLocal(type);
                if (info.BestmatchConstructor == null)
                {
                    il.Emit(OpCodes.Ldloca, result);
                    il.Emit(OpCodes.Initobj, type);
                }
                else
                {
                    foreach (var item in info.ConstructorParameters)
                    {
                        var local = members.First(x => x.MemberInfo == item);
                        il.EmitLdloc(local.LocalField);
                    }
                    il.Emit(OpCodes.Newobj, info.BestmatchConstructor);
                    il.Emit(OpCodes.Stloc, result);
                }

                foreach (var item in members.Where(x => x.MemberInfo != null && x.MemberInfo.IsWritable))
                {
                    il.EmitLdloca(result);
                    il.EmitLdloc(item.LocalField);
                    item.MemberInfo.EmitStoreValue(il);
                }

                return result; // struct returns local result field
            }
        }

        struct DeserializeInfo
        {
            public MetaMember MemberInfo;
            public LocalBuilder LocalField;
        }

        internal static class EmitInfo
        {
            public static readonly ConstructorInfo ObjectCtor = typeof(object).GetTypeInfo().DeclaredConstructors.First(x => x.GetParameters().Length == 0);
            public static readonly MethodInfo GetFormatterWithVerify = typeof(JsonFormatterResolverExtensions).GetRuntimeMethod("GetFormatterWithVerify", new[] { typeof(IJsonFormatterResolver) });
            public static readonly MethodInfo UnsafeMemory_MemoryCopy = ExpressionUtility.GetMethodInfo((Utf8Json.JsonWriter writer, byte[] src) => UnsafeMemory.MemoryCopy(ref writer, src));
            public static readonly ConstructorInfo InvalidOperationExceptionConstructor = typeof(System.InvalidOperationException).GetTypeInfo().DeclaredConstructors.First(x => { var p = x.GetParameters(); return p.Length == 1 && p[0].ParameterType == typeof(string); });


            public static MethodInfo Serialize(Type type)
            {
                return typeof(IJsonFormatter<>).MakeGenericType(type).GetRuntimeMethod("Serialize", new[] { typeof(Utf8Json.JsonWriter).MakeByRefType(), type, typeof(IJsonFormatterResolver) });
            }

            public static MethodInfo Deserialize(Type type)
            {
                return typeof(IJsonFormatter<>).MakeGenericType(type).GetRuntimeMethod("Deserialize", new[] { typeof(Utf8Json.JsonReader).MakeByRefType(), typeof(IJsonFormatterResolver) });
            }

            internal static class JsonWriter
            {
                public static readonly MethodInfo GetEncodedPropertyNameWithBeginObject = ExpressionUtility.GetMethodInfo(() => Utf8Json.JsonWriter.GetEncodedPropertyNameWithBeginObject(default(string)));

                public static readonly MethodInfo GetEncodedPropertyNameWithPrefixValueSeparator = ExpressionUtility.GetMethodInfo(() => Utf8Json.JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator(default(string)));

                public static readonly MethodInfo GetEncodedPropertyNameWithoutQuotation = ExpressionUtility.GetMethodInfo(() => Utf8Json.JsonWriter.GetEncodedPropertyNameWithoutQuotation(default(string)));

                public static readonly MethodInfo GetEncodedPropertyName = ExpressionUtility.GetMethodInfo(() => Utf8Json.JsonWriter.GetEncodedPropertyName(default(string)));

                public static readonly MethodInfo WriteNull = ExpressionUtility.GetMethodInfo((Utf8Json.JsonWriter writer) => writer.WriteNull());
                public static readonly MethodInfo WriteEndObject = ExpressionUtility.GetMethodInfo((Utf8Json.JsonWriter writer) => writer.WriteEndObject());

                static JsonWriter()
                {
                }
            }

            internal static class JsonReader
            {
                public static readonly MethodInfo ReadIsNull = ExpressionUtility.GetMethodInfo((Utf8Json.JsonReader reader) => reader.ReadIsNull());
                public static readonly MethodInfo ReadIsBeginObjectWithVerify = ExpressionUtility.GetMethodInfo((Utf8Json.JsonReader reader) => reader.ReadIsBeginObjectWithVerify());
                public static readonly MethodInfo ReadIsEndObjectWithSkipValueSeparator = ExpressionUtility.GetMethodInfo((Utf8Json.JsonReader reader, int count) => reader.ReadIsEndObjectWithSkipValueSeparator(ref count));
                public static readonly MethodInfo ReadPropertyNameSegmentUnsafe = ExpressionUtility.GetMethodInfo((Utf8Json.JsonReader reader) => reader.ReadPropertyNameSegmentUnescaped());
                public static readonly MethodInfo ReadNextBlock = ExpressionUtility.GetMethodInfo((Utf8Json.JsonReader reader) => reader.ReadNextBlock());
                public static readonly MethodInfo GetBufferUnsafe = ExpressionUtility.GetMethodInfo((Utf8Json.JsonReader reader) => reader.GetBufferUnsafe());
                public static readonly MethodInfo GetCurrentOffsetUnsafe = ExpressionUtility.GetMethodInfo((Utf8Json.JsonReader reader) => reader.GetCurrentOffsetUnsafe());

                static JsonReader()
                {
                }
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
}