using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class CircularMasterAttachment
{
    public int Id { get; set; }

    public int CircularId { get; set; }

    public string ContentType { get; set; } = null!;

    public string FileName { get; set; } = null!;

    public byte[] FileData { get; set; } = null!;

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual CircularMaster Circular { get; set; } = null!;
}
