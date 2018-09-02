using Microsoft.AspNetCore.Mvc.Formatters;
using System.Threading.Tasks;

namespace Utf8Json.AspNetCoreMvcFormatter
{
    public class JsonOutputFormatter : IOutputFormatter //, IApiResponseTypeMetadataProvider
    {
		static readonly string[] SupportedContentTypes = new[] { ContentType.ApplicationJson, ContentType.TextPlain };

        readonly IJsonFormatterResolver resolver;

        public JsonOutputFormatter()
            : this(null)
        {

        }
        public JsonOutputFormatter(IJsonFormatterResolver resolver)
        {
            this.resolver = resolver ?? JsonSerializer.DefaultResolver;
        }

        //public IReadOnlyList<string> GetSupportedContentTypes(string contentType, Type objectType)
        //{
        //    return SupportedContentTypes;
        //}

        public bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            return true;
        }

        public Task WriteAsync(OutputFormatterWriteContext context)
        {
	        // when 'object' is string
	        if (context.ObjectType == typeof(string))
	        {
		        context.HttpContext.Response.ContentType = ContentType.TextPlain;
		        return JsonSerializer.NonGeneric.SerializeAsync(context.ObjectType, context.HttpContext.Response.Body, context.Object, resolver);
	        }

	        context.HttpContext.Response.ContentType = ContentType.ApplicationJson;

			// when 'object' use the concrete type(object.GetType())
			if (context.ObjectType == typeof(object))
            {
                return JsonSerializer.NonGeneric.SerializeAsync(context.HttpContext.Response.Body, context.Object, resolver);
            }
            else
            {
                return JsonSerializer.NonGeneric.SerializeAsync(context.ObjectType, context.HttpContext.Response.Body, context.Object, resolver);
            }
        }
    }

    public class JsonInputFormatter : IInputFormatter // , IApiRequestFormatMetadataProvider
    {
		static readonly string[] SupportedContentTypes = new[] { ContentType.ApplicationJson };

        readonly IJsonFormatterResolver resolver;

        public JsonInputFormatter()
            : this(null)
        {

        }

        public JsonInputFormatter(IJsonFormatterResolver resolver)
        {
            this.resolver = resolver ?? JsonSerializer.DefaultResolver;
        }

        //public IReadOnlyList<string> GetSupportedContentTypes(string contentType, Type objectType)
        //{
        //    return SupportedContentTypes;
        //}

        public bool CanRead(InputFormatterContext context)
        {
            return true;
        }

        public Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
        {
            var request = context.HttpContext.Request;
            var result = JsonSerializer.NonGeneric.Deserialize(context.ModelType, request.Body, resolver);
            return InputFormatterResult.SuccessAsync(result);
        }
    }

	internal static class ContentType
	{
		public const string ApplicationJson = "application/json";
		public const string TextPlain = "text/plain";
	}
}