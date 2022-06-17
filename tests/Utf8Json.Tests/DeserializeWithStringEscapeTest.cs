using System;
using System.Text;
using Xunit;

namespace Utf8Json.Tests
{
    public class DeserializeWithStringEscapeTest
    {
        [Theory]
        [InlineData(@"{""Name"":""\\"", ""Test"":""Something""}", "Something")]
        [InlineData(@"{""Name"":"""", ""Test"":""Something""}", "Something")]
        [InlineData(@"{""Name"":""\"""", ""Test"":""Something""}", "Something")]
        [InlineData(@"{""Name"":""\""\"""", ""Test"":""Something""}", "Something")]
        public void ShouldNotEscapeDoublequoteWithEscapedBackslash(string json, string expectedValue)
        {
            // Arrage
            var reader = new JsonReader(Encoding.UTF8.GetBytes(json));

            // Act
            reader.ReadIsBeginObject();
            var count = 0;
            while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref count))
            {
                var name = reader.ReadPropertyName();
                if (name == "Test")
                    reader.ReadString().Is(expectedValue);
                else
                {
                    reader.ReadNextBlock();
                }
            }

            reader.ReadIsValueSeparator();
        }
    }
}