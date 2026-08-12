// PartDispWarrantyCreateViewModel.cs
using System;
using System.Text.Json.Serialization;

namespace DMS_BAPL_Utils.ViewModels
{
    public class PartDispWarrantyCreateViewModel
    {
        [JsonPropertyName("invoicedate")]
        public DateTime? Invoicedate { get; set; }

        [JsonPropertyName("invoiceno")]
        public string? Invoiceno { get; set; }

        [JsonPropertyName("invoicetype")]
        public string? Invoicetype { get; set; }

        [JsonPropertyName("chassisnumber")]
        public string? Chassisnumber { get; set; }

        [JsonPropertyName("itemcode")]
        public string? Itemcode { get; set; }

        [JsonPropertyName("serialno")]
        public string? Serialno { get; set; }

        //[JsonPropertyName("vendorid")]
        //public int? Vendorid { get; set; }

        [JsonPropertyName("dealercode")]
        public string? Dealercode { get; set; }

        [JsonPropertyName("devicetype")]
        public string? Devicetype { get; set; }

        [JsonPropertyName("itemqty")]
        public int? Itemqty { get; set; }

        [JsonPropertyName("lotno")]
        public string? Lotno { get; set; }

        [JsonPropertyName("mfgdate")]
        public DateTime? Mfgdate { get; set; }

        [JsonPropertyName("invoiceitemcode")]
        public string? Invoiceitemcode { get; set; }

        [JsonPropertyName("lineno")]
        public int? Lineno { get; set; }

        [JsonPropertyName("invoiceAmt")]
        public decimal? InvoiceAmt { get; set; }
    }

    public class PartDispWarrantyBulkCreateViewModel
    {
        [JsonPropertyName("data")]
        public System.Collections.Generic.List<PartDispWarrantyCreateViewModel> Data { get; set; } = new();
    }
}