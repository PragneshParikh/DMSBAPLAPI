using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class ExceptionLog
{
    public int Id { get; set; }

    public string UserName { get; set; } = null!;

    public string HttpMethod { get; set; } = null!;

    public string Controller { get; set; } = null!;

    public string Action { get; set; } = null!;

    public string Path { get; set; } = null!;

    public string QueryString { get; set; } = null!;

    public string ExceptionMessage { get; set; } = null!;

    public string StackTrace { get; set; } = null!;

    public DateTime OccureAt { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
