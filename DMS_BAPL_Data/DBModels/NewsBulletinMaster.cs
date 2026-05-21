using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class NewsBulletinMaster
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime PublishDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public bool IsActive { get; set; }

    public byte[]? FileData { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<NewsBulletinMasterAttachment> NewsBulletinMasterAttachments { get; set; } = new List<NewsBulletinMasterAttachment>();
}
