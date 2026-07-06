using System;
using System.Collections.Generic;

namespace DMS_BAPL_Utils.ViewModels
{
    public class VehicleInwardReportFilterModel
    {
        public string? DealerCode { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? LocationCode { get; set; }
        public string? InvoiceNo { get; set; }
        public string? ChassisNo { get; set; }
        public string? MotorNo { get; set; }
        public string? BatteryNo { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class VehicleInwardReportViewModel
    {
        public int SrNo { get; set; }

        // NOTE: VehicleInward has only one date column (InvoiceDate, stored as DateOnly).
        // There is no separate "Receiving Date" — this mirrors InvoiceDate.
        public DateTime? ReceivingDate { get; set; }
        public DateTime? InvoiceDate { get; set; }

        public string? DealerCode { get; set; }
        public string? DealerName { get; set; }
        public string? BgInvoiceNo { get; set; }

        // NOTE: no ReceivingNo / PartyName column exists on VehicleInward — always null.
        public int? LotInspectionNo { get; set; }
        public string? PartyName { get; set; }

        public string? PurchaseReceivingLocation { get; set; }
        public string? ModelName { get; set; }

        public int Quantity { get; set; }

        public string? ChassisNo { get; set; }
        public string? MotorNo { get; set; }
        public string? Colour { get; set; }
        public int? MfgYear { get; set; }
        public string? BatteryNo { get; set; }
        public string? BatteryMake { get; set; }
        public string? BatteryCapacity { get; set; }
        public string? BatteryChemical { get; set; }
        public string? ChargerNo { get; set; }
        public string? ControllerNo { get; set; }

        public decimal Rate { get; set; }
        public decimal SubsidyAmountFame2 { get; set; }
        public decimal Sgst { get; set; }
        public decimal Cgst { get; set; }
        public decimal Igst { get; set; }
        public decimal Hst { get; set; }
    }

    public class VehicleInwardReportResponse
    {
        public int TotalRecords { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        public int TotalQuantity { get; set; }
        public decimal TotalRate { get; set; }
        public decimal TotalSubsidy { get; set; }
        public decimal TotalSgst { get; set; }
        public decimal TotalCgst { get; set; }
        public decimal TotalIgst { get; set; }
        public decimal TotalHst { get; set; }
        public decimal GrandTotal { get; set; }

        public List<VehicleInwardReportViewModel> Data { get; set; } = new();
    }
}