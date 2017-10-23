﻿using Utf8Json.Formatters;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;

namespace Utf8Json.ImmutableCollection
{
    public class ImmutableCollectionResolver : IJsonFormatterResolver
    {
        public static IJsonFormatterResolver Instance = new ImmutableCollectionResolver();

        ImmutableCollectionResolver()
        {

        }

        public IJsonFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.formatter;
        }

        static class FormatterCache<T>
        {
            public static readonly IJsonFormatter<T> formatter;

            static FormatterCache()
            {
                formatter = (IJsonFormatter<T>)ImmutableCollectionGetFormatterHelper.GetFormatter(typeof(T));
            }
        }
    }

    internal static class ImmutableCollectionGetFormatterHelper
    {
        static readonly Dictionary<Type, Type> formatterMap = new Dictionary<Type, Type>
        {
              {typeof(ImmutableArray<>), typeof(ImmutableArrayFormatter<>)},
              {typeof(ImmutableList<>), typeof(ImmutableListFormatter<>)},
              {typeof(ImmutableDictionary<,>), typeof(ImmutableDictionaryFormatter<,>)},
              {typeof(ImmutableHashSet<>), typeof(ImmutableHashSetFormatter<>)},
              {typeof(ImmutableSortedDictionary<,>), typeof(ImmutableSortedDictionaryFormatter<,>)},
              {typeof(ImmutableSortedSet<>), typeof(ImmutableSortedSetFormatter<>)},
              {typeof(ImmutableQueue<>), typeof(ImmutableQueueFormatter<>)},
              {typeof(ImmutableStack<>), typeof(ImmutableStackFormatter<>)},
              {typeof(IImmutableList<>), typeof(InterfaceImmutableListFormatter<>)},
              {typeof(IImmutableDictionary<,>), typeof(InterfaceImmutableDictionaryFormatter<,>)},
              {typeof(IImmutableQueue<>), typeof(InterfaceImmutableQueueFormatter<>)},
              {typeof(IImmutableSet<>), typeof(InterfaceImmutableSetFormatter<>)},
              {typeof(IImmutableStack<>), typeof(InterfaceImmutableStackFormatter<>)},
        };

        internal static object GetFormatter(Type t)
        {
            var ti = t.GetTypeInfo();

            if (ti.IsGenericType)
            {
                var genericType = ti.GetGenericTypeDefinition();
                var genericTypeInfo = genericType.GetTypeInfo();
                var isNullable = genericTypeInfo.IsNullable();
                var nullableElementType = isNullable ? ti.GenericTypeArguments[0] : null;

                if (formatterMap.TryGetValue(genericType, out var formatterType))
                {
                    return CreateInstance(formatterType, ti.GenericTypeArguments);
                }
                if (isNullable && nullableElementType.IsConstructedGenericType && nullableElementType.GetGenericTypeDefinition() == typeof(ImmutableArray<>))
                {
                    return CreateInstance(typeof(NullableFormatter<>), new[] { nullableElementType });
                }
            }

            return null;
        }

        static object CreateInstance(Type genericType, Type[] genericTypeArguments, params object[] arguments)
        {
            return Activator.CreateInstance(genericType.MakeGenericType(genericTypeArguments), arguments);
        }
    }

    internal static class ReflectionExtensions
    {
        public static bool IsNullable(this System.Reflection.TypeInfo type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(System.Nullable<>);
        }
    }
}