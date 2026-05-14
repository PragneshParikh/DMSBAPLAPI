using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class CurrentStockReportViewModel
    {
        public int SrNo { get; set; }

        public string? DealerCode { get; set; }
        public string? DealerName { get; set; }

        public string? ModelCode { get; set; }
        public string? ModelName { get; set; }
        public string? OEMModelName { get; set; }

        public string? ColorCode { get; set; }
        public string? ColorName { get; set; }

        public string? ChassisNo { get; set; }
        public string? MotorNo { get; set; }

        public string? BatteryNo { get; set; }
        public string? BatteryNo2 { get; set; }
        public string? BatteryNo3 { get; set; }
        public string? BatteryNo4 { get; set; }
        public string? BatteryNo5 { get; set; }
        public string? BatteryNo6 { get; set; }

        public string? ChargerNo { get; set; }
        public string? ControllerNo { get; set; }
        public string? ConverterNo { get; set; }

        public string? RegisterNo { get; set; }

        public string? InvoiceNo { get; set; }

        public DateTime? DispatchDate { get; set; }
        public DateTime? ReceiveDate { get; set; }

        public string? VehicleStatus { get; set; }
        public string? StockStatus { get; set; }

        public string? Location { get; set; }
        public string? CurrentLocation { get; set; }

        public decimal PurchaseRate { get; set; }
        public decimal EstimatedSaleRate { get; set; }

        public bool IsBilled { get; set; }

        public int DaysInStock { get; set; }
    }
}
