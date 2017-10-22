﻿using System.Reflection;
using System.Runtime.CompilerServices;

namespace Utf8Json.Internal
{
    internal static class ReflectionExtensions
    {
        public static bool IsNullable(this System.Reflection.TypeInfo type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Nullable<>);
        }

        public static bool IsPublic(this System.Reflection.TypeInfo type)
        {
            return type.IsPublic;
        }

        public static bool IsAnonymous(this System.Reflection.TypeInfo type)
        {
            return type.GetCustomAttribute<CompilerGeneratedAttribute>() != null
                && type.Name.Contains("AnonymousType")
                && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
                && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }

#if NETSTANDARD

        public static bool IsConstructedGenericType(this System.Reflection.TypeInfo type)
        {
            return type.AsType().IsConstructedGenericType;
        }

        public static MethodInfo GetGetMethod(this PropertyInfo propInfo)
        {
            return propInfo.GetMethod;
        }

        public static MethodInfo GetSetMethod(this PropertyInfo propInfo)
        {
            return propInfo.SetMethod;
        }

#endif
    }
}