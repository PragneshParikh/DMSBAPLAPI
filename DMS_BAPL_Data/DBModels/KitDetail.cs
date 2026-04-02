using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class KitDetail
{
    public int Id { get; set; }

    public int KitHeaderId { get; set; }

    public int ItemId { get; set; }

    public int Quantity { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ItemMaster Item { get; set; } = null!;

    public virtual KitHeader KitHeader { get; set; } = null!;
}
