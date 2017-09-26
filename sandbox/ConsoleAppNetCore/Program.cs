using System;
using System.Collections.Generic;
using System.IO;
using Utf8Json;
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



    class Program
    {
        static void Main(string[] args)
        {
            CompositeResolver.RegisterAndSetAsDefault(new[]{
                EnumResolver.UnderlyingValue,
                StandardResolver.Default
            });

            var dict = new Dictionary<MyEnum, int>();
            dict.Add(MyEnum.Fruit, 100);
            dict.Add(MyEnum.Grape, 30000);
            dict.Add(MyEnum.Orange, 99900);

            var bin = JsonSerializer.Serialize(dict);


            var foo = JsonSerializer.Deserialize<Dictionary<MyEnum, int>>(bin);
        }
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
}
