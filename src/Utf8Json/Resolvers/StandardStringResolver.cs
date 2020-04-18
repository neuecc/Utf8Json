using System;
using System.Reflection;
using Utf8Json.Formatters;
using Utf8Json.Internal;

namespace Utf8Json.Resolvers
{
    internal static class StandardStringResolver
    {
        /// <summary>Serialize as is.</summary>
        public static readonly IJsonFormatterResolver Original = StringDefaultResolver.Instance;

        /// <summary>Serialize as CamelCase.</summary>
        public static readonly IJsonFormatterResolver CamelCase = StringCamelCaseValueResolver.Instance;

        /// <summary>Serialize as SnakeCase.</summary>
        public static readonly IJsonFormatterResolver SnakeCase =
            StringCamelCaseValueResolver.StringSnakeCaseValueResolver.Instance;
    }

    internal sealed class StringDefaultResolver : IJsonFormatterResolver
    {
        public static readonly IJsonFormatterResolver Instance = new StringDefaultResolver();

        StringDefaultResolver()
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

                if (ti.IsNullable())
                {
                    // build underlying type and use wrapped formatter.
                    ti = ti.GenericTypeArguments[0].GetTypeInfo();
                    if (!ti.IsEnum)
                    {
                        return;
                    }

                    var innerFormatter = Instance.GetFormatterDynamic(ti.AsType());
                    if (innerFormatter == null)
                    {
                        return;
                    }

                    formatter = (IJsonFormatter<T>) Activator.CreateInstance(
                        typeof(StaticNullableFormatter<>).MakeGenericType(ti.AsType()), innerFormatter);
                    return;
                }

                if (ti == typeof(string))
                {
                    formatter = (IJsonFormatter<T>) (object) NullableStringFormatter.Default;
                }
            }
        }
    }

    internal sealed class StringCamelCaseValueResolver : IJsonFormatterResolver
    {
        public static readonly IJsonFormatterResolver Instance = new StringCamelCaseValueResolver();

        StringCamelCaseValueResolver()
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

                if (ti.IsNullable())
                {
                    // build underlying type and use wrapped formatter.
                    ti = ti.GenericTypeArguments[0].GetTypeInfo();
                    if (!ti.IsEnum)
                    {
                        return;
                    }

                    var innerFormatter = Instance.GetFormatterDynamic(ti.AsType());
                    if (innerFormatter == null)
                    {
                        return;
                    }

                    formatter = (IJsonFormatter<T>) Activator.CreateInstance(
                        typeof(StaticNullableFormatter<>).MakeGenericType(ti.AsType()), innerFormatter);
                    return;
                }

                if (ti == typeof(string))
                {
                    formatter = (IJsonFormatter<T>) (object) new NullableStringFormatter(StringMutator.ToCamelCase);
                }
            }
        }

        internal sealed class StringSnakeCaseValueResolver : IJsonFormatterResolver
        {
            public static readonly IJsonFormatterResolver Instance = new StringSnakeCaseValueResolver();

            StringSnakeCaseValueResolver()
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

                    if (ti.IsNullable())
                    {
                        // build underlying type and use wrapped formatter.
                        ti = ti.GenericTypeArguments[0].GetTypeInfo();
                        if (!ti.IsEnum)
                        {
                            return;
                        }

                        var innerFormatter = Instance.GetFormatterDynamic(ti.AsType());
                        if (innerFormatter == null)
                        {
                            return;
                        }

                        formatter = (IJsonFormatter<T>) Activator.CreateInstance(
                            typeof(StaticNullableFormatter<>).MakeGenericType(ti.AsType()), innerFormatter);
                        return;
                    }

                    if (ti == typeof(string))
                    {
                        formatter = (IJsonFormatter<T>) (object) new NullableStringFormatter(StringMutator.ToSnakeCase);
                    }
                }
            }
        }
    }
}