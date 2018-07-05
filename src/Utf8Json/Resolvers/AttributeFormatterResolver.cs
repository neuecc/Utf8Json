using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using Utf8Json.Formatters;
using Utf8Json.Internal;

namespace Utf8Json.Resolvers
{
    /// <summary>
    /// Get formatter from [JsonFormatter] attribute.
    /// </summary>
    public sealed class AttributeFormatterResolver : IJsonFormatterResolver
    {
        public static IJsonFormatterResolver Instance = new AttributeFormatterResolver();

        AttributeFormatterResolver()
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
                var ti = typeof(T).GetTypeInfo();
                var genericTypeInfo = ti.GetTypeInfo();
                var isNullable = genericTypeInfo.IsNullable();
                var elementType = isNullable ? ti.GenericTypeArguments[0] : typeof(T);

#if (UNITY_METRO || UNITY_WSA) && !NETFX_CORE
                var attr = (JsonFormatterAttribute)elementType.GetCustomAttributes(typeof(JsonFormatterAttribute), true).FirstOrDefault();
#else
                var attr = elementType.GetTypeInfo().GetCustomAttribute<JsonFormatterAttribute>();
#endif
                if (attr == null)
                {
                    return;
                }

                try
                {
                    object fmt;
                    if (attr.FormatterType.IsGenericType && !attr.FormatterType.GetTypeInfo().IsConstructedGenericType())
                    {
                        var t = attr.FormatterType.MakeGenericType(typeof(T)); // use T self
                        fmt = Activator.CreateInstance(t, attr.Arguments);
                    }
                    else
                    {
                        fmt = Activator.CreateInstance(attr.FormatterType, attr.Arguments);
                    }

                    if (isNullable)
                    {
                        formatter = (IJsonFormatter<T>)Activator.CreateInstance(typeof(StaticNullableFormatter<>).MakeGenericType(elementType), fmt);
                    }
                    else
                    {
                        formatter = (IJsonFormatter<T>)fmt;
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Can not create formatter from JsonFormatterAttribute, check the target formatter is public and has constructor with right argument. FormatterType:" + attr.FormatterType.Name, ex);
                }
            }
        }
    }
}
