using System;
using System.IO;
using System.Text;
using Utf8Json.Resolvers;
using Xunit;

namespace Utf8Json.Tests
{
    public class CamelCaseNonGenericDeserializeTest
    {
        
        private readonly Guid _guid = Guid.Parse("EA0E3AF3-7EED-4ABA-970E-AC91E25625F3");

        public class TestClass
        {
            public TestClass(Guid actualId)
            {
                ActualId = actualId;
            }

            public Guid ActualId { get; }
        }

        [Fact]
        public void ShouldDeserializeExistingJsonFromByteArrayWithoutOffset()
        {
            var jsonString = "{\"actualId\":\"ea0e3af3-7eed-4aba-970e-ac91e25625f3\"}";
            var bytes = Encoding.UTF8.GetBytes(jsonString);
            var item = (TestClass)JsonSerializer.NonGeneric.Deserialize(typeof(TestClass), bytes, StandardResolver.CamelCase);
            Assert.Equal(_guid, item.ActualId);
        }

        [Fact]
        public void ShouldDeserializeExistingJsonFromByteArrayWithOffset()
        {
            var jsonString = "{\"actualId\":\"ea0e3af3-7eed-4aba-970e-ac91e25625f3\"}";
            var bytes = Encoding.UTF8.GetBytes(jsonString);
            var item = (TestClass)JsonSerializer.NonGeneric.Deserialize(typeof(TestClass), bytes, 0, StandardResolver.CamelCase);
            Assert.Equal(_guid, item.ActualId);
        }

        [Fact]
        public void ShouldDeserializeExistingJsonFromStream()
        {
            var jsonString = "{\"actualId\":\"ea0e3af3-7eed-4aba-970e-ac91e25625f3\"}";
            var bytes = Encoding.UTF8.GetBytes(jsonString);
            var item = (TestClass)JsonSerializer.NonGeneric.Deserialize(typeof(TestClass), new MemoryStream(bytes), StandardResolver.CamelCase);
            Assert.Equal(_guid, item.ActualId);
        }
    }
}