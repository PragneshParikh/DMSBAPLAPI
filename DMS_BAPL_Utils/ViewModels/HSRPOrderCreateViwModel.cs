using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class HSRPOrderCreateViwModel
    {
        public int? id { get; set; }
        public string? DealerCode { get; set; }
        public string OrderNo { get; set; } = null!;

        public DateTime OrderDate { get; set; }

        public int? SupplierLedgerId { get; set; }

        public string? InvoiceNo { get; set; }

        public string? ChassisNo { get; set; }

        public string? RegNo { get; set; }

        public DateTime? RegDate { get; set; }

        public string? SaleBillNo { get; set; }

        public int? SaleBillDetailsId { get; set; }

        public int? CustomerLedgerId { get; set; }
        public string? CustomerName { get; set; }

        public bool? IsFrontPlate { get; set; }

        public bool? IsRearPlate { get; set; }

        public bool? IsTlpsticker { get; set; }

        public string? MotorNo { get; set; }

    }


    public class HsrpExternalRequestViewModel
    {
        public string DealerCode { get; set; }
        public int vendorid { get; set; }
        public string? OrderNumber { get; set; }
        public string HSRPOrderType { get; set; }
        public string OrderDate { get; set; }
        public string ChassisNumber { get; set; }
        public string EngineNo { get; set; }
        public string RegistrationNo { get; set; }
        public string CustomerName { get; set; }
        public string VehicleClass { get; set; }
        public string VehicleType { get; set; }
        public string Fuel { get; set; }
        public string RegDate { get; set; }
        public string? FrontPlate { get; set; }
        public string? RearPlate { get; set; }
        public string TLP { get; set; }

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

    public class HSRPLoginResponse
    {
        public bool Valid { get; set; }
        public string Description { get; set; }
        public List<HSRPLoginValue> Value { get; set; }
    }

    public class HSRPLoginValue
    {
        public string accesstoken { get; set; }
        public string LoginEmail_Idno { get; set; }
        public string loginemail { get; set; }
        public string vendorname { get; set; }
        public string vendorcode { get; set; }
    }

    public class HSRPLoginResponseViewModel
    {
        public string STATUS { get; set; }
        public string MESSAGE { get; set; }
        public HSRPTokenValue Value { get; set; }
    }

    public class HSRPTokenValue
    {
        public string AccessToken { get; set; }
        public DateTime Expiration { get; set; }
    }

    public class HsrpApiResponse
    {
        public string STATUS { get; set; }
        public string MESSAGE { get; set; }
        public HsrpApiValue Value { get; set; }
    }

    public class HsrpApiValue
    {
        public string ChassisNumber { get; set; }
    }

    public class HSRPDispatchRequest
    {
        public List<HSRPDispatchItem> Data { get; set; }
    }
    public class HSRPDispatchItem
    {
        public string? Ver { get; set; }
        public int VendorId { get; set; }
        public string DealerCode { get; set; }
        public string DispatchNumber { get; set; }
        public string DispatchDate { get; set; }
        public string OrderNumber { get; set; }
        public int OrderId { get; set; }
        public string RegistrationNumber { get; set; }
        public string FrontLaserCode { get; set; }
        public string RearLaserCode { get; set; }
    }
    public class HSRPDispatchResponse
    {
        public bool Valid { get; set; }
        public string Description { get; set; }
        public List<HSRPDispatchResponseValue> Value { get; set; }
    }

    public class HSRPDispatchResponseValue
    {
        public string Msg { get; set; }
        public string StatusCode { get; set; }
        public string ResponseStatus { get; set; }
    }

    public class HSRPInwardRequestViewModel
    {
        public string Ver { get; set; } = "1.1";
        public int VendorId { get; set; }
        public string DealerCode { get; set; }
        public string DCNumber { get; set; }
        public string RegistrationNo { get; set; }
        public string ChassisNumber { get; set; }
        public int FrontPlate { get; set; }
        public int RearPlate { get; set; }
        public string ReceivingDate { get; set; }
        public string ReceivingMessage { get; set; } = "RECEIVED";
        public int OrderID { get; set; }
    }
    public class HSRPInwardResponseViewModel
    {
        public string STATUS { get; set; }
        public string MESSAGE { get; set; }
    }
}

