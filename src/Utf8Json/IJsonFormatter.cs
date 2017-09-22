namespace Utf8Json
{
    public delegate void JsonSerializeAction<T>(ref JsonWriter writer, T value, IJsonFormatterResolver resolver);
    public delegate T JsonDeserializeFunc<T>(ref JsonReader reader,  IJsonFormatterResolver resolver);

    public interface IJsonFormatter<T>
    {
        void Serialize(ref JsonWriter writer, T value, IJsonFormatterResolver formatterResolver);
        T Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver);
    }
}