using System;
using System.Collections.Generic;

namespace DMS_BAPL_Utils.ViewModels
{
    public class D2DReportFilterModel
    {
        public string? DealerCode { get; set; }
        public string? LocationCode { get; set; }
        public string? ChassisNo { get; set; }
        public string? MotorNo { get; set; }
        public string? BatteryNo { get; set; }
        public string? ChargerNo { get; set; }
        public string? ControllerNo { get; set; }
        public string? StockStatus { get; set; }     // NEW
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Search { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class D2DReportViewModel
    {
        public int SrNo { get; set; }
        public DateTime? ReceivingDate { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string? DealerCode { get; set; }
        public string? DealerName { get; set; }
        public string? DealerCity { get; set; }
        public string? DealerState { get; set; }          // NEW - To Dealer State
        public string? FromDealerCode { get; set; }       // NEW
        public string? FromDealerName { get; set; }       // NEW
        public string? FromDealerCity { get; set; }       // NEW
        public string? FromDealerState { get; set; }      // NEW
        public string? BgInvoiceNo { get; set; }
        public string? LocationCode { get; set; }
        public string? PurchaseReceivingLocation { get; set; }
        public string? LocationCity { get; set; }
        public string? ModelCode { get; set; }
        public string? ModelName { get; set; }
        public string? OemModelName { get; set; }
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
        public string? StockStatus { get; set; }
        public bool IsD2D { get; set; }
    }

    public class D2DReportResponse
    {
        public int TotalRecords { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public List<D2DReportViewModel> Data { get; set; } = new();
    }
}