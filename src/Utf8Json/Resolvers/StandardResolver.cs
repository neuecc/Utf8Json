using System;
using System.Collections.Generic;
using System.Text;

namespace Utf8Json.Resolvers
{
    public sealed class StandardResolver : IJsonFormatterResolver
    {
        public static readonly IJsonFormatterResolver Instance = new StandardResolver();

        StandardResolver()
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
                if (typeof(T) == typeof(object))
                {
                    // TODO:fallback
                    //#if NETSTANDARD1_4
                    //                    formatter = (IMessagePackFormatter<T>)ObjectFallbackFormatter;
                    //#else
                    //                    formatter = PrimitiveObjectResolver.Instance.GetFormatter<T>();
                    //#endif
                }
                else
                {
                    formatter = StandardResolverCore.Instance.GetFormatter<T>();
                }
            }
        }

        sealed class StandardResolverCore : IJsonFormatterResolver
        {
            public static readonly IJsonFormatterResolver Instance = new StandardResolverCore();

            static readonly IJsonFormatterResolver[] resolvers = new[]
            {
                // TODO:more resolvers
                BuiltinResolver.Instance, // Try Builtin
                DynamicObjectResolver.Instance, // Try Object
            };

            StandardResolverCore()
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
                    foreach (var item in resolvers)
                    {
                        var f = item.GetFormatter<T>();
                        if (f != null)
                        {
                            formatter = f;
                            return;
                        }
                    }
                }
            }
        }
    }
}