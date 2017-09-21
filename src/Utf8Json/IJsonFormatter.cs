namespace Utf8Json
{
    public interface IJsonFormatter<T>
    {
        void Serialize(ref JsonWriter writer, ref T value, IJsonFormatterResolver formatterResolver);
        T Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver);
    }
}