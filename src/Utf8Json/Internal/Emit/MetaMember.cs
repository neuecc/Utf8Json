using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Runtime.Serialization;

namespace Utf8Json.Internal.Emit
{
    internal class MetaMember
    {
        public string Name { get; private set; }
        public string MemberName { get; private set; }

        public bool IsProperty { get { return PropertyInfo != null; } }
        public bool IsField { get { return FieldInfo != null; } }
        public bool IsWritable { get; private set; }
        public bool IsReadable { get; private set; }
        public Type Type { get { return IsField ? FieldInfo.FieldType : PropertyInfo.PropertyType; } }
        public FieldInfo FieldInfo { get; private set; }
        public PropertyInfo PropertyInfo { get; private set; }
        public MethodInfo ShouldSerializeMethodInfo { get; private set; }

        MethodInfo getMethod;
        MethodInfo setMethod;

        public MetaMember(FieldInfo info, string name, bool allowPrivate)
        {
            this.Name = name;
            this.MemberName = info.Name;
            this.FieldInfo = info;
            this.IsReadable = allowPrivate || info.IsPublic;
            this.IsWritable = allowPrivate || (info.IsPublic && !info.IsInitOnly);
            this.ShouldSerializeMethodInfo = GetShouldSerialize(info);
        }

        public MetaMember(PropertyInfo info, string name, bool allowPrivate)
        {
            this.getMethod = info.GetGetMethod(true);
            this.setMethod = info.GetSetMethod(true);

            this.Name = name;
            this.MemberName = info.Name;
            this.PropertyInfo = info;
            this.IsReadable = (getMethod != null) && (allowPrivate || getMethod.IsPublic) && !getMethod.IsStatic;
            this.IsWritable = (setMethod != null) && (allowPrivate || setMethod.IsPublic) && !setMethod.IsStatic;
            this.ShouldSerializeMethodInfo = GetShouldSerialize(info);
        }

        static MethodInfo GetShouldSerialize(MemberInfo info)
        {
            var shouldSerialize = "ShouldSerialize" + info.Name;

            // public only
            return info.DeclaringType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.Name == shouldSerialize && x.ReturnType == typeof(bool) && x.GetParameters().Length == 0)
                .FirstOrDefault();
        }

        public bool IsDeclaredIsValueType
        {
            get
            {
                var mi = IsProperty ? (MemberInfo)PropertyInfo : FieldInfo;
                return mi.DeclaringType.GetTypeInfo().IsValueType;
            }
        }

        public T GetCustomAttribute<T>(bool inherit) where T : Attribute
        {
            if (IsProperty)
            {
                return PropertyInfo.GetCustomAttribute<T>(inherit);
            }
            else
            {
                return FieldInfo.GetCustomAttribute<T>(inherit);
            }
        }

        public void EmitLoadValue(ILGenerator il)
        {
            if (IsProperty)
            {
                il.EmitCall(getMethod);
            }
            else
            {
                il.Emit(OpCodes.Ldfld, FieldInfo);
            }
        }

        public void EmitStoreValue(ILGenerator il)
        {
            if (IsProperty)
            {
                il.EmitCall(setMethod);
            }
            else
            {
                il.Emit(OpCodes.Stfld, FieldInfo);
            }
        }
    }
}
