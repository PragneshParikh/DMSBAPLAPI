using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS_BAPL_Data.DBModels;

public partial class DmsLineOrderReport
{
    public int Id { get; set; }

    public string? DealerName { get; set; }
    public string? DealerCode { get; set; }
    public string? UniqueId { get; set; }
    public string? LocCode { get; set; }
    public DateOnly? DocDate { get; set; }
    public string? DocNo { get; set; }
    public string? DocType { get; set; }
    public DateOnly? JobDate { get; set; }
    public string? JobNo { get; set; }
    public string? BrandName { get; set; }
    public string? Model { get; set; }
    public string? JobCardType { get; set; }
    public string? PaymentMode { get; set; }
    public string? PartyName { get; set; }
    public string? PartyMobile { get; set; }
    public string? RegNo { get; set; }
    public string? VehicleType { get; set; }
    public string? ChassisNo { get; set; }
    public string? Location { get; set; }
    public string? ItemName { get; set; }
    public string? ItemDescription { get; set; }
    public string? ItemType { get; set; }
    public string? Qty { get; set; }

    public decimal? Rate { get; set; }
    public decimal? Total { get; set; }
    public decimal? SgstPer { get; set; }
    public decimal? SgstAmount { get; set; }
    public decimal? CgstPer { get; set; }
    public decimal? CgstAmount { get; set; }
    public decimal? IgstPer { get; set; }
    public decimal? IgstAmount { get; set; }
    public decimal? Discount { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? Mrp { get; set; }

    public string? DealerType { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public string? RowHash { get; set; }
    public string? UniqueKey { get; set; }

    public string? MaterialCode { get; set; }
    public DateOnly? MaterialDate { get; set; }
}