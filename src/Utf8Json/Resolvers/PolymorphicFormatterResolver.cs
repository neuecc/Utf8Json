using System.Reflection;
using Utf8Json.Formatters;

namespace Utf8Json.Resolvers
{
	/// <summary>
	/// Get polymorphic formatter
	/// </summary>
	public sealed class PolymorphicFormatterResolver : IJsonFormatterResolver
	{
		public static IJsonFormatterResolver Instance = new PolymorphicFormatterResolver();

		PolymorphicFormatterResolver()
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
#if (UNITY_METRO || UNITY_WSA) && !NETFX_CORE
                var attr = (PolymorphicFormatterAttribute)typeof(T).GetCustomAttributes(typeof(PolymorphicFormatterAttribute), true).FirstOrDefault();
#else
				var attr = typeof(T).GetTypeInfo().GetCustomAttribute<PolymorphicFormatterAttribute>();
#endif
				if (attr == null)
				{
					return;
				}

				formatter = new PolymorphicFormatter<T>();
			}
		}
	}
}