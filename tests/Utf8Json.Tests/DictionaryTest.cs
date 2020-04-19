using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Utf8Json.Resolvers;
using Xunit;


namespace Utf8Json.Tests
{
    public class DictionaryTest
    {
        T Convert<T>(T value)
        {
            return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value));
        }

        public static object dictionaryTestData = new object[]
        {
            new object[]{ new Dictionary<int, int>() { { 1, 100 } }, null },
            new object[]{ new ReadOnlyDictionary<int,int>(new Dictionary<int, int>() { { 1, 100 } }), null },
            new object[]{ new SortedList<int, int>() { { 1, 100 } }, null },
            new object[]{ new SortedDictionary<int, int>() { { 1, 100 } }, null },
        };

        [Theory]
        [MemberData(nameof(dictionaryTestData))]
        public void DictionaryTestAll<T>(T x, T y)
        {
            Convert(x).IsStructuralEqual(x);
            Convert(y).IsStructuralEqual(y);
        }

        [Fact]
        public void InterfaceDictionaryTest()
        {
            var a = (IDictionary<int, int>)new Dictionary<int, int>() { { 1, 100 } };
            var b = (IReadOnlyDictionary<int, int>)new Dictionary<int, int>() { { 1, 100 } };
            var c = (IDictionary<int, int>)null;
            var d = (IReadOnlyDictionary<int, int>)null;

            var huga = JsonSerializer.ToJsonString(a);

            Convert(a).IsStructuralEqual(a);
            Convert(b).IsStructuralEqual(b);
            Convert(c).IsStructuralEqual(c);
            Convert(d).IsStructuralEqual(d);
        }

        [Fact]
        public void ConcurrentDictionaryTest()
        {
            var cd = new ConcurrentDictionary<int, int>();

            cd.TryAdd(1, 100);
            cd.TryAdd(2, 200);
            cd.TryAdd(3, 300);

            var conv = Convert(cd);
            conv[1].Is(100);
            conv[2].Is(200);
            conv[3].Is(300);

            cd = null;
            Convert(cd).IsNull();
        }

        public class ObjectTestFormatter
        {
            public string MyProperty { get; set; }
            public Dictionary<string, string> Dictionary { get; set; }
        }

        public static object dictionaryFormatterTestData = 
            new object[]
            {
                new object[] 
                {
                    new ObjectTestFormatter
                    {
                        MyProperty = "Value",
                        Dictionary = new Dictionary<string, string> {{"Key", "Value"}}
                    },
                    StandardResolver.CamelCase,
                    new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }
                },
                new object[]
                {
                    new ObjectTestFormatter
                    {
                        MyProperty = "Value",
                        Dictionary = new Dictionary<string, string> {{"Key", "Value"}},
                    },
                    StandardResolver.Default,
                    new JsonSerializerSettings()
                },
                new object[] 
                {
                    new ObjectTestFormatter
                    {
                        MyProperty = "Value",
                        Dictionary = new Dictionary<string, string> {{"Key", "Value"}},
                    },
                    StandardResolver.SnakeCase,
                    new JsonSerializerSettings
                    {
                        ContractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy { ProcessDictionaryKeys = true } },
                    }
                }
            };
        
        [Theory]
        [MemberData(nameof(dictionaryFormatterTestData))]
        public void DictionaryFormatterTest(ObjectTestFormatter input, IJsonFormatterResolver resolver, JsonSerializerSettings newtonsoftResolver)
        {
            var data = JsonSerializer.Serialize(input, resolver);
            var actualJson = Encoding.UTF8.GetString(data);
            var expectedJson = JsonConvert.SerializeObject(input, newtonsoftResolver);

            actualJson.Is(expectedJson);
        }
    }
}