using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class LotinspectionHeader
{
    public int Id { get; set; }

    public string DealerCode { get; set; } = null!;

    public string LocCode { get; set; } = null!;

    public string InvoiceNo { get; set; } = null!;

    public DateOnly InvoiceDate { get; set; }

    public int LotNo { get; set; }

    public DateOnly? ArrivalDate { get; set; }

    public TimeOnly? ArrivalTime { get; set; }

    public string? LrNo { get; set; }

    public DateOnly? LrDate { get; set; }

    public string? TruckNo { get; set; }

    public string? TransporterName { get; set; }

    public string? DriverName { get; set; }

    public string? DriverContact { get; set; }

    public string? CommonRemarks { get; set; }

    public string? VehicleFasteningBracket { get; set; }

    public string? PlasticCover { get; set; }

    public string? SupervisorName { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateOnly CreatedDate { get; set; }

    public string? UpdateBy { get; set; }

    public DateOnly? UpdatedDate { get; set; }

    public virtual ICollection<LotinspectionDetail> LotinspectionDetails { get; set; } = new List<LotinspectionDetail>();
}
