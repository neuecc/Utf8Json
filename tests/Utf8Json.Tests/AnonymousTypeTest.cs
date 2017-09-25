using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Utf8Json.Tests
{
    public class AnonymousTypeTest
    {
        [Fact]
        public void SerializeAndToJson()
        {
            var testData = new { Hoge = 100, Huga = true, Yaki = new { Rec = 1, T = 10 }, Nano = "nanoanno" };

            var data = JsonSerializer.ToJsonString(testData);

            data.Is(@"{""Hoge"":100,""Huga"":true,""Yaki"":{""Rec"":1,""T"":10},""Nano"":""nanoanno""}");
        }
    }
}
