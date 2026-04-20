using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class Apikey
{
    public int Id { get; set; }

    public string Apikey1 { get; set; } = null!;

    public bool IsActive { get; set; }
}
