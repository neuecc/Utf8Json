using System;
using System.Reflection;

namespace Utf8Json
{
    public interface IJsonFormatterResolver
    {
        IJsonFormatter<T> GetFormatter<T>();
    }

    public static class JsonFormatterResolverExtensions
    {
        public static IJsonFormatter<T> GetFormatterWithVerify<T>(this IJsonFormatterResolver resolver)
        {
            IJsonFormatter<T> formatter;
            try
            {
                formatter = resolver.GetFormatter<T>();
            }
            catch (TypeInitializationException ex)
            {
                Exception inner = ex;
                while (inner.InnerException != null)
                {
                    inner = inner.InnerException;
                }

                throw inner;
            }

            if (formatter == null)
            {
                throw new FormatterNotRegisteredException(typeof(T).FullName + " is not registered in this resolver. resolver:" + resolver.GetType().Name);
            }

            return formatter;
        }

        public static object GetFormatterDynamic(this IJsonFormatterResolver resolver, Type type)
        {
            var methodInfo = typeof(IJsonFormatterResolver).GetRuntimeMethod("GetFormatter", Type.EmptyTypes);

            var formatter = methodInfo.MakeGenericMethod(type).Invoke(resolver, null);
            return formatter;
        }

        public static void DeserializeToWithFallbackReplace<T>(this IJsonFormatterResolver formatterResolver, ref T value, ref JsonReader reader)
        {
            var formatter = formatterResolver.GetFormatterWithVerify<T>();
            var overwriteFormatter = formatter as IOverwriteJsonFormatter<T>;
            if (overwriteFormatter != null)
            {
                overwriteFormatter.DeserializeTo(ref value, ref reader, formatterResolver);
            }
            else
            {
                // deserialize new value and replace with it.
                value = formatter.Deserialize(ref reader, formatterResolver);
            }
        }
    }

    public class FormatterNotRegisteredException : Exception
    {
        public FormatterNotRegisteredException(string message) : base(message)
        {
        }
    }
}