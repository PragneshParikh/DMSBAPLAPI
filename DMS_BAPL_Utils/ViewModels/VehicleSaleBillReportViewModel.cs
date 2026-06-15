using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class VehicleSaleBillReportViewModel
    {

        // Col 1
        public int SrNo { get; set; }

        // Col 2 — Bill No
        public string? BillNo { get; set; }

        // Col 3 — Bill Date
        public DateTime BillDate { get; set; }

        // Col 4 — Booking Id
        public string? BookingId { get; set; }

        // Col 5 — Party Name  (CustomerName)
        public string? PartyName { get; set; }

        // Col 6 — Contact Person  (BillingName)
        public string? ContactPerson { get; set; }

        // Col 7 — Party Address  (LedgerMaster.Address)
        public string? PartyAddress { get; set; }

        // Col 8 — Location  (LocationMaster.Locname)
        public string? Location { get; set; }

        // Col 9 — Party Mobile  (LedgerMaster.MobileNumber)
        public string? PartyMobile { get; set; }

        // Col 10 — Party Email  (LedgerMaster.EMail)
        public string? PartyEmail { get; set; }

        // Col 11 — Executive Name  (SalesExecutive)
        public string? ExecutiveName { get; set; }

        // Col 12 — GSTN No  (LedgerMaster.GSTNo or similar)
        public string? GstnNo { get; set; }

        // Col 13 — Item Model  (ItemMaster.Itemname)
        public string? ItemModel { get; set; }

        // Col 14 — Description  (ItemMaster.Itemdesc)
        public string? Description { get; set; }

        // Col 15 — OEM Model Name  (ItemMaster.Oemmodelname)
        public string? OemModelName { get; set; }

        // Col 16 — HSNSAC Code  (ItemMaster.Hsncode)
        public string? HsnSacCode { get; set; }

        // Col 17 — Sales Type  (VehicleSaleBillHeader.SaleType)
        public string? SalesType { get; set; }

        // Col 18 — Item Rate
        public decimal ItemRate { get; set; }

        // Col 19 — Insu. Amnt  (InsuranceAmount)
        public decimal InsuAmnt { get; set; }

        // Col 20 — REGN. AMNT  (RegAmount)
        public decimal RegnAmnt { get; set; }

        // Col 21 — ACSRY AMNT  (Accessory Amount — 0 unless you have a field)
        public decimal AcsryAmnt { get; set; }

        // Col 22 — Fin. Amnt  (Finance Amount — 0 placeholder)
        public decimal FinAmnt { get; set; }

        // Col 23 — Processing Fee  (0 placeholder)
        public decimal ProcessingFee { get; set; }

        // Col 24 — Hyp Amnt  (Hypothecation Amount — 0 placeholder)
        public decimal HypAmnt { get; set; }

        // Col 25 — Other Charge  (0 placeholder)
        public decimal OtherCharge { get; set; }

        // Col 26 — SmartCard Amnt  (0 placeholder)
        public decimal SmartCardAmnt { get; set; }

        // Col 27 — PostGST Disc Amnt  (PostGstDisc)
        public decimal PostGstDiscAmnt { get; set; }

        // Col 28 — PreGST Disc Amnt  (PreGstDiscount)
        public decimal PreGstDiscAmnt { get; set; }

        // Col 29 — SGST%
        public decimal Sgstper { get; set; }

        // Col 30 — SGST Amnt
        public decimal Sgstamnt { get; set; }

        // Col 31 — CGST%
        public decimal Cgstper { get; set; }

        // Col 32 — CGST Amnt
        public decimal Cgstamnt { get; set; }

        // Col 33 — IGST%
        public decimal Igstper { get; set; }

        // Col 34 — IGST Amnt
        public decimal Igstamnt { get; set; }

        // Col 35 — Subsidy Amnt  (FameIi / ItemMaster.Fame2amount)
        public decimal SubsidyAmnt { get; set; }

        // Col 36 — StateSubSidy Amnt  (0 placeholder — no DB field yet)
        public decimal StateSubsidyAmnt { get; set; }

        // Col 37 — NumPlate Amnt  (0 placeholder)
        public decimal NumPlateAmnt { get; set; }

        // Col 38 — Handling Charges  (0 placeholder)
        public decimal HandlingCharges { get; set; }

        // Col 39 — Net Amnt  (FinalAmount)
        public decimal NetAmnt { get; set; }

        // Col 40 — Reg No.
        public string? RegNo { get; set; }

        // Col 41 — Chasis No.
        public string? ChasisNo { get; set; }

        // Col 42 — Color  (ColorMaster.Colorname via VehicleInward.ColrCode)
        public string? Color { get; set; }

        // Col 43 — Financer Name  (LedgerMaster.LedgerName for Financier)
        public string? FinancerName { get; set; }

        // ── Extra fields used internally / for dealer restriction ─────────
        public string? DealerCode { get; set; }
        public string? Status { get; set; }
        public string? InvoiceNo { get; set; }
    }
}
