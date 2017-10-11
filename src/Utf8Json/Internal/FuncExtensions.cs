using System;

namespace Utf8Json.Internal
{
    internal static class FuncExtensions
    {
        // hack of avoid closure allocation(() => value).
        public static Func<T> AsFunc<T>(this T value)
        {
            return value.ReturnBox<T>;
        }

        public static Func<T> AsFuncFast<T>(this T value) where T : class
        {
            return value.Return<T>;
        }

        static T Return<T>(this T value)
        {
            return value;
        }

        static T ReturnBox<T>(this object value)
        {
            return (T)value;
        }
    }
}
