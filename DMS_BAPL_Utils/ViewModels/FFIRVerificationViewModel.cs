namespace DMS_BAPL_Utils.ViewModels
{
    /// <summary>FFIR fields displayed by the BG Warranty verification list.</summary>
    public sealed class FFIRVerificationViewModel
    {
        public int Id { get; set; }
        public string FFIRNumber { get; set; } = "";
        public DateTime? FFIRDate { get; set; }
        public string? DealerCode { get; set; }
        public string? DealerName { get; set; }
        public string? City { get; set; }
        public string? ReportTitle { get; set; }
        public string? ReportPreparedBy { get; set; }
        public string? TypeOfRoadSurface { get; set; }
        public string? FFIRRemarks { get; set; }
        public string? ChassisNo { get; set; }
        public string? RegisterNo { get; set; }
        public string? CustomerMobile { get; set; }
        public string? BatteryNo { get; set; }
        public string? Technician { get; set; }
        public string? CustomerVoice { get; set; }
        public string JobType { get; set; } = "";
        public string? JobCardNumber { get; set; }
        public DateTime? JobCardDate { get; set; }
        public string? ServiceHead { get; set; }
        public string? ServiceType { get; set; }
        public DateTime? SaleDate { get; set; }
        public DateTime? DispatchDate { get; set; }
        public string? CustomerName { get; set; }
        public string? ModelName { get; set; }
        public string? Purpose { get; set; }
        public double? CurrentKms { get; set; }
        public int? NoOfPassenger { get; set; }
        public bool? RepeatFailure { get; set; }
        public bool? ChassisModified { get; set; }
        public string? MotorNo { get; set; }
        public string? ChargerNo { get; set; }
        public string Status { get; set; } = "Pending";
        public string? Complaint { get; set; }
        public string? DetailedObservation { get; set; }
        public string? RootCause { get; set; }
        public string? CorrectiveAction { get; set; }
        public string? QuickSuggestion { get; set; }
        public string? PresentVehicleStatus { get; set; }
        public string? VehicleOffRoadReason { get; set; }
        public string? MainPartAffected { get; set; }
        public string? Remarks { get; set; }
    }

    /// <summary>Requested status change for a pending FFIR verification.</summary>
    public sealed record FFIRVerificationDecisionRequest(string Status, string? Remarks = null);
}
