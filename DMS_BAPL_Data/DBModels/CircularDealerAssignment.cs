using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class CircularDealerAssignment
{
    public int Id { get; set; }

    public int CircularId { get; set; }

    public string DealerCode { get; set; } = null!;

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
