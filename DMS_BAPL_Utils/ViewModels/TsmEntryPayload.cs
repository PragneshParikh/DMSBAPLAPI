using System.Text.Json.Serialization;

namespace DMS_BAPL_Utils.ViewModels
{
    public class TsmEntryPayload
    {
        [JsonPropertyName("tsmcode")]
        public string? TsmCode { get; set; }

        [JsonPropertyName("tsmname")]
        public string? TsmName { get; set; }

        [JsonPropertyName("mobileno")]
        public string? MobileNo { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("estatus")]
        public string? EStatus { get; set; }

        [JsonPropertyName("doa")]
        public string? Doa { get; set; }

        [JsonPropertyName("dob")]
        public string? Dob { get; set; }

        [JsonPropertyName("doe")]
        public string? Doe { get; set; }

        [JsonPropertyName("gender")]
        public string? Gender { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("areaoffidno")]
        public string? AreaOfficeId { get; set; }

        [JsonPropertyName("Photo")]
        public string? Photo { get; set; }

        [JsonPropertyName("tsmheadcode")]
        public string? TsmHeadCode { get; set; }
    }
}