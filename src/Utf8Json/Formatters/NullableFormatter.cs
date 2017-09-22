
namespace Utf8Json.Formatters
{
    public sealed class NullableFormatter<T> : IJsonFormatter<T?>
        where T : struct
    {
        public void Serialize(ref JsonWriter writer, T? value, IJsonFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                formatterResolver.GetFormatterWithVerify<T>().Serialize(ref writer, value.Value, formatterResolver);
            }
        }

        public T? Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull())
            {
                return null;
            }
            else
            {
                return formatterResolver.GetFormatterWithVerify<T>().Deserialize(ref reader, formatterResolver);
            }
        }
    }

    public sealed class StaticNullableFormatter<T> : IJsonFormatter<T?>
        where T : struct
    {
        readonly IJsonFormatter<T> underlyingFormatter;

        public StaticNullableFormatter(IJsonFormatter<T> underlyingFormatter)
        {
            this.underlyingFormatter = underlyingFormatter;
        }

        public void Serialize(ref JsonWriter writer, T? value, IJsonFormatterResolver formatterResolver)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                underlyingFormatter.Serialize(ref writer, value.Value, formatterResolver);
            }
        }

        public T? Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull())
            {
                return null;
            }
            else
            {
                return underlyingFormatter.Deserialize(ref reader, formatterResolver);
            }
        }
    }
}
