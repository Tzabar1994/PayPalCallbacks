using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BTCallback.Results
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
