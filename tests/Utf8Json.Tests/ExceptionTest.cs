using System;
using Xunit;

namespace Utf8Json.Tests
{
    public class FooException : Exception
    {
        public int Bar { get; set; }

        public FooException() : base("BCD")
        {

        }
    }

    public class ExceptionTest
    {
        [Fact]
        public void Root1()
        {
            var ex = new Exception("ABC");
            var json = JsonSerializer.ToJsonString(ex);
            json.Is("{\"ClassName\":\"System.Exception\",\"Message\":\"ABC\",\"Source\":null,\"StackTrace\":null}");
        }

        [Fact]
        public void Root2()
        {
            var ex = new FooException { Bar = 100 };

            var json = JsonSerializer.ToJsonString(ex);
            json.Is("{\"ClassName\":\"Utf8Json.Tests.FooException\",\"Message\":\"BCD\",\"Source\":null,\"StackTrace\":null}");
        }

        [Fact]
        public void Inner()
        {
            var ex = new Exception("ABC", new FooException { Bar = 100 });

            var json = JsonSerializer.ToJsonString(ex);
            json.Is("{\"ClassName\":\"System.Exception\",\"Message\":\"ABC\",\"Source\":null,\"StackTrace\":null,\"InnerException\":{\"ClassName\":\"Utf8Json.Tests.FooException\",\"Message\":\"BCD\",\"Source\":null,\"StackTrace\":null}}");
        }
    }
}
