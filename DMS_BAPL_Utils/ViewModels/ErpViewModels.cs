using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DMS_BAPL_Utils.ViewModels
{
    // One line of the payload sent to the ERP. Field names follow the ONLY
    // confirmed ERP contract available - the GET /erpreport/wcjo REPORT
    // response shape - since no POST/submit contract was provided.
    // CONFIRM the real submit endpoint's expected field names before
    // relying on this: a report-fetch response shape is not a guaranteed
    // match for a push/submit request shape.
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

        // UNCONFIRMED: WarrantyOrderGridDetail only stores LocationName
        // (display text), never a raw Loccode - so this can't be resolved
        // to LocationMaster.City without that code. Left blank.
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

        // UNCONFIRMED: only ServiceHead is stored on WarrantyOrderGridDetail,
        // not a separate ServiceType - reusing ServiceHead as a best-effort
        // stand-in. Confirm whether ERP actually wants ServiceType distinctly.
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

        // UNCONFIRMED: DealerObservation/RootCauseAnalysis live on
        // WarrantyJcclaimDetail, but are never copied onto
        // WarrantyOrderGridDetail at snapshot time - so they aren't
        // available here without an additional join back to the claim
        // detail table (fragile: would need to match by claim+part/labour,
        // since WarrantyOrderGridDetail doesn't store the WarrantyJcclaimDetailId).
        // Left blank; wire in a proper FK if these are required by ERP.
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

        // ASSUMPTION: mapped to this Warranty Invoice's own
        // InvoicePrefix+InvoiceNo/InvoiceDate. CONFIRM this is actually
        // what ERP means by "Doc No/Date" here - it could just as
        // plausibly mean something else entirely (e.g. a GST e-invoice
        // document number).
        [JsonPropertyName("Doc. No")]
        public string? DocNo { get; set; }

        [JsonPropertyName("Doc Date")]
        public string? DocDate { get; set; }

        // UNCONFIRMED
        [JsonPropertyName("Vendor PO No.")]
        public string? VendorPoNo { get; set; }

        [JsonPropertyName("Vendor PO Date")]
        public string? VendorPoDate { get; set; }

        // UNCONFIRMED: meaning of this field in the sample response is
        // unclear (appears constant per-record in the example, possibly a
        // total-row-count artifact rather than a per-line value).
        [JsonPropertyName("Total")]
        public string? Total { get; set; }
    }

    // Request body shape - GENUINELY UNCONFIRMED for a submit/push
    // endpoint. Built by analogy to the GET report's own request params
    // (vendorid, subvendorcode) plus the line data as "Value", since no
    // real submit contract was provided. Adjust field names/shape once
    // the actual endpoint is documented.
    public class ErpWarrantyClaimSubmitRequest
    {
        public int VendorId { get; set; }
        public string? SubVendorCode { get; set; } // base64, per the documented GET contract
        public List<ErpWarrantyClaimLineViewModel> Value { get; set; } = new();
    }

    // Matches the documented ERP response envelope from the GET report
    // spec ({ Valid, Description, Value }) - reused defensively here since
    // no separate submit-response schema is documented either.
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