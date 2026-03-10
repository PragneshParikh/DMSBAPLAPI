using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class Apitracking
{
    public int Id { get; set; }

    public string Endpoint { get; set; } = null!;

    public DateTime Dateofhit { get; set; }

    public string? Payload { get; set; }

    public string? Status { get; set; }

    public string? Response { get; set; }
}
