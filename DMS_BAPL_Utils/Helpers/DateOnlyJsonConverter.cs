using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DMS_BAPL_Utils.Helpers
{
    public class DateOnlyJsonConverter : JsonConverter<DateOnly?>
    {
        private const string Format = "dd/MM/yyyy";

        public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();

            if (string.IsNullOrWhiteSpace(value))
                return null;

            return DateOnly.ParseExact(value, Format, CultureInfo.InvariantCulture);
        }

        public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteStringValue(value.Value.ToString(Format));
            else
                writer.WriteNullValue();
        }
    }
}