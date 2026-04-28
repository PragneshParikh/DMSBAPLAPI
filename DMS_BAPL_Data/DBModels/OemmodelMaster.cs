using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class OemmodelMaster
{
    public int Id { get; set; }

    public string ModelName { get; set; } = null!;

    public string? ModelShortName { get; set; }

    public bool IsActive { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<ExtendedBatteryWarranty> ExtendedBatteryWarranties { get; set; } = new List<ExtendedBatteryWarranty>();

    public virtual ICollection<Form22Master> Form22Masters { get; set; } = new List<Form22Master>();

    public virtual ICollection<ModelwiseServiceSchedule> ModelwiseServiceSchedules { get; set; } = new List<ModelwiseServiceSchedule>();

    public virtual ICollection<OemmodelWarranty> OemmodelWarranties { get; set; } = new List<OemmodelWarranty>();
}
