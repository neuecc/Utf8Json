using System.Text;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Threading.Tasks;

namespace Utf8Json.AspNetCoreMvcFormatter
{
    public class JsonOutputFormatter : TextOutputFormatter
    {
        readonly IJsonFormatterResolver resolver;

        public JsonOutputFormatter()
            : this(null)
        {

        }

        public JsonOutputFormatter(IJsonFormatterResolver resolver)
        {
            this.resolver = resolver ?? JsonSerializer.DefaultResolver;
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedMediaTypes.Add(MediaTypeHeaderValues.ApplicationJson);
            SupportedMediaTypes.Add(MediaTypeHeaderValues.TextJson);
            SupportedMediaTypes.Add(MediaTypeHeaderValues.ApplicationAnyJsonSyntax);
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            return context.ObjectType == typeof(object)
                ? JsonSerializer.NonGeneric.SerializeAsync(context.HttpContext.Response.Body,
                    context.Object,
                    resolver)
                : JsonSerializer.NonGeneric.SerializeAsync(context.ObjectType,
                    context.HttpContext.Response.Body,
                    context.Object,
                    resolver);
        }
    }

    public class JsonInputFormatter : TextInputFormatter
    {
        readonly IJsonFormatterResolver resolver;

        public JsonInputFormatter()
            : this(null)
        {

        }

        public JsonInputFormatter(IJsonFormatterResolver resolver)
        {
            this.resolver = resolver ?? JsonSerializer.DefaultResolver;
            SupportedEncodings.Add(UTF8EncodingWithoutBOM);
            SupportedMediaTypes.Add(MediaTypeHeaderValues.ApplicationJson);
            SupportedMediaTypes.Add(MediaTypeHeaderValues.TextJson);
            SupportedMediaTypes.Add(MediaTypeHeaderValues.ApplicationAnyJsonSyntax);
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            var request = context.HttpContext.Request;
            var result = await JsonSerializer.NonGeneric.DeserializeAsync(context.ModelType,
                request.Body,
                resolver).ConfigureAwait(false);
            return InputFormatterResult.Success(result);
        }
    }
}