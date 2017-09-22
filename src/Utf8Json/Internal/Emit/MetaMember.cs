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

        public bool IsProperty { get { return PropertyInfo != null; } }
        public bool IsField { get { return FieldInfo != null; } }
        public bool IsWritable { get; private set; }
        public bool IsReadable { get; private set; }
        public Type Type { get { return IsField ? FieldInfo.FieldType : PropertyInfo.PropertyType; } }
        public FieldInfo FieldInfo { get; private set; }
        public PropertyInfo PropertyInfo { get; private set; }

        public MetaMember(FieldInfo info, string name)
        {
            this.Name = name; ;
            this.FieldInfo = info;
            this.IsReadable = info.IsPublic;
            this.IsWritable = info.IsPublic && !info.IsInitOnly;
        }

        public MetaMember(PropertyInfo info, string name)
        {
            this.Name = name;
            this.PropertyInfo = info;
            this.IsReadable = (info.GetGetMethod() != null) && info.GetGetMethod().IsPublic && !info.GetGetMethod().IsStatic;
            this.IsWritable = (info.GetSetMethod() != null) && info.GetSetMethod().IsPublic && !info.GetSetMethod().IsStatic;
        }

        public bool IsValueType
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
                il.EmitCall(PropertyInfo.GetGetMethod());
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
                il.EmitCall(PropertyInfo.GetSetMethod());
            }
            else
            {
                il.Emit(OpCodes.Stfld, FieldInfo);
            }
        }
    }
}
