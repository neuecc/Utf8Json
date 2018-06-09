using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace Utf8Json.Internal.Emit
{
    internal class MetaType
    {
        public Type Type { get; private set; }
        public bool IsClass { get; private set; }
        public bool IsStruct { get { return !IsClass; } }
        public bool IsConcreteClass { get; private set; }

        public ConstructorInfo BestmatchConstructor { get; internal set; }
        public MetaMember[] ConstructorParameters { get; internal set; }
        public MetaMember[] Members { get; internal set; }

        public MetaType(Type type, Func<string, string> nameMutetor, bool allowPrivate)
        {
            var ti = type.GetTypeInfo();
            var isClass = ti.IsClass || ti.IsInterface || ti.IsAbstract;

            this.Type = type;
            var stringMembers = new Dictionary<string, MetaMember>();

            {
                foreach (var item in type.GetAllProperties())
                {
                    if (item.GetIndexParameters().Length > 0) continue; // skip indexer
                    if (item.GetCustomAttribute<IgnoreDataMemberAttribute>(true) != null) continue;

                    var dm = item.GetCustomAttribute<DataMemberAttribute>(true);
                    var name = (dm != null && dm.Name != null) ? dm.Name : nameMutetor(item.Name);

                    var member = new MetaMember(item, name, allowPrivate);
                    if (!member.IsReadable && !member.IsWritable) continue;

                    if (stringMembers.ContainsKey(member.Name))
                    {
                        throw new InvalidOperationException("same (custom)name is in type. Type:" + type.Name + " Name:" + member.Name);
                    }
                    stringMembers.Add(member.Name, member);
                }
                foreach (var item in type.GetAllFields())
                {
                    if (item.GetCustomAttribute<IgnoreDataMemberAttribute>(true) != null) continue;
                    if (item.GetCustomAttribute<System.Runtime.CompilerServices.CompilerGeneratedAttribute>(true) != null) continue;
                    if (item.IsStatic) continue;
                    if (item.Name.StartsWith("<")) continue; // compiler generated field(anonymous type, etc...)

                    var dm = item.GetCustomAttribute<DataMemberAttribute>(true);
                    var name = (dm != null && dm.Name != null) ? dm.Name : nameMutetor(item.Name);

                    var member = new MetaMember(item, name, allowPrivate);
                    if (!member.IsReadable && !member.IsWritable) continue;

                    if (stringMembers.ContainsKey(member.Name))
                    {
                        throw new InvalidOperationException("same (custom)name is in type. Type:" + type.Name + " Name:" + member.Name);
                    }
                    stringMembers.Add(member.Name, member);
                }
            }

            // GetConstructor
            var ctor = ti.DeclaredConstructors.Where(x => x.IsPublic)
                .SingleOrDefault(x => x.GetCustomAttribute<SerializationConstructorAttribute>(false) != null);
            var constructorParameters = new List<MetaMember>();
            {
                IEnumerator<ConstructorInfo> ctorEnumerator = null;
                if (ctor == null)
                {
                    // descending.
                    ctorEnumerator = ti.DeclaredConstructors.Where(x => x.IsPublic).OrderByDescending(x => x.GetParameters().Length).GetEnumerator();
                    if (ctorEnumerator.MoveNext())
                    {
                        ctor = ctorEnumerator.Current;
                    }
                }

                if (ctor != null)
                {
                    var constructorLookupDictionary = stringMembers.ToLookup(x => x.Key, x => x, StringComparer.OrdinalIgnoreCase);
                    do
                    {
                        constructorParameters.Clear();
                        var ctorParamIndex = 0;
                        foreach (var item in ctor.GetParameters())
                        {
                            MetaMember paramMember;

                            var hasKey = constructorLookupDictionary[item.Name];
                            var len = hasKey.Count();
                            if (len != 0)
                            {
                                if (len != 1)
                                {
                                    if (ctorEnumerator != null)
                                    {
                                        ctor = null;
                                        continue;
                                    }
                                    else
                                    {
                                        throw new InvalidOperationException("duplicate matched constructor parameter name:" + type.FullName + " parameterName:" + item.Name + " paramterType:" + item.ParameterType.Name);
                                    }
                                }

                                paramMember = hasKey.First().Value;
                                if (item.ParameterType == paramMember.Type && paramMember.IsReadable)
                                {
                                    constructorParameters.Add(paramMember);
                                }
                                else
                                {
                                    ctor = null;
                                    continue;
                                }
                            }
                            else
                            {
                                ctor = null;
                                continue;
                            }
                            ctorParamIndex++;
                        }
                    } while (TryGetNextConstructor(ctorEnumerator, ref ctor));
                }
            }

            this.IsClass = isClass;
            this.IsConcreteClass = isClass && !(ti.IsAbstract || ti.IsInterface);
            this.BestmatchConstructor = ctor;
            this.ConstructorParameters = constructorParameters.ToArray();
            this.Members = stringMembers.Values.ToArray();
        }

        static bool TryGetNextConstructor(IEnumerator<ConstructorInfo> ctorEnumerator, ref ConstructorInfo ctor)
        {
            if (ctorEnumerator == null || ctor != null)
            {
                return false;
            }

            if (ctorEnumerator.MoveNext())
            {
                ctor = ctorEnumerator.Current;
                return true;
            }
            else
            {
                ctor = null;
                return false;
            }
        }
    }
}
