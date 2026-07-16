using DMS_BAPL_Data.DBModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DMS_BAPL_Data.Configurations;

public class ErpVehicleSaleConfiguration : IEntityTypeConfiguration<ErpVehicleSale>
{
    public void Configure(EntityTypeBuilder<ErpVehicleSale> b)
    {
        b.ToTable("ERP_VehicleSales");
        b.HasKey(x => x.Id);

        b.HasIndex(x => new { x.DealerCode, x.InvoiceNo }).IsUnique();
        b.HasIndex(x => x.InvoiceDate);
        b.HasIndex(x => x.ChassisNo);

        b.Property(x => x.FinAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.ItemRate).HasColumnType("decimal(18,2)");
        b.Property(x => x.InsuAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.RegnAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.AcsryAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.PreGstdiscAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.PostGstdisc).HasColumnType("decimal(18,2)");
        b.Property(x => x.FameIi).HasColumnType("decimal(18,2)");
        b.Property(x => x.StateFameIi).HasColumnType("decimal(18,2)");
        b.Property(x => x.Sgstper).HasColumnType("decimal(8,2)");
        b.Property(x => x.Sgstamount).HasColumnType("decimal(18,2)");
        b.Property(x => x.Cgstper).HasColumnType("decimal(8,2)");
        b.Property(x => x.Cgstamount).HasColumnType("decimal(18,2)");
        b.Property(x => x.Igstper).HasColumnType("decimal(8,2)");
        b.Property(x => x.Igstamount).HasColumnType("decimal(18,2)");
        b.Property(x => x.NetAmount).HasColumnType("decimal(18,2)");
    }
}