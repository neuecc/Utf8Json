using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Utf8Json
{
    public interface IJsonFormatterResolver
    {
        IJsonFormatter<T> GetFormatter<T>();
    }

    public static class JsonFormatterResolverExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
                ThrowFormatterIsNull(typeof(T).FullName, resolver);
            }

            return formatter;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowFormatterIsNull(string name, IJsonFormatterResolver resolver)
        {
            throw new FormatterNotRegisteredException(name + " is not registered in this resolver. resolver:" + resolver.GetType().Name);

        }

        public static object GetFormatterDynamic(this IJsonFormatterResolver resolver, Type type)
        {
            var methodInfo = typeof(IJsonFormatterResolver).GetRuntimeMethod("GetFormatter", Type.EmptyTypes);

            var formatter = methodInfo.MakeGenericMethod(type).Invoke(resolver, null);
            return formatter;
        }
    }

    public class FormatterNotRegisteredException : Exception
    {
        public FormatterNotRegisteredException(string message) : base(message)
        {
        }
    }
}