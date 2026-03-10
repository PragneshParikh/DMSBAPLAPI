using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DMS_BAPL_Data.DBModels;

public partial class BapldmsvadContext : DbContext
{
    public BapldmsvadContext()
    {
    }

    public BapldmsvadContext(DbContextOptions<BapldmsvadContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Apitracking> Apitrackings { get; set; }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<ColorMaster> ColorMasters { get; set; }

    public virtual DbSet<DealerMaster> DealerMasters { get; set; }

    public virtual DbSet<ItemMaster> ItemMasters { get; set; }

    public virtual DbSet<LmsleadMaster> LmsleadMasters { get; set; }

    public virtual DbSet<LocationMaster> LocationMasters { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Apitracking>(entity =>
        {
            entity.ToTable("APITracking");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Dateofhit)
                .HasColumnType("datetime")
                .HasColumnName("dateofhit");
            entity.Property(e => e.Endpoint)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("endpoint");
            entity.Property(e => e.Payload)
                .HasMaxLength(5000)
                .IsUnicode(false)
                .HasColumnName("payload");
            entity.Property(e => e.Response)
                .HasMaxLength(5000)
                .IsUnicode(false)
                .HasColumnName("response");
            entity.Property(e => e.Status)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("status");
        });

        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");

            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<ColorMaster>(entity =>
        {
            entity.ToTable("ColorMaster");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Colorcode)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("colorcode");
            entity.Property(e => e.Colorname)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("colorname");
            entity.Property(e => e.CreatedBy).HasColumnName("createdBy");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("createdDate");
            entity.Property(e => e.Rrgcoloridno).HasColumnName("rrgcoloridno");
            entity.Property(e => e.UpdatedBy).HasColumnName("updatedBy");
            entity.Property(e => e.UpdatedDate)
                .HasColumnType("datetime")
                .HasColumnName("updatedDate");
        });

        modelBuilder.Entity<DealerMaster>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DealerMa__3213E83FE6BF4BFD");

            entity.ToTable("DealerMaster");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Adress1)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("adress1");
            entity.Property(e => e.Adress2)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("adress2");
            entity.Property(e => e.Areaofficeid).HasColumnName("areaofficeid");
            entity.Property(e => e.B2b).HasColumnName("b2b");
            entity.Property(e => e.BrandName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("brand_name");
            entity.Property(e => e.CeditLimit)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("cedit_limit");
            entity.Property(e => e.CinNo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("cin_no");
            entity.Property(e => e.City)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("city");
            entity.Property(e => e.CompImage)
                .HasMaxLength(700)
                .IsUnicode(false)
                .HasColumnName("comp_image");
            entity.Property(e => e.Compcode)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("compcode");
            entity.Property(e => e.CompgstinNo)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("compgstin_no");
            entity.Property(e => e.Compname)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("compname");
            entity.Property(e => e.Contactperson)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("contactperson");
            entity.Property(e => e.CreatedBy).HasColumnName("createdBy");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("createdDate");
            entity.Property(e => e.Dealercode)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("dealercode");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.FameiiCode)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("fameii_code");
            entity.Property(e => e.IsTcs).HasColumnName("is_tcs");
            entity.Property(e => e.Mobile)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("mobile");
            entity.Property(e => e.Pan)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("pan");
            entity.Property(e => e.PhoneOff)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("phone_off");
            entity.Property(e => e.Pin)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("pin");
            entity.Property(e => e.RegAddress)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("reg_address");
            entity.Property(e => e.RegDate)
                .HasColumnType("datetime")
                .HasColumnName("reg_date");
            entity.Property(e => e.State)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("state");
            entity.Property(e => e.TcsPercent)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("tcs_percent");
            entity.Property(e => e.TradCert)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("trad_cert");
            entity.Property(e => e.UpdatedBy).HasColumnName("updatedBy");
            entity.Property(e => e.UpdatedDate)
                .HasColumnType("datetime")
                .HasColumnName("updatedDate");
            entity.Property(e => e.VatNo)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("vat_no");
        });

        modelBuilder.Entity<ItemMaster>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ItemMast__3213E83FA7C38EFE");

            entity.ToTable("ItemMaster");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Batterytypeidno).HasColumnName("batterytypeidno");
            entity.Property(e => e.Boq).HasColumnName("boq");
            entity.Property(e => e.Cgst)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("cgst");
            entity.Property(e => e.Colorcode)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("colorcode");
            entity.Property(e => e.Compcode)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("compcode");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("createdBy");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("createdDate");
            entity.Property(e => e.Custprice)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("custprice");
            entity.Property(e => e.Displayname)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("displayname");
            entity.Property(e => e.Dlrprice)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("dlrprice");
            entity.Property(e => e.Fame2amount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("fame2amount");
            entity.Property(e => e.Grpidno).HasColumnName("grpidno");
            entity.Property(e => e.Hsncode)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("hsncode");
            entity.Property(e => e.Igst)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("igst");
            entity.Property(e => e.Ipurrate)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("ipurrate");
            entity.Property(e => e.Iselectric).HasColumnName("iselectric");
            entity.Property(e => e.Itemcc).HasColumnName("itemcc");
            entity.Property(e => e.Itemcode)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("itemcode");
            entity.Property(e => e.Itemdesc)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("itemdesc");
            entity.Property(e => e.Itemname)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("itemname");
            entity.Property(e => e.Itemtype).HasColumnName("itemtype");
            entity.Property(e => e.Moq).HasColumnName("moq");
            entity.Property(e => e.Noofbatteries).HasColumnName("noofbatteries");
            entity.Property(e => e.Oemmodelname)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("oemmodelname");
            entity.Property(e => e.Rrgitemidno).HasColumnName("rrgitemidno");
            entity.Property(e => e.Sgst)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("sgst");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Ugst)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("ugst");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("updatedBy");
            entity.Property(e => e.UpdatedDate)
                .HasColumnType("datetime")
                .HasColumnName("updatedDate");
            entity.Property(e => e.Vehtype).HasColumnName("vehtype");
        });

        modelBuilder.Entity<LmsleadMaster>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LMSLeadM__3213E83F94CA5A34");

            entity.ToTable("LMSLeadMaster");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Area)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("area");
            entity.Property(e => e.Brancharea)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("brancharea");
            entity.Property(e => e.Branchpin).HasColumnName("branchpin");
            entity.Property(e => e.City)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("city");
            entity.Property(e => e.Color)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("color");
            entity.Property(e => e.ColorId).HasColumnName("colorId");
            entity.Property(e => e.Company)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("company");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("createdBy");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("createdDate");
            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");
            entity.Property(e => e.DealerId).HasColumnName("dealerId");
            entity.Property(e => e.Dealercode)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("dealercode");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Leadid).HasColumnName("leadid");
            entity.Property(e => e.Mobile)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("mobile");
            entity.Property(e => e.Model)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("model");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.Pincode).HasColumnName("pincode");
            entity.Property(e => e.Productcode).HasColumnName("productcode");
            entity.Property(e => e.Sourceapp)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("sourceapp");
            entity.Property(e => e.Time).HasColumnName("time");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("updatedBy");
            entity.Property(e => e.UpdatedDate)
                .HasColumnType("datetime")
                .HasColumnName("updatedDate");
            entity.Property(e => e.Variant)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("variant");

            entity.HasOne(d => d.ColorNavigation).WithMany(p => p.LmsleadMasters)
                .HasForeignKey(d => d.ColorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKColorMaster");

            entity.HasOne(d => d.Dealer).WithMany(p => p.LmsleadMasters)
                .HasForeignKey(d => d.DealerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FKDealerMaster");
        });

        modelBuilder.Entity<LocationMaster>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Location__3213E83F3DF6EF21");

            entity.ToTable("LocationMaster");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Acntidno).HasColumnName("acntidno");
            entity.Property(e => e.Action)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("action");
            entity.Property(e => e.Active)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("Y")
                .IsFixedLength()
                .HasColumnName("active");
            entity.Property(e => e.Add1)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("add1");
            entity.Property(e => e.Add2)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("add2");
            entity.Property(e => e.City)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("city");
            entity.Property(e => e.Compid).HasColumnName("compid");
            entity.Property(e => e.Contperemail1)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("contperemail1");
            entity.Property(e => e.Contperemail2)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("contperemail2");
            entity.Property(e => e.Contpermob1)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("contpermob1");
            entity.Property(e => e.Contpermob2)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("contpermob2");
            entity.Property(e => e.Contpername1)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("contpername1");
            entity.Property(e => e.Contpername2)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("contpername2");
            entity.Property(e => e.CreatedBy).HasColumnName("createdBy");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("createdDate");
            entity.Property(e => e.Dealercode)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("dealercode");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Formtype)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("formtype");
            entity.Property(e => e.Gstinno)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("gstinno");
            entity.Property(e => e.Lineno)
                .HasDefaultValue(0)
                .HasColumnName("lineno");
            entity.Property(e => e.Locareaidno).HasColumnName("locareaidno");
            entity.Property(e => e.Loccode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("loccode");
            entity.Property(e => e.Locname)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("locname");
            entity.Property(e => e.Mobileno)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("mobileno");
            entity.Property(e => e.Pincode)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("pincode");
            entity.Property(e => e.Rrglocationidno).HasColumnName("rrglocationidno");
            entity.Property(e => e.State)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("state");
            entity.Property(e => e.UpdateBy).HasColumnName("updateBy");
            entity.Property(e => e.UpdatedDate)
                .HasColumnType("datetime")
                .HasColumnName("updatedDate");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
