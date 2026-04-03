using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class NumberSequence
{
    public int Id { get; set; }

    public string SequenceCode { get; set; } = null!;

    public string SequenceName { get; set; } = null!;

    public string Format { get; set; } = null!;

    public int NextNo { get; set; }

    public int Increment { get; set; }

    public string DealerCode { get; set; } = null!;

    public int Year { get; set; }

    public bool IsActive { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }
}
