using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.CustomModel
{
    public class ApiResponse
    {
        // FIXED: without JsonPropertyName, System.Text.Json's default camelCase
        // policy lowercases the first letter on serialization (Valid -> valid),
        // which is what Swagger showed. This forces exact PascalCase output
        // regardless of the app's global JSON naming policy.
        [JsonPropertyName("Valid")]
        public bool Valid { get; set; }

        [JsonPropertyName("Description")]
        public string? Description { get; set; } = null;

        [JsonPropertyName("Value")]
        public object? Value { get; set; } = null;
    }

    // NEW: typed shape for the items inside Value, so nested properties
    // (Msg/StatusCode/ResponseStatus) also serialize with capital letters
    // instead of being camelCased as anonymous objects would be.
    public class ApiResponseValue
    {
        [JsonPropertyName("Msg")]
        public string Msg { get; set; } = string.Empty;

        [JsonPropertyName("StatusCode")]
        public string StatusCode { get; set; } = string.Empty;

        [JsonPropertyName("ResponseStatus")]
        public string ResponseStatus { get; set; } = string.Empty;
    }
}