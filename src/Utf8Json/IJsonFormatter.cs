namespace Utf8Json
{
    public interface IJsonFormatter<T>
    {
        void Serialize(ref JsonWriter writer, ref T value, IFormatterResolver formatterResolver);
        T Deserialize(ref JsonReader reader, IFormatterResolver formatterResolver);
    }
}