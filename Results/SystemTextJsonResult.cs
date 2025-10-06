using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ShippingCallbacks.Results
{
    public class SystemTextJsonResult : ContentResult
    {
        private const string ContentTypeApplicationJson = "application/json";

        public SystemTextJsonResult(object value, JsonSerializerOptions options, int? code = 200)
        {
            ContentType = ContentTypeApplicationJson;
            Content = JsonSerializer.Serialize(value, options);
            StatusCode = code;
        }
    }
}
