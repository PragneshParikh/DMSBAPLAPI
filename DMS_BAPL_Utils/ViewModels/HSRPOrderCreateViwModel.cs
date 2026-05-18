using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class HSRPOrderCreateViwModel
    {
        public int? id {  get; set; }
        public string? DealerCode { get; set; }
        public string OrderNo { get; set; } = null!;

        public DateTime OrderDate { get; set; }

        public int? SupplierLedgerId { get; set; }

        public string? InvoiceNo { get; set; }

        public string? ChassisNo { get; set; }

        public string? RegNo { get; set; }

        public string? SaleBillNo { get; set; }

        public int? SaleBillDetailsId { get; set; }

        public int? CustomerLedgerId { get; set; }

        public bool? IsFrontPlate { get; set; }

        public bool? IsRearPlate { get; set; }

        public bool? IsTlpsticker { get; set; }

        
    }


    public class HsrpExternalRequestViewModel
    {
        public string VinNo { get; set; }
        public string EngineNo { get; set; }
        public string VehicleRegNo { get; set; }
        public string VehicleClass { get; set; }
        public string VehicleType { get; set; }
        public string DateOfReg { get; set; }
        public string FrontLaserCode { get; set; }
        public string RearLaserCode { get; set; }
    }
     public class HSRPInward
    {
        public int? id { get; set; }
        public string? DealerCode { get; set; }
        public string OrderNo { get; set; } = null!;

        public DateTime OrderDate { get; set; }

        public int? SupplierLedgerId { get; set; }

        public string? InvoiceNo { get; set; }

        public string? ChassisNo { get; set; }

        public string? RegNo { get; set; }

        public string? SaleBillNo { get; set; }

        public int? SaleBillDetailsId { get; set; }

        public int? CustomerLedgerId { get; set; }

        public bool? IsFrontPlate { get; set; }

        public bool? IsRearPlate { get; set; }
        public string? InwardStatus { get; set; }
        public string? InwardResponse { get; set; }

        public bool? IsTlpsticker { get; set; }
        public string? SupplierName { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerNumber { get; set; }
    }
}

