using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Utf8Json.Tests
{
    public class StringEscapeTest
    {
        [Fact]
        public void BasicEncodedChars()
        {
            JsonSerializer.ToJsonString("\"").Is("\"\\\"\"");
            JsonSerializer.ToJsonString("\\").Is("\"\\\\\"");
            JsonSerializer.ToJsonString("\b").Is("\"\\b\"");
            JsonSerializer.ToJsonString("\f").Is("\"\\f\"");
            JsonSerializer.ToJsonString("\n").Is("\"\\n\"");
            JsonSerializer.ToJsonString("\r").Is("\"\\r\"");
            JsonSerializer.ToJsonString("\t").Is("\"\\t\"");

            // Check round-trip
            var str = "\" \\ \b \f \n \r \t";
            var encoded = JsonSerializer.ToJsonString(str);
            encoded.Is("\"\\\" \\\\ \\b \\f \\n \\r \\t\"");
            JsonSerializer.Deserialize<string>(encoded).Is(str);
        }

        [Fact]
        public void Mixed()
        {
            var str = @"""\u0428\u0440\u0438-\u041b\u0430\u043d\u043a\u0430""";
            JsonSerializer.Deserialize<string>(str).Is("Шри-Ланка");

            str = @"""\u041d\u043e\u0432\u0430\u044f \u0437\u0435\u043b\u0430\u043d\u0434\u0438\u044f""";
            JsonSerializer.Deserialize<string>(str).Is("Новая зеландия");

            str = @"""\u041d\u043e\u0432\u0430\u044f___\u0437\u0435\u043b\u0430\t\u043d\u0434\u0438\u044f""";
            JsonSerializer.Deserialize<string>(str).Is("Новая___зела\tндия");
        }

        [Fact]
        public void LongEscapedCharactersShouldDeserialize()
        {
            var str = @"""\uD834\uDD1E""";
            JsonSerializer.Deserialize<string>(str).Is("𝄞");
            JsonSerializer.ToJsonString("𝄞").Is(@"""𝄞""");
        }

        [Fact]
        public void ControlCharactersShouldDeserialize()
        {
            string ByteAsStr(byte charNum) => new string(new[] {(char)charNum });

            JsonSerializer.Deserialize<string>(@"""\u0000""").Is(ByteAsStr(0x00));
            JsonSerializer.Deserialize<string>(@"""\u0001""").Is(ByteAsStr(0x01));
            JsonSerializer.Deserialize<string>(@"""\u0002""").Is(ByteAsStr(0x02));
            JsonSerializer.Deserialize<string>(@"""\u0003""").Is(ByteAsStr(0x03));
            JsonSerializer.Deserialize<string>(@"""\u0004""").Is(ByteAsStr(0x04));
            JsonSerializer.Deserialize<string>(@"""\u0005""").Is(ByteAsStr(0x05));
            JsonSerializer.Deserialize<string>(@"""\u0006""").Is(ByteAsStr(0x06));
            JsonSerializer.Deserialize<string>(@"""\u0007""").Is(ByteAsStr(0x07));
            JsonSerializer.Deserialize<string>(@"""\u0008""").Is(ByteAsStr(0x08));
            JsonSerializer.Deserialize<string>(@"""\b""").Is(ByteAsStr(0x08));
            JsonSerializer.Deserialize<string>(@"""\u0009""").Is(ByteAsStr(0x09));
            JsonSerializer.Deserialize<string>(@"""\t""").Is(ByteAsStr(0x09));
            JsonSerializer.Deserialize<string>(@"""\u000a""").Is(ByteAsStr(0x0a));
            JsonSerializer.Deserialize<string>(@"""\n""").Is(ByteAsStr(0x0a));
            JsonSerializer.Deserialize<string>(@"""\u000b""").Is(ByteAsStr(0x0b));
            JsonSerializer.Deserialize<string>(@"""\u000c""").Is(ByteAsStr(0x0c));
            JsonSerializer.Deserialize<string>(@"""\f""").Is(ByteAsStr(0x0c));
            JsonSerializer.Deserialize<string>(@"""\u000d""").Is(ByteAsStr(0x0d));
            JsonSerializer.Deserialize<string>(@"""\r""").Is(ByteAsStr(0x0d));
            JsonSerializer.Deserialize<string>(@"""\u000e""").Is(ByteAsStr(0x0e));
            JsonSerializer.Deserialize<string>(@"""\u000f""").Is(ByteAsStr(0x0f));
            JsonSerializer.Deserialize<string>(@"""\u0010""").Is(ByteAsStr(0x10));
            JsonSerializer.Deserialize<string>(@"""\u0011""").Is(ByteAsStr(0x11));
            JsonSerializer.Deserialize<string>(@"""\u0012""").Is(ByteAsStr(0x12));
            JsonSerializer.Deserialize<string>(@"""\u0013""").Is(ByteAsStr(0x13));
            JsonSerializer.Deserialize<string>(@"""\u0014""").Is(ByteAsStr(0x14));
            JsonSerializer.Deserialize<string>(@"""\u0015""").Is(ByteAsStr(0x15));
            JsonSerializer.Deserialize<string>(@"""\u0016""").Is(ByteAsStr(0x16));
            JsonSerializer.Deserialize<string>(@"""\u0017""").Is(ByteAsStr(0x17));
            JsonSerializer.Deserialize<string>(@"""\u0018""").Is(ByteAsStr(0x18));
            JsonSerializer.Deserialize<string>(@"""\u0019""").Is(ByteAsStr(0x19));
            JsonSerializer.Deserialize<string>(@"""\u001a""").Is(ByteAsStr(0x1a));
            JsonSerializer.Deserialize<string>(@"""\u001b""").Is(ByteAsStr(0x1b));
            JsonSerializer.Deserialize<string>(@"""\u001c""").Is(ByteAsStr(0x1c));
            JsonSerializer.Deserialize<string>(@"""\u001d""").Is(ByteAsStr(0x1d));
            JsonSerializer.Deserialize<string>(@"""\u001e""").Is(ByteAsStr(0x1e));
            JsonSerializer.Deserialize<string>(@"""\u001f""").Is(ByteAsStr(0x1f));
        }

        [Fact]
        public void ControlCharactersShouldSerialize()
        {
            string ByteAsStr(byte charNum) => new string(new[] { (char)charNum });

            JsonSerializer.ToJsonString(ByteAsStr(0x00)).Is(@"""\u0000""");
            JsonSerializer.ToJsonString(ByteAsStr(0x01)).Is(@"""\u0001""");
            JsonSerializer.ToJsonString(ByteAsStr(0x02)).Is(@"""\u0002""");
            JsonSerializer.ToJsonString(ByteAsStr(0x03)).Is(@"""\u0003""");
            JsonSerializer.ToJsonString(ByteAsStr(0x04)).Is(@"""\u0004""");
            JsonSerializer.ToJsonString(ByteAsStr(0x05)).Is(@"""\u0005""");
            JsonSerializer.ToJsonString(ByteAsStr(0x06)).Is(@"""\u0006""");
            JsonSerializer.ToJsonString(ByteAsStr(0x07)).Is(@"""\u0007""");
            JsonSerializer.ToJsonString(ByteAsStr(0x08)).Is(@"""\b""");
            JsonSerializer.ToJsonString(ByteAsStr(0x09)).Is(@"""\t""");
            JsonSerializer.ToJsonString(ByteAsStr(0x0a)).Is(@"""\n""");
            JsonSerializer.ToJsonString(ByteAsStr(0x0b)).Is(@"""\u000b""");
            JsonSerializer.ToJsonString(ByteAsStr(0x0c)).Is(@"""\f""");
            JsonSerializer.ToJsonString(ByteAsStr(0x0d)).Is(@"""\r""");
            JsonSerializer.ToJsonString(ByteAsStr(0x0e)).Is(@"""\u000e""");
            JsonSerializer.ToJsonString(ByteAsStr(0x0f)).Is(@"""\u000f""");
            JsonSerializer.ToJsonString(ByteAsStr(0x10)).Is(@"""\u0010""");
            JsonSerializer.ToJsonString(ByteAsStr(0x11)).Is(@"""\u0011""");
            JsonSerializer.ToJsonString(ByteAsStr(0x12)).Is(@"""\u0012""");
            JsonSerializer.ToJsonString(ByteAsStr(0x13)).Is(@"""\u0013""");
            JsonSerializer.ToJsonString(ByteAsStr(0x14)).Is(@"""\u0014""");
            JsonSerializer.ToJsonString(ByteAsStr(0x15)).Is(@"""\u0015""");
            JsonSerializer.ToJsonString(ByteAsStr(0x16)).Is(@"""\u0016""");
            JsonSerializer.ToJsonString(ByteAsStr(0x17)).Is(@"""\u0017""");
            JsonSerializer.ToJsonString(ByteAsStr(0x18)).Is(@"""\u0018""");
            JsonSerializer.ToJsonString(ByteAsStr(0x19)).Is(@"""\u0019""");
            JsonSerializer.ToJsonString(ByteAsStr(0x1a)).Is(@"""\u001a""");
            JsonSerializer.ToJsonString(ByteAsStr(0x1b)).Is(@"""\u001b""");
            JsonSerializer.ToJsonString(ByteAsStr(0x1c)).Is(@"""\u001c""");
            JsonSerializer.ToJsonString(ByteAsStr(0x1d)).Is(@"""\u001d""");
            JsonSerializer.ToJsonString(ByteAsStr(0x1e)).Is(@"""\u001e""");
            JsonSerializer.ToJsonString(ByteAsStr(0x1f)).Is(@"""\u001f""");
        }

        [Fact]
        public void MixedControlCharactersShouldRoundTrip()
        {
            var str = "\"" +
                      "TextAtTheBeginning" +
                      "\\u0000\\u0001\\u0002\\u0003\\u0004\\u0005\\u0006\\u0007\\b\\t\\n\\u000b\\f\\r\\u000e\\u000f" +
                      "𝄞TextInTheMiddle𝄞" +
                      "\\u0010\\u0011\\u0012\\u0013\\u0014\\u0015\\u0016\\u0017\\u0018\\u0019\\u001a\\u001b\\u001c\\u001d\\u001e\\u001f" +
                      "TextAtTheEnd" +
                      "\"";
            var bytes = JsonSerializer.Deserialize<string>(str);
            var convertedStr = JsonSerializer.ToJsonString(bytes);
            convertedStr.Is(str);
        }
    }
}
