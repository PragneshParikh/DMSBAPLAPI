using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DMS_BAPL_Utils.Helpers
{
    public class DateOnlyJsonConverter : JsonConverter<DateOnly?>
    {
        private const string WriteFormat = "yyyy-MM-dd";

        private static readonly string[] Formats =
        {
            "yyyy-MM-dd",
            "dd/MM/yyyy",
            "dd-MM-yyyy"
        };

        public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();

            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (DateOnly.TryParseExact(
                    value,
                    Formats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var date))
            {
                return date;
            }

            throw new JsonException($"Invalid date format: {value}");
        }

        public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteStringValue(value.Value.ToString(WriteFormat));
            else
                writer.WriteNullValue();
        }
    }
}