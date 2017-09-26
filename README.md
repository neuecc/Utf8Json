Utf8Json - Fast JSON Serializer for C#
===
// TODO: badge

Definitely Fastest and Zero Allocation JSON Serializer for C#(.NET, .NET Core, Unity and Xamarin), this serializer only focus on performance of serialize/deserialize to UTF8 binary. And I adopt the same architecture as the fastest binary serializer, [MessagePack for C#](https://github.com/neuecc/MessagePack-CSharp) that I've developed.

// TODO:graph

Utf8Json does not beat MessagePack for C#(binary), but shows a similar memory consumption(there is no additional memory allocation) and higher performance than other JSON serializers.

The crucial difference is that read and write directly to UTF8 binaries means that there is no overhead. Normaly serialization requires serialize to `Stream` or `byte[]`, it requires additional UTF8.GetBytes cost or StreamReader/Writer overhead(it is very slow!).

```csharp
TargetClass obj1;

// Object to UTF8 byte[]
[Benchmark]
public byte[] Utf8JsonSerializer()
{
    return Utf8Json.JsonSerializer.Serialize(obj1, jsonresolver);
}

// Object to String to UTF8 byte[]
[Benchmark]
public byte[] Jil()
{
    return utf8.GetBytes(global::Jil.JSON.Serialize(obj1));
}

// Object to Stream with StreamWriter
[Benchmark]
public void JilTextWriter()
{
    using (var ms = new MemoryStream())
    using (var sw = new StreamWriter(ms, utf8))
    {
        global::Jil.JSON.Serialize(obj1, sw);
    }
}
```

For example, the `OutputFormatter` of [ASP.NET Core](https://github.com/aspnet/Home) needs to write to Body(`Stream`), but using [Jil](https://github.com/kevin-montrose/Jil)'s TextWriter overload is slow.

```csharp
// ASP.NET Core, OutputFormatter
public class JsonOutputFormatter : IOutputFormatter //, IApiResponseTypeMetadataProvider
{
    const string ContentType = "application/json";
    static readonly string[] SupportedContentTypes = new[] { ContentType };

    public Task WriteAsync(OutputFormatterWriteContext context)
    {
        context.HttpContext.Response.ContentType = ContentType;

        // Jil, normaly JSON Serializer requires serialize to Stream or byte[].
        using (var writer = new StreamWriter(context.HttpContext.Response.Body))
        {
            Jil.JSON.Serialize(context.Object, writer, _options);
            writer.Flush();
            return Task.CompletedTask;
        }

        // Utf8Json
        // Utf8Json.JsonSerializer.NonGeneric.Serialize(context.ObjectType, context.HttpContext.Response.Body, context.Object, resolver);
    }
}
```

The approach of directly write/read from JSON binary is similar to [corefxlab/System.Text.Json](https://github.com/dotnet/corefxlab/tree/master/src/System.Text.Json/System/Text/Json). But it is not yet finished and not be general serializer.

Install and QuickStart
---

// TODO:...


Performance of Serialize
---

// image



releated to [dotnet/coreclr - issue #9786 Optimize Buffer.MemoryCopy](https://github.com/dotnet/coreclr/pull/9786)



```csharp
public sealed class PersonFormatter : IJsonFormatter<Person>
{
    private readonly byte[][] stringByteKeys;
    
    public PersonFormatter()
    {
        // pre-encoded escaped string byte with "{", ":" and ",".
        this.stringByteKeys = new byte[][]
        {
            JsonWriter.GetEncodedPropertyNameWithBeginObject("Age"), // {\"Age\":
            JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Name") // ",\"Name\":
        };
    }

    public sealed Serialize(ref JsonWriter writer, Person person, IJsonFormatterResolver jsonFormatterResolver)
    {
        if (person == null) { writer.WriteNull(); return; }

        // WriteRawX is optimize byte->byte copy when we know src size.
        UnsafeMemory64.WriteRaw7(ref writer, this.stringByteKeys[0]);
        writer.WriteInt32(person.Age);
        UnsafeMemory64.WriteRaw8(ref writer, this.stringByteKeys[1]);
        writer.WriteString(person.Name);

        writer.WriteEndObject();
    }

    // public unsafe Person Deserialize(ref JsonReader reader, IJsonFormatterResolver jsonFormatterResolver)
}
```

WriteRaw7(7 byte memory copy, sizeof(long) copy * 2) + WriteInt32(itoa algorithm requires some branch and copy memory by digits) + WriteRaw8(8 byte memory copy, sizeof(long) copy * 1) + WriteString(two-path encoding, search escape char + Encoding.UTF8.GetBytes) + WriteEndObject(write `}` directly.

This is the everything of serialization cost, probably it will be the smallest.

Performance of Deserialize
---

// image


```csharp
public sealed class PersonFormatter : IJsonFormatter<Person>
{
    private readonly byte[][] stringByteKeys;
    
    public PersonFormatter()
    {
        this.stringByteKeys = new byte[][]
        {
            JsonWriter.GetEncodedPropertyNameWithBeginObject("Age"),
            JsonWriter.GetEncodedPropertyNameWithPrefixValueSeparator("Name")
        };
    }

    public sealed Serialize(ref JsonWriter writer, Person person, IJsonFormatterResolver jsonFormatterResolver)
    {
        if (person == null) { writer.WriteNull(); return; }

        UnsafeMemory64.WriteRaw7(ref writer, this.stringByteKeys[0]);
        writer.WriteInt32(person.Age);
        UnsafeMemory64.WriteRaw8(ref writer, this.stringByteKeys[1]);
        writer.WriteString(person.Name);

        writer.WriteEndObject();
    }

    public unsafe Person Deserialize(ref JsonReader reader, IJsonFormatterResolver jsonFormatterResolver)
    {
        if (reader.ReadIsNull()) return null;
        
        reader.ReadIsBeginObjectWithVerify();
        
        byte[] bufferUnsafe = reader.GetBufferUnsafe();
        int age;
        string name;
        fixed (byte* ptr2 = &bufferUnsafe[0])
        {
            int num;
            while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref num))
            {
                ArraySegment<byte> arraySegment = reader.ReadPropertyNameSegmentUnescaped();
                byte* ptr3 = ptr2 + arraySegment.Offset;
                int count = arraySegment.Count;
                if (count != 0)
                {
                    ulong key = AutomataKeyGen.GetKey(ref ptr3, ref count);
                    if (count == 0)
                    {
                        if (key == 6645569uL)
                        {
                            age = reader.ReadInt32();
                            continue;
                        }
                        if (key == 1701667150uL)
                        {
                            name = reader.ReadString();
                            continue;
                        }
                    }
                }
                reader.ReadNextBlock();
            }
        }

        return new PersonSample
        {
            Age = age,
            Name = name
        };
    }
}
```


Built-in support types
---

DataContract compatibility
---


Serialize ImmutableObject(SerializationConstructor)
---

Dynamic Deserialization
---

Resolvers and Configuration(DateTime format, SnakeCase, etc...)
---

Object Type Serialization
---


Extensions
----

Which Serializer should be used
---


High-Level API(JsonSerializer)
---


Low-Level API(IJsonFormatter)
---


Primitive API(JsonReader/Writer)
---

C# 7.2 ref-like types but not yet safety, be careful.

https://github.com/dotnet/csharplang/blob/master/proposals/csharp-7.2/span-safety.md


Extension Point(IJsonFormatterResolver)
---


IJsonFormatterAttribute
---


for Unity
---


Pre Code Generation(Unity/Xamarin Supports)
---



Author Info
---


License
---
