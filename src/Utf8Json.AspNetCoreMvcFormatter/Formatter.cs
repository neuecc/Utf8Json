using Microsoft.AspNetCore.Mvc.Formatters;
using System.Threading.Tasks;

namespace Utf8Json.AspNetCoreMvcFormatter
{
    public class Utf8JsonOutputFormatter : IOutputFormatter
    {
        private const string ContentType = "application/json";
        private readonly IJsonFormatterResolver _jsonFormatterResolver;

        public Utf8JsonOutputFormatter() : this(null)
        {
        }

        public Utf8JsonOutputFormatter(
            IJsonFormatterResolver jsonFormatterResolver)
            => _jsonFormatterResolver =
                jsonFormatterResolver ?? JsonSerializer.DefaultResolver;

        public bool CanWriteResult(OutputFormatterCanWriteContext context)
            => context.HttpContext.Request.Headers.TryGetValue(
                   "Accept",
                   out var acceptValues) &&
               acceptValues.All(accept => accept.Contains(
                   ContentType));

        public Task WriteAsync(OutputFormatterWriteContext context)
        {
            var objectType = context.ObjectType;
            var obj = context.Object;

            var serializeType = objectType == typeof(object)
                ? obj.GetType()
                : objectType;

            return JsonSerializer.NonGeneric.SerializeAsync(
                serializeType,
                context.HttpContext.Response.Body,
                obj,
                _jsonFormatterResolver);
        }
    }

    public class Utf8JsonInputFormatter : IInputFormatter
    {
        private const string ContentType = "application/json";
        private readonly IJsonFormatterResolver _jsonFormatterResolver;

        public Utf8JsonInputFormatter() : this(null)
        {
        }

        public Utf8JsonInputFormatter(
            IJsonFormatterResolver jsonFormatterResolver)
            => _jsonFormatterResolver =
                jsonFormatterResolver ?? JsonSerializer.DefaultResolver;

        public bool CanRead(InputFormatterContext context)
            => context.HttpContext.Request.ContentType ==
               ContentType;

        public async Task<InputFormatterResult> ReadAsync(
            InputFormatterContext context)
        {
            try
            {
                var model = await JsonSerializer.NonGeneric
                    .DeserializeAsync(
                        context.ModelType,
                        context.HttpContext.Request.Body,
                        _jsonFormatterResolver)
                    .ConfigureAwait(false);
                return InputFormatterResult.Success(model);
            }
            catch (JsonParsingException)
            {
                return InputFormatterResult.Failure();
            }
        }
    }
}