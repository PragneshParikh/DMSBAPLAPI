using System;
using System.Collections.Generic;

namespace DMS_BAPL_Data.DBModels;

public partial class PurchaseOrder
{
    public int Id { get; set; }

    public string Ponumber { get; set; } = null!;

    public DateTime? PurchaseDate { get; set; }

    public string? OrderType { get; set; }

    public string? ReferenceNo { get; set; }

    public string? TestCertificate { get; set; }

    public string? ConsigneeCode { get; set; }

    public string? CustomerCode { get; set; }

    public decimal? Amount { get; set; }

    public string? FameIiflag { get; set; }

    public string? TransactionType { get; set; }

    public bool Status { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
