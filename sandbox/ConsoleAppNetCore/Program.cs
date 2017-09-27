using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using Utf8Json;
using Utf8Json.Formatters;
using Utf8Json.ImmutableCollection;
using Utf8Json.Resolvers;

namespace ConsoleAppNetCore
{
    public class CustomPoint
    {
        public readonly int X;
        public readonly int Y;

        public CustomPoint(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        // used this constructor.
        [SerializationConstructor]
        public CustomPoint(int x)
        {
            this.X = x;
        }
    }

    public enum MyEnum
    {
        Fruit, Orange, Grape
    }

    public class Sample
    {
        public Sample Child { get; set; }
    }


    class Program
    {
        static void Main(string[] args)
        {

            var dto = DateTime.UtcNow;
            var serialized = Utf8Json.JsonSerializer.ToJsonString(dto);
            var deSerialized = Utf8Json.JsonSerializer.Deserialize<DateTime>(serialized);
            var serialized2 = Utf8Json.JsonSerializer.ToJsonString(deSerialized);

            //serialized2.Is(serialized);
            Console.WriteLine(serialized);
            Console.WriteLine(serialized2);



        }
    }

    public class Person
    {
        public int Age { get; set; }
        public string Name { get; set; }

        [JsonFormatter(typeof(DateTimeFormatter), "yyyy-MM-dd")]
        public DateTime Birth { get; set; }
    }


    public class FileInfoFormatter<T> : IJsonFormatter<FileInfo>
    {
        public void Serialize(ref JsonWriter writer, FileInfo value, IJsonFormatterResolver formatterResolver)
        {
            if (value == null) { writer.WriteNull(); return; }

            // if target type is primitive, you can also use writer.Write***.
            formatterResolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.FullName, formatterResolver);
        }

        public FileInfo Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
        {
            if (reader.ReadIsNull()) return null;

            // if target type is primitive, you can also use reader.Read***.
            var path = formatterResolver.GetFormatterWithVerify<string>().Deserialize(ref reader, formatterResolver);
            return new FileInfo(path);
        }
    }


    // if serializing, choosed CustomObjectFormatter.
    [JsonFormatter(typeof(CustomObjectFormatter))]
    public class CustomObject
    {
        string internalId;

        public CustomObject()
        {
            this.internalId = Guid.NewGuid().ToString();
        }

        // serialize/deserialize internal field.
        class CustomObjectFormatter : IJsonFormatter<CustomObject>
        {
            public void Serialize(ref JsonWriter writer, CustomObject value, IJsonFormatterResolver formatterResolver)
            {
                formatterResolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.internalId, formatterResolver);
            }

            public CustomObject Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
            {
                var id = formatterResolver.GetFormatterWithVerify<string>().Deserialize(ref reader, formatterResolver);
                return new CustomObject { internalId = id };
            }
        }
    }



    // create custom composite resolver per project is recommended way.
    // let's try to copy and paste:)
    public class ProjectDefaultResolver : IJsonFormatterResolver
    {
        public static IJsonFormatterResolver Instance = new ProjectDefaultResolver();

        // configure your resolver and formatters.
        static IJsonFormatter[] formatters = new IJsonFormatter[]{
        new DateTimeFormatter("yyyy-MM-dd HH:mm:ss")
    };

        static readonly IJsonFormatterResolver[] resolvers = new[]
        {
        ImmutableCollectionResolver.Instance,
        EnumResolver.UnderlyingValue,
        StandardResolver.AllowPrivateExcludeNullSnakeCase
    };

        ProjectDefaultResolver()
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
                foreach (var item in formatters)
                {
                    foreach (var implInterface in item.GetType().GetTypeInfo().ImplementedInterfaces)
                    {
                        var ti = implInterface.GetTypeInfo();
                        if (ti.IsGenericType && ti.GenericTypeArguments[0] == typeof(T))
                        {
                            formatter = (IJsonFormatter<T>)item;
                            return;
                        }
                    }
                }

                foreach (var item in resolvers)
                {
                    var f = item.GetFormatter<T>();
                    if (f != null)
                    {
                        formatter = f;
                        return;
                    }
                }
            }
        }
    }
}
