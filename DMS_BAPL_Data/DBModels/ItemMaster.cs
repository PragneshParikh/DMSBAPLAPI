using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class ItemMaster
{
    public int Id { get; set; }

    public string? DealerCode { get; set; }

    public int Itemtype { get; set; }

    public string Itemname { get; set; } = null!;

    public string Itemcode { get; set; } = null!;

    public string Itemdesc { get; set; } = null!;

    public bool Status { get; set; }

    public string Hsncode { get; set; } = null!;

    public decimal Dlrprice { get; set; }

    public decimal Custprice { get; set; }

    public int Moq { get; set; }

    public int Boq { get; set; }

    public decimal Sgst { get; set; }

    public decimal Cgst { get; set; }

    public decimal Igst { get; set; }

    public decimal Ugst { get; set; }

    public int Grpidno { get; set; }

    public decimal Ipurrate { get; set; }

    public bool Iselectric { get; set; }

    public int Vehtype { get; set; }

    public int Noofbatteries { get; set; }

    public string? Colorcode { get; set; }

    public int Rrgitemidno { get; set; }

    public int Itemcc { get; set; }

    public int Batterytypeidno { get; set; }

    public decimal Fame2amount { get; set; }

    public string? Compcode { get; set; }

    public string? Displayname { get; set; }

    public string? Oemmodelname { get; set; }

    public int? HsncodeId { get; set; }

    public int? MinBillQty { get; set; }

    public int? MinOrderQty { get; set; }

    public int? WarrantyPeriod { get; set; }

    public int? WarrantyDurationType { get; set; }

    public int? WarrantyKms { get; set; }

    public bool? IsWarrantyApproval { get; set; }

    public decimal? Vorrate { get; set; }

    public bool? IsVor { get; set; }

    public string? Remarks { get; set; }

    public bool? IsExempted { get; set; }

    public bool? IsToolkitFirstAid { get; set; }

    public bool? IsStockRequired { get; set; }

    public bool? IsHelmet { get; set; }

    public bool? IsInventory { get; set; }

    public bool? IsInEligibleInput { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? Uom { get; set; }

    public virtual HsncodeMaster? HsncodeNavigation { get; set; }

    public virtual ICollection<KitDetail> KitDetails { get; set; } = new List<KitDetail>();

    public virtual ICollection<MaterialTransfer> MaterialTransfers { get; set; } = new List<MaterialTransfer>();

    public virtual ICollection<RepairBillDetail> RepairBillDetails { get; set; } = new List<RepairBillDetail>();
}
