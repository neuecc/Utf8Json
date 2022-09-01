using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using Utf8Json.AspNetCoreMvcFormatter;
using Utf8Json.Resolvers;
using Xunit;

namespace Utf8Json.Tests
{
    public class AspNetCoreFormatterTests
    {
        private MemoryStream responseStream = new MemoryStream();
        private InputFormatter inputFormatter;
        private OutputFormatter outputFormatter;
        private HttpContext context;
        private TestClass obj = new TestClass
        {
            A = 2
        };

        public class TestClass
        {
            public int A { get; set; }
        }

        public AspNetCoreFormatterTests()
        {
            context = new DefaultHttpContext();
            context.Response.Body = responseStream;
        }

        [Fact]
        public async Task OutputFormatterShouldFormatWithDefaultParameters()
        {
            outputFormatter = new JsonOutputFormatter();
            var outputContext = CreateOutputFormatterContext(obj, obj.GetType(), "application/json");

            Assert.True(outputFormatter.CanWriteResult(outputContext));
            await outputFormatter.WriteAsync(outputContext);

            Assert.Equal(outputContext.ContentType, "application/json; charset=utf-8");
            Assert.Equal(200, outputContext.HttpContext.Response.StatusCode);

            var responseText = Encoding.UTF8.GetString(responseStream.ToArray());
            Assert.Equal("{\"A\":2}", responseText);
        }

        [Fact]
        public async Task OutputFormatterShouldRespectSerializerSettings()
        {
            outputFormatter = new JsonOutputFormatter(StandardResolver.CamelCase);
            var outputContext = CreateOutputFormatterContext(obj, obj.GetType(), "application/json");

            await outputFormatter.WriteAsync(outputContext);

            Assert.Equal(outputContext.ContentType, "application/json; charset=utf-8");
            Assert.Equal(200, outputContext.HttpContext.Response.StatusCode);

            var responseText = Encoding.UTF8.GetString(responseStream.ToArray());
            Assert.Equal("{\"a\":2}", responseText);
        }

        [Fact]
        public void OutputFormatterShouldNotWriteResultWithUnsupportedContentType()
        {
            outputFormatter = new JsonOutputFormatter();
            var outputContext = CreateOutputFormatterContext(obj, obj.GetType(), "application/xml");

            Assert.False(outputFormatter.CanWriteResult(outputContext));
        }

        [Theory]
        [InlineData("application/json", false, "application/json")]
        [InlineData("application/json", true, "application/json")]
        [InlineData("application/xml", false, null)]
        [InlineData("application/xml", true, null)]
        [InlineData("application/*", false, "application/json")]
        [InlineData("text/*", false, "text/json")]
        [InlineData("custom/*", false, null)]
        [InlineData("application/json;v=2", false, null)]
        [InlineData("application/json;v=2", true, null)]
        [InlineData("application/some.entity+json", false, null)]
        [InlineData("application/some.entity+json", true, "application/some.entity+json")]
        [InlineData("application/some.entity+json;v=2", true, "application/some.entity+json;v=2")]
        [InlineData("application/some.entity+xml", true, null)]
        public void OutputFormatterCanWriteReturnsExpectedValue(string mediaType, bool isServerDefined, string expectedResult)
        {
            var formatter = new JsonOutputFormatter();
            var outputFormatterContext = CreateOutputFormatterContext(new object(), typeof(object), mediaType, isServerDefined);

            var actualCanWriteValue = formatter.CanWriteResult(outputFormatterContext);

            // Assert
            var expectedContentType = expectedResult ?? mediaType;
            Assert.Equal(expectedResult != null, actualCanWriteValue);
            Assert.Equal(new StringSegment(expectedContentType), outputFormatterContext.ContentType);
        }

        [Fact]
        public async Task InputFormatterShouldFormatWithDefaultParameters()
        {
            inputFormatter = new JsonInputFormatter();
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{\"A\":2}"));
            context.Request.ContentLength = context.Request.Body.Length;
            context.Request.ContentType = "application/json";

            var inputContext = CreateInputFormatterContext(typeof(TestClass));
            Assert.True(inputFormatter.CanRead(inputContext));
            var inputFormatterResult = await inputFormatter.ReadAsync(inputContext);
            Assert.Equal(2, ((TestClass)inputFormatterResult.Model).A);
        }

        [Fact]
        public async Task InputFormatterShouldRespectSerializerSettings()
        {
            inputFormatter = new JsonInputFormatter(StandardResolver.CamelCase);
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{\"a\":2}"));
            context.Request.ContentLength = context.Request.Body.Length;
            context.Request.ContentType = "application/json";

            var inputContext = CreateInputFormatterContext(typeof(TestClass));
            Assert.True(inputFormatter.CanRead(inputContext));
            var inputFormatterResult = await inputFormatter.ReadAsync(inputContext);
            Assert.Equal(2, ((TestClass)inputFormatterResult.Model).A);
        }

        [Theory]
        [InlineData("application/json", true)]
        [InlineData("application/*", false)]
        [InlineData("*/*", false)]
        [InlineData("text/json", true)]
        [InlineData("text/*", false)]
        [InlineData("text/xml", false)]
        [InlineData("application/xml", false)]
        [InlineData("application/some.entity+json", true)]
        [InlineData("application/some.entity+json;v=2", true)]
        [InlineData("application/some.entity+xml", false)]
        [InlineData("application/some.entity+*", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        [InlineData("invalid", false)]
        public void InputFormatterCanReadAnySupportedContentType(string requestContentType, bool expectedCanRead)
        {
            context.Request.ContentType = requestContentType;
            inputFormatter = new JsonInputFormatter();
            var formatterContext = CreateInputFormatterContext(typeof(string));

            var result = inputFormatter.CanRead(formatterContext);

            Assert.Equal(expectedCanRead, result);
        }




        private OutputFormatterWriteContext CreateOutputFormatterContext(
            object outputValue,
            Type outputType,
            string contentType = "application/xml; charset=utf-8",
            bool contentTypeIsServerDefined = false)
        {
            return new OutputFormatterWriteContext(
                context,
                (stream, encoding) => new StreamWriter(stream),
                outputType,
                outputValue)
            {
                ContentType = new StringSegment(contentType),
                ContentTypeIsServerDefined = contentTypeIsServerDefined
            };
        }

        private InputFormatterContext CreateInputFormatterContext(
            Type modelType,
            string modelName = null)
        {
            var provider = new EmptyModelMetadataProvider();
            var metadata = provider.GetMetadataForType(modelType);

            return new InputFormatterContext(
                context,
                modelName ?? string.Empty,
                new ModelStateDictionary(),
                metadata,
                (stream, encoding) => new StreamReader(stream));
        }
    }
}