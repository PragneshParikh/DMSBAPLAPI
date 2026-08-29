using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DMS_BAPL_Utils.ViewModels
{

    public class ErpWarrantyClaimLineViewModel
    {
        [JsonPropertyName("Sl.No")]
        public string SlNo { get; set; } = "";

        [JsonPropertyName("Dealer Name")]
        public string? DealerName { get; set; }

        [JsonPropertyName("Dealer Code")]
        public string? DealerCode { get; set; }

        [JsonPropertyName("Location")]
        public string? Location { get; set; }

        [JsonPropertyName("Loc. City")]
        public string? LocationCity { get; set; }

        [JsonPropertyName("Job No")]
        public string? JobNo { get; set; }

        [JsonPropertyName("Job Date")]
        public string? JobDate { get; set; }

        [JsonPropertyName("Claim No")]
        public string? ClaimNo { get; set; }

        [JsonPropertyName("Claim Date")]
        public string? ClaimDate { get; set; }

        [JsonPropertyName("KMS")]
        public string? Kms { get; set; }

        [JsonPropertyName("Vehicle Sale Date")]
        public string? VehicleSaleDate { get; set; }

        [JsonPropertyName("Part Failure Date")]
        public string? PartFailureDate { get; set; }

        [JsonPropertyName("Service Type")]
        public string? ServiceType { get; set; }

        [JsonPropertyName("Chassis No")]
        public string? ChassisNo { get; set; }

        [JsonPropertyName("Model Name")]
        public string? ModelName { get; set; }

        // UNCONFIRMED: no source field exists for this anywhere in the
        // current schema.
        [JsonPropertyName("Variants")]
        public string? Variants { get; set; }

        [JsonPropertyName("Part Code")]
        public string? PartCode { get; set; }

        [JsonPropertyName("Part Name")]
        public string? PartName { get; set; }

        [JsonPropertyName("Qty")]
        public string? Qty { get; set; }

        [JsonPropertyName("Rate")]
        public string? Rate { get; set; }

        [JsonPropertyName("CGST%")]
        public string? CgstPercent { get; set; }

        [JsonPropertyName("CGST Amnt")]
        public string? CgstAmount { get; set; }

        [JsonPropertyName("SGST%")]
        public string? SgstPercent { get; set; }

        [JsonPropertyName("SGST Amnt")]
        public string? SgstAmount { get; set; }

        [JsonPropertyName("IGST%")]
        public string? IgstPercent { get; set; }

        [JsonPropertyName("IGST Amnt")]
        public string? IgstAmount { get; set; }

        [JsonPropertyName("Amount")]
        public string? Amount { get; set; }

        [JsonPropertyName("Dealer Observation")]
        public string? DealerObservation { get; set; }

        [JsonPropertyName("RCA")]
        public string? Rca { get; set; }

        // UNCONFIRMED
        [JsonPropertyName("Invoice_RefNo")]
        public string? InvoiceRefNo { get; set; }

        [JsonPropertyName("Invoice No")]
        public string? InvoiceNo { get; set; }

        [JsonPropertyName("Invoice Date")]
        public string? InvoiceDate { get; set; }

        [JsonPropertyName("Doc. No")]
        public string? DocNo { get; set; }

        [JsonPropertyName("Doc Date")]
        public string? DocDate { get; set; }

        // UNCONFIRMED
        [JsonPropertyName("Vendor PO No.")]
        public string? VendorPoNo { get; set; }

        [JsonPropertyName("Vendor PO Date")]
        public string? VendorPoDate { get; set; }
        
        [JsonPropertyName("Total")]
        public string? Total { get; set; }
        public int UniqueId { get; set; }
    }


    public class ErpWarrantyClaimSubmitRequest
    {
        public int VendorId { get; set; }
        public string? SubVendorCode { get; set; }
        public List<ErpWarrantyClaimLineViewModel> Value { get; set; } = new();
    }


    public class ErpApiResponse<T>
    {
        public bool Valid { get; set; }
        public string? Description { get; set; }
        public List<T> Value { get; set; } = new();
    }

    public class ErpSubmitResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int LinesSent { get; set; }
    }
}