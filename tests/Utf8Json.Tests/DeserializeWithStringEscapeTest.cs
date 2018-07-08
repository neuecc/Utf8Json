using System.Text;
using Xunit;

namespace Utf8Json.Tests
{
    public class DeserializeWithStringEscapeTest
    {
        [Theory]
        [InlineData(@"{""Name"":""\\"", ""Test"":""Something""}", "\\","Something")]
        [InlineData(@"{""Name"":"""", ""Test"":""Something""}", "","Something")]
        [InlineData(@"{""Name"":""\"""", ""Test"":""Something""}", "\"","Something")]
        [InlineData(@"{""Name"":""\""\"""", ""Test"":""Something""}", "\"\"","Something")]
        public void ShouldNotEscapeDoublequoteWithEscapedBackslash(string json, string expectedValueWithBackSlash, string expectedValue)
        {
            // Arrage
            var reader = new JsonReader(Encoding.UTF8.GetBytes(json));

            // Act
            reader.ReadIsBeginObject();
            var count = 0;
            while (!reader.ReadIsEndObjectWithSkipValueSeparator(ref count))
            {
                var name = reader.ReadPropertyName();
                switch (name)
                {
                    case "Test":
                        reader.ReadString().Is(expectedValue);
                        break;
                    case "Name":
                        reader.ReadString().Is(expectedValueWithBackSlash);
                        break;
                }
            }
            reader.ReadIsValueSeparator();
        }
    }
}