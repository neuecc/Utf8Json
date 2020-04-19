using System;
using System.Collections.Generic;
using System.Linq;

namespace Utf8Json.Formatters
{
	public class PolymorphicFormatter<T> : IJsonFormatter<T>
	{
		private readonly IReadOnlyDictionary<string, Type> _derivedTypesByName =
			typeof(T).Assembly.GetTypes().Where(t => typeof(T).IsAssignableFrom(t)).ToDictionary(t => t.FullName);

		public void Serialize(ref JsonWriter writer, T value, IJsonFormatterResolver formatterResolver)
		{
			if (value == null)
			{
				writer.WriteNull();
			}
			else
			{
				var instanceType = value.GetType();
				if (!_derivedTypesByName.ContainsKey(instanceType.FullName))
				{
					throw new InvalidOperationException("Trying to serialize unexpected type {0} derived from {1}");
				}

				writer.WriteBeginObject();
				
				writer.WritePropertyName("$type");
				writer.WriteString(instanceType.FullName);
				writer.WriteValueSeparator();
				
				var innerFormatter = formatterResolver.GetFormatterDynamic(instanceType);
				var specificTypeFormatter = (IJsonFormatter) formatterResolver.GetFormatterDynamic(instanceType);
				var serializeMethod = specificTypeFormatter.GetType().GetMethod("Serialize");
				
				writer.WritePropertyName("$value");
				var parameters = new object[] {writer, value, formatterResolver};
				serializeMethod.Invoke(innerFormatter, parameters);
				writer = (JsonWriter) parameters[0]; // pass back updated JsonWriter, which is a struct

				writer.WriteEndObject();
			}
		}

		public T Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
		{
			if (reader.ReadIsNull())
			{
				return default(T);
			}

			reader.ReadIsBeginObjectWithVerify();
			if (reader.ReadPropertyName() != "$type")
			{
				throw new InvalidOperationException(string.Format("Could not find $type property for {0} message", typeof(T).Name));
			}
			var typeName = reader.ReadString();
			
			// after read type, need to create dynamic formatter for the type
			var instanceType = _derivedTypesByName[typeName];
			var specificTypeFormatter = (IJsonFormatter) formatterResolver.GetFormatterDynamic(instanceType);
			var deserializeMethod = specificTypeFormatter.GetType().GetMethod("Deserialize");

			// then deserialize the whole thing
			reader.ReadIsValueSeparatorWithVerify();
			if (reader.ReadPropertyName() != "$value")
			{
				throw new InvalidOperationException(string.Format("Could not find $value property for {0} message", typeof(T).Name));
			}
			var parameters = new object[] {reader, formatterResolver};
			var result = (T) deserializeMethod.Invoke(specificTypeFormatter, parameters);
			reader = (JsonReader) parameters[0]; // pass back updated JsonReader, which is a struct
			reader.ReadIsEndObjectWithVerify();
			return result;
		}
	}
}