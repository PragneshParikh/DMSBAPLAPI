using System;
using System.Text.Json.Serialization;

namespace DMS_BAPL_Utils.ViewModels
{
    public class PartDispatchCreateViewModel
    {
        [JsonPropertyName("invoice_date")]
        public DateTime? InvoiceDate { get; set; }

        [JsonPropertyName("invoice_no")]
        public string? InvoiceNo { get; set; }

        [JsonPropertyName("part_no")]
        public string? PartNo { get; set; }

        [JsonPropertyName("item_idno")]
        public int? ItemIdno { get; set; }

        [JsonPropertyName("item_hsncode")]
        public string? ItemHsncode { get; set; }

        [JsonPropertyName("item_rate")]
        public decimal? ItemRate { get; set; }

        [JsonPropertyName("item_mrp")]
        public decimal? ItemMrp { get; set; }

        [JsonPropertyName("item_qty")]
        public int? ItemQty { get; set; }

        [JsonPropertyName("sgst")]
        public decimal? Sgst { get; set; }

        [JsonPropertyName("cgst")]
        public decimal? Cgst { get; set; }

        [JsonPropertyName("igst")]
        public decimal? Igst { get; set; }

        [JsonPropertyName("ugst")]
        public decimal? Ugst { get; set; }

        [JsonPropertyName("item_disc")]
        public decimal? ItemDisc { get; set; }

        [JsonPropertyName("discount_type")]
        public string? DiscountType { get; set; }

        [JsonPropertyName("loc_code")]
        public string? LocCode { get; set; }

        //[JsonPropertyName("vendor_idno")]
        //public int? VendorIdno { get; set; }

        [JsonPropertyName("isAccepted")]
        public bool? IsAccepted { get; set; }

        [JsonPropertyName("dealer_code")]
        public string? DealerCode { get; set; }
    }
}