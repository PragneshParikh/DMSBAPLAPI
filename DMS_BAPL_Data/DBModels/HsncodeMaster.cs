using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class HsncodeMaster
{
    public int Id { get; set; }

    public string Hsncode { get; set; } = null!;

    public string? Description { get; set; }

    public string Type { get; set; } = null!;


    public string CreatedBy { get; set; } = null!;
    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<ItemMaster> ItemMasters { get; set; } = new List<ItemMaster>();
}
