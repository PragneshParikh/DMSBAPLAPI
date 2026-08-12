using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class EbwInvoiceSaveViewModel
    {
        public int Id { get; set; }
        public string DealerCode { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string PrefixNo { get; set; }
        public int? BillNo { get; set; }
        public string LocationCode { get; set; }
        public string BillType { get; set; }
        public int? CashAccountId { get; set; }
        public int SchemeId { get; set; }
        public string SchemeName { get; set; }
        public string ChassisNo { get; set; }
        public string SoldByDealerCode { get; set; }
        public DateTime? ChassisSaleDate { get; set; }
        public DateTime? ValidityExpiryDate { get; set; }
        public string PartyName { get; set; }
        public string PartyMobile { get; set; }
        public string PartyAddress { get; set; }
        public string PartyCity { get; set; }
        public string PartyPincode { get; set; }
        public string PartyState { get; set; }
        public string DealerState { get; set; }
        public bool IsInterstate { get; set; }
        public string SerialNo { get; set; }
        public string ItemCode { get; set; }
        public decimal PartsAmount { get; set; }
        public decimal NetAmount { get; set; }
        public string Remarks { get; set; }
        public List<EbwInvoiceDetailViewModel> Items { get; set; } = new();
    }
}
