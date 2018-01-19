using System;

namespace Utf8Json.Formatters
{
    public class ExceptionFormatter<TException> : IJsonFormatter<TException>
        where TException : Exception
    {
        static readonly byte[] classNamePropertyName;
        static readonly byte[] messagePropertyName;
        static readonly byte[] sourcePropertyName;
        static readonly byte[] stackTracePropertyName;
        static readonly byte[] innerExceptionPropertyName;

        static ExceptionFormatter()
        {
            classNamePropertyName = JsonWriter.GetEncodedPropertyNameWithBeginObject("ClassName");
            messagePropertyName = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Message");
            sourcePropertyName = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Source");
            stackTracePropertyName = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("StackTrace");
            innerExceptionPropertyName = JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("InnerException");
        }

        public void Serialize(ref JsonWriter writer, TException value, IJsonFormatterResolver formatterResolver)
        {
            writer.WriteRaw(classNamePropertyName);
            writer.WriteString(value.GetType().FullName);

            writer.WriteRaw(messagePropertyName);
            writer.WriteString(value.Message);

            writer.WriteRaw(sourcePropertyName);
            writer.WriteString(value.Source);

            writer.WriteRaw(stackTracePropertyName);
            writer.WriteString(value.StackTrace);

            if (value.InnerException != null)
            {
                writer.WriteRaw(innerExceptionPropertyName);
                formatterResolver.GetFormatter<Exception>().Serialize(ref writer, value.InnerException, formatterResolver);
            }

            writer.WriteEndObject();
        }

        public TException Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            throw new NotSupportedException("Exception formatter does not support deserialize.");
        }
    }
}