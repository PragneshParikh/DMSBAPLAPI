using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DMS_BAPL_Utils.Helpers
{
    public class FlexibleDateTimeConverter : JsonConverter<DateTime?>
    {
        private static readonly string[] Formats =
        {
            "dd/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-dd", "yyyy-MM-ddTHH:mm:ss"
        };

        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null) return null;

            var value = reader.GetString();
            if (string.IsNullOrWhiteSpace(value)) return null;

            if (DateTime.TryParseExact(value, Formats, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var result))
                return result;

            if (DateTime.TryParse(value, out result))
                return result;

            throw new JsonException($"Unable to parse date: {value}");
        }

        //public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        //{
        //    if (value.HasValue)
        //        writer.WriteStringValue(value.Value.ToString("yyyy-MM-dd"));
        //    else
        //        writer.WriteNullValue();
        //}
        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (!value.HasValue)
            {
                writer.WriteNullValue();
                return;
            }

            // Normalize all DateTime values to UTC before serializing them in the API response.
            var utcValue = value.Value.Kind switch
            {
                DateTimeKind.Utc => value.Value,
                DateTimeKind.Local => value.Value.ToUniversalTime(),
                DateTimeKind.Unspecified => DateTime.SpecifyKind(
                    value.Value,
                    DateTimeKind.Utc),
                _ => value.Value
            };

            writer.WriteStringValue(utcValue.ToString("O", CultureInfo.InvariantCulture));
        }
    }
}