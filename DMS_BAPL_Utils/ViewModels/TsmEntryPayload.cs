using System.Text.Json.Serialization;

namespace DMS_BAPL_Utils.ViewModels
{
    public class TsmEntryPayload
    {
        [JsonPropertyName("employeeCode")]
        public string? TsmCode { get; set; }

        [JsonPropertyName("employeeName")]
        public string? TsmName { get; set; }

        [JsonPropertyName("mobileno")]
        public string? MobileNo { get; set; }

        [JsonPropertyName("state")]
        public int? State { get; set; }

        [JsonPropertyName("city")]
        public int? City { get; set; }

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

        [JsonPropertyName("pwd")]
        public string? Pwd { get; set; }

        [JsonPropertyName("areaoffidno")]
        public string? AreaOfficeId { get; set; }

        [JsonPropertyName("Photo")]
        public string? Photo { get; set; }

        [JsonPropertyName("tsmheadcode")]
        public string? TsmHeadCode { get; set; }

        [JsonPropertyName("reportingTo")]
        public int? ReportingTo { get; set; }

        [JsonPropertyName("department")]
        public int? Department { get; set; }

        [JsonPropertyName("isAccepted")]
        public bool? IsAccepted { get; set; }

        [JsonPropertyName("address1")]
        public string? Address1 { get; set; }

        [JsonPropertyName("address2")]
        public string? Address2 { get; set; }

        [JsonPropertyName("pincode")]
        public string? Pincode { get; set; }
    }
}