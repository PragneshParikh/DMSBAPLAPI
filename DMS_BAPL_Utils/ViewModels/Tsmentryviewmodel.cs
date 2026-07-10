using System.Text.Json.Serialization;

namespace DMS_BAPL_Utils.ViewModels
{
    // Matches the JSON shape returned by the external ERP TSM API:
    // https://bapldmsai-e6f0hzhmg4achue9.centralindia-01.azurewebsites.net/api/erptsmmaster/TSMEntry
    // [JsonPropertyName] attributes preserve the exact external casing on
    // BOTH deserialization (reading their response) and serialization
    // (this API's own response to Angular) — so the frontend's existing
    // applyTsmData() parsing needs no changes at all.
    public class TsmEntryViewModel
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
        public string? AreaOffIdNo { get; set; }

        [JsonPropertyName("Photo")]
        public string? Photo { get; set; }

        [JsonPropertyName("tsmheadcode")]
        public string? TsmHeadCode { get; set; }
    }
}