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

    public virtual DbSet<AggregateTaxCode> AggregateTaxCodes { get; set; }

    public virtual DbSet<Apikey> Apikeys { get; set; }

    public virtual DbSet<Apitracking> Apitrackings { get; set; }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<BatteryCapacityMaster> BatteryCapacityMasters { get; set; }

    public virtual DbSet<ColorMaster> ColorMasters { get; set; }

    public virtual DbSet<DealerMaster> DealerMasters { get; set; }

    public virtual DbSet<ExceptionLog> ExceptionLogs { get; set; }

    public virtual DbSet<Form22Master> Form22Masters { get; set; }

    public virtual DbSet<HsncodeMaster> HsncodeMasters { get; set; }

    public virtual DbSet<HsnwiseTaxCode> HsnwiseTaxCodes { get; set; }

    public virtual DbSet<ItemMaster> ItemMasters { get; set; }

    public virtual DbSet<LedgerMaster> LedgerMasters { get; set; }

    public virtual DbSet<LmsleadMaster> LmsleadMasters { get; set; }

    public virtual DbSet<LocationMaster> LocationMasters { get; set; }

    public virtual DbSet<LotinspectionDetail> LotinspectionDetails { get; set; }

    public virtual DbSet<LotinspectionHeader> LotinspectionHeaders { get; set; }

    public virtual DbSet<MenuMaster> MenuMasters { get; set; }

    public virtual DbSet<OemmodelMaster> OemmodelMasters { get; set; }

    public virtual DbSet<ParameterMasterTable> ParameterMasterTables { get; set; }

    public virtual DbSet<PurchaseOrder> PurchaseOrders { get; set; }

    public virtual DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }

    public virtual DbSet<ReceiptEntry> ReceiptEntries { get; set; }

    public virtual DbSet<RoleWiseMenuRight> RoleWiseMenuRights { get; set; }

    public virtual DbSet<TaxCodeMaster> TaxCodeMasters { get; set; }

    public virtual DbSet<TaxDetail> TaxDetails { get; set; }

    public virtual DbSet<VehicleDispatch> VehicleDispatches { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=tcp:bapldmsvad01.database.windows.net,1433;Initial Catalog=BAPLDMSvad;User ID=bapladmin;Password=$@plDMS_v@d1205;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AggregateTaxCode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Aggregat__3214EC07F8C7B74F");

            entity.ToTable("AggregateTaxCode");

            entity.Property(e => e.AtaxCode)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("ATaxCode");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Description)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.TaxCode)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TaxRate).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<Apikey>(entity =>
        {
            entity.ToTable("APIKeys");

            entity.Property(e => e.Apikey1)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("APIKey");
        });

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
                .IsUnicode(false)
                .HasColumnName("payload");
            entity.Property(e => e.Response)
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

        modelBuilder.Entity<BatteryCapacityMaster>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BatteryC__3214EC07BF68DD87");

            entity.ToTable("BatteryCapacityMaster");

            entity.Property(e => e.BatteryCapacity).HasMaxLength(50);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<ColorMaster>(entity =>
        {
            entity.ToTable("ColorMaster");

            entity.HasIndex(e => e.Colorname, "UQ_ColorMaster_colorname").IsUnique();

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
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .HasColumnName("createdBy");
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
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100)
                .HasColumnName("updatedBy");
            entity.Property(e => e.UpdatedDate)
                .HasColumnType("datetime")
                .HasColumnName("updatedDate");
            entity.Property(e => e.VatNo)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("vat_no");
        });

        modelBuilder.Entity<ExceptionLog>(entity =>
        {
            entity.ToTable("ExceptionLog");

            entity.Property(e => e.Action)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Controller)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.ExceptionMessage).IsUnicode(false);
            entity.Property(e => e.HttpMethod)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OccureAt).HasColumnType("datetime");
            entity.Property(e => e.Path)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.QueryString)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.StackTrace).IsUnicode(false);
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            entity.Property(e => e.UserName)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Form22Master>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Form22Ma__3214EC07F814C78A");

            entity.ToTable("Form22Master");

            entity.Property(e => e.ApprovalCertificateNo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.OemModelName)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.OemmodelId).HasColumnName("OEMModelId");
            entity.Property(e => e.PassbyNoiseLevel)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.SoundLevelHorn)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Oemmodel).WithMany(p => p.Form22Masters)
                .HasForeignKey(d => d.OemmodelId)
                .HasConstraintName("FK_Form22Master_OEMModelMaster");
        });

        modelBuilder.Entity<HsncodeMaster>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__HSNCodeM__3214EC07751AF33E");

            entity.ToTable("HSNCodeMaster");

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Description)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Hsncode)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("HSNCode");
            entity.Property(e => e.Type)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<HsnwiseTaxCode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__HSNWiseT__3214EC07CF7F1937");

            entity.ToTable("HSNWiseTaxCode");

            entity.Property(e => e.AtaxCode)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("ATaxCode");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.EffectiveDate).HasColumnType("datetime");
            entity.Property(e => e.Hsncode)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasColumnName("HSNCode");
            entity.Property(e => e.StateFlag)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
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
            entity.Property(e => e.HsncodeId).HasColumnName("HSNCodeId");
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

            entity.HasOne(d => d.HsncodeNavigation).WithMany(p => p.ItemMasters)
                .HasForeignKey(d => d.HsncodeId)
                .HasConstraintName("FK_HSNCodeMaster_ItemMaster");
        });

        modelBuilder.Entity<LedgerMaster>(entity =>
        {
            entity.ToTable("LedgerMaster");

            entity.Property(e => e.AadharNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Address)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.City)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("createdBy");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("createdDate");
            entity.Property(e => e.EMail)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("eMail");
            entity.Property(e => e.Gender)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Gstno)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("GSTNo");
            entity.Property(e => e.LedgerCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LedgerName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.LedgerType)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.MobileNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Pan)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("PAN");
            entity.Property(e => e.Pin)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.State)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("updatedBy");
            entity.Property(e => e.UpdatedDate)
                .HasColumnType("datetime")
                .HasColumnName("updatedDate");
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
            entity.Property(e => e.Productcode)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("productcode");
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
                .HasConstraintName("FKColorMaster");

            entity.HasOne(d => d.Dealer).WithMany(p => p.LmsleadMasters)
                .HasForeignKey(d => d.DealerId)
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

        modelBuilder.Entity<LotinspectionDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LOTInspe__3213E83FC262210D");

            entity.ToTable("LOTInspectionDetails");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AttributeCard).HasColumnName("attributeCard");
            entity.Property(e => e.BatteryNo)
                .HasMaxLength(100)
                .HasColumnName("batteryNo");
            entity.Property(e => e.ChargerNo)
                .HasMaxLength(100)
                .HasColumnName("chargerNo");
            entity.Property(e => e.ChargerQty).HasColumnName("chargerQty");
            entity.Property(e => e.ChargingKit).HasColumnName("chargingKit");
            entity.Property(e => e.ChassisNo)
                .HasMaxLength(100)
                .HasColumnName("chassisNo");
            entity.Property(e => e.ChassisWiseRemarks)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("chassisWiseRemarks");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("createdBy");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdDate");
            entity.Property(e => e.DamageDetails)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("damageDetails");
            entity.Property(e => e.FirstaidkitQty).HasColumnName("firstaidkitQty");
            entity.Property(e => e.IgnitionKeyset).HasColumnName("ignitionKeyset");
            entity.Property(e => e.InspectionDate).HasColumnName("inspectionDate");
            entity.Property(e => e.KeyFobSetQty).HasColumnName("keyFobSetQty");
            entity.Property(e => e.LocationName)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("locationName");
            entity.Property(e => e.LotHeaderId).HasColumnName("lotHeaderId");
            entity.Property(e => e.LotVehicleDamageImage)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("lotVehicleDamageImage");
            entity.Property(e => e.MirrorsetQty).HasColumnName("mirrorsetQty");
            entity.Property(e => e.ModelName)
                .HasMaxLength(200)
                .HasColumnName("modelName");
            entity.Property(e => e.ModelWiseSupervisor)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("modelWiseSupervisor");
            entity.Property(e => e.MotorNo)
                .HasMaxLength(100)
                .HasColumnName("motorNo");
            entity.Property(e => e.NoofVehicle).HasColumnName("noofVehicle");
            entity.Property(e => e.OwnersManual).HasColumnName("ownersManual");
            entity.Property(e => e.ToolKitQty).HasColumnName("toolKitQty");
            entity.Property(e => e.UpdateBy)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("updateBy");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updatedDate");
            entity.Property(e => e.VehicleStatus)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("vehicleStatus");

            entity.HasOne(d => d.LotHeader).WithMany(p => p.LotinspectionDetails)
                .HasForeignKey(d => d.LotHeaderId)
                .HasConstraintName("FK_LOTInspectionDetails_Header");
        });

        modelBuilder.Entity<LotinspectionHeader>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LOTInspe__3213E83FE7743E50");

            entity.ToTable("LOTInspectionHeader");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ArrivalDate).HasColumnName("arrivalDate");
            entity.Property(e => e.ArrivalTime)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("arrivalTime");
            entity.Property(e => e.CommonRemarks)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasColumnName("commonRemarks");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("createdBy");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createdDate");
            entity.Property(e => e.DealerCode)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("dealer_code");
            entity.Property(e => e.DriverContact)
                .HasMaxLength(100)
                .HasColumnName("driverContact");
            entity.Property(e => e.DriverName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("driverName");
            entity.Property(e => e.InvoiceDate).HasColumnName("invoiceDate");
            entity.Property(e => e.InvoiceNo)
                .HasMaxLength(100)
                .HasColumnName("invoiceNo");
            entity.Property(e => e.LocCode)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("loc_code");
            entity.Property(e => e.LotNo).HasColumnName("lotNo");
            entity.Property(e => e.LrDate).HasColumnName("lrDate");
            entity.Property(e => e.LrNo)
                .HasMaxLength(100)
                .HasColumnName("lrNo");
            entity.Property(e => e.PlasticCover)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("plasticCover");
            entity.Property(e => e.SupervisorName)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("supervisorName");
            entity.Property(e => e.TransporterName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("transporterName");
            entity.Property(e => e.TruckNo)
                .HasMaxLength(100)
                .HasColumnName("truckNo");
            entity.Property(e => e.UpdateBy)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("updateBy");
            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updatedDate");
            entity.Property(e => e.VehicleFasteningBracket)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("vehicleFasteningBracket");
        });

        modelBuilder.Entity<MenuMaster>(entity =>
        {
            entity.ToTable("MenuMaster");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedBy).HasColumnName("createdBy");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("createdDate");
            entity.Property(e => e.MenuName)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("menuName");
            entity.Property(e => e.ModuleName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("moduleName");
            entity.Property(e => e.ParentMenuId).HasColumnName("parentMenuId");
            entity.Property(e => e.PathName)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("pathName");
            entity.Property(e => e.SerialNo).HasColumnName("serialNo");
            entity.Property(e => e.UpdatedBy).HasColumnName("updatedBy");
            entity.Property(e => e.UpdatedDate)
                .HasColumnType("datetime")
                .HasColumnName("updatedDate");
        });

        modelBuilder.Entity<OemmodelMaster>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OEMModel__3214EC07901A12F4");

            entity.ToTable("OEMModelMaster");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModelName).HasMaxLength(150);
            entity.Property(e => e.ModelShortName).HasMaxLength(150);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<ParameterMasterTable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Paramete__3214EC073BC8EE12");

            entity.ToTable("ParameterMasterTable");

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.ParameterName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ParameterValue).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Purchase__3214EC07451B4139");

            entity.ToTable("PurchaseOrder");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ConsigneeCode)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CustomerCode)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FameIiflag)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("FameIIFlag");
            entity.Property(e => e.OrderType)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Ponumber)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("PONumber");
            entity.Property(e => e.PurchaseDate).HasColumnType("datetime");
            entity.Property(e => e.ReferenceNo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TestCertificate)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TransactionType)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<PurchaseOrderDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Purchase__3214EC073EB72DCD");

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ItemCode)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.LineAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Ponumber)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("PONumber");
            entity.Property(e => e.Rate).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.Subsidy).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Unit)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<ReceiptEntry>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ReceiptE__3214EC07F28D8BBC");

            entity.ToTable("ReceiptEntry");

            entity.Property(e => e.BookingId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BusinessType).HasMaxLength(50);
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Financier)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Location)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.MobileNo).HasMaxLength(15);
            entity.Property(e => e.Narration)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PartyName)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.ProductCode)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.ReceiptNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ReceiptType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RefNo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SaleType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SalesExecutive)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<RoleWiseMenuRight>(entity =>
        {
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("createdBy");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("createdDate");
            entity.Property(e => e.RoleId).HasMaxLength(450);
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("updatedBy");
            entity.Property(e => e.UpdatedDate)
                .HasColumnType("datetime")
                .HasColumnName("updatedDate");

            entity.HasOne(d => d.Role).WithMany(p => p.RoleWiseMenuRights)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_RoleWiseMenuRights");

            entity.HasOne(d => d.SubMenu).WithMany(p => p.RoleWiseMenuRights)
                .HasForeignKey(d => d.SubMenuId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MenuMaster_RoleWiseMenuRights");
        });

        modelBuilder.Entity<TaxCodeMaster>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TaxCodeM__3214EC0754AFA1A1");

            entity.ToTable("TaxCodeMaster");

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Description)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.EffectiveDate).HasColumnType("datetime");
            entity.Property(e => e.TaxCode)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TaxRate).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<TaxDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TaxDetai__3214EC0758AC8F4C");

            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ItemCode).HasMaxLength(50);
            entity.Property(e => e.PodetailsLineNumber).HasColumnName("PODetailsLineNumber");
            entity.Property(e => e.Ponumber)
                .HasMaxLength(50)
                .HasColumnName("PONumber");
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TaxCode).HasMaxLength(20);
            entity.Property(e => e.TaxRate).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<VehicleDispatch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__VehicleD__3214EC0732A91997");

            entity.ToTable("VehicleDispatch");

            entity.Property(e => e.BatteryCapacity)
                .HasMaxLength(20)
                .HasColumnName("battery_capacity");
            entity.Property(e => e.BatteryChemistry)
                .HasMaxLength(20)
                .HasColumnName("battery_chemistry");
            entity.Property(e => e.BatteryId)
                .HasMaxLength(50)
                .HasColumnName("battery_id");
            entity.Property(e => e.BatteryIdno).HasColumnName("battery_idno");
            entity.Property(e => e.BatteryMake)
                .HasMaxLength(100)
                .HasColumnName("battery_make");
            entity.Property(e => e.BatteryNo)
                .HasMaxLength(50)
                .HasColumnName("battery_no");
            entity.Property(e => e.BatteryNo2)
                .HasMaxLength(50)
                .HasColumnName("battery_no2");
            entity.Property(e => e.BatteryNo3)
                .HasMaxLength(50)
                .HasColumnName("battery_no3");
            entity.Property(e => e.BatteryNo4)
                .HasMaxLength(50)
                .HasColumnName("battery_no4");
            entity.Property(e => e.BatteryNo5)
                .HasMaxLength(50)
                .HasColumnName("battery_no5");
            entity.Property(e => e.BatteryNo6)
                .HasMaxLength(50)
                .HasColumnName("battery_no6");
            entity.Property(e => e.BikeMobileno)
                .HasMaxLength(15)
                .HasColumnName("bike_mobileno");
            entity.Property(e => e.BikeSimid)
                .HasMaxLength(10)
                .HasColumnName("bike_simid");
            entity.Property(e => e.ChargerNo)
                .HasMaxLength(50)
                .HasColumnName("charger_no");
            entity.Property(e => e.ChasisNo)
                .HasMaxLength(50)
                .HasColumnName("chasis_no");
            entity.Property(e => e.ColrCode)
                .HasMaxLength(10)
                .HasColumnName("colr_code");
            entity.Property(e => e.ControllerNo)
                .HasMaxLength(50)
                .HasColumnName("controller_no");
            entity.Property(e => e.Converter)
                .HasMaxLength(50)
                .HasColumnName("converter");
            entity.Property(e => e.DealerCode)
                .HasMaxLength(20)
                .HasColumnName("dealer_code");
            entity.Property(e => e.EcuBalMac)
                .HasMaxLength(50)
                .HasColumnName("ecu_bal_mac");
            entity.Property(e => e.EcuImEi)
                .HasMaxLength(50)
                .HasColumnName("ecu_im_ei");
            entity.Property(e => e.EcuSerno)
                .HasMaxLength(50)
                .HasColumnName("ecu_serno");
            entity.Property(e => e.Fame2Discount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("fame2_discount");
            entity.Property(e => e.GstIdno).HasColumnName("gst_idno");
            entity.Property(e => e.ImmoblizerNo)
                .HasMaxLength(50)
                .HasColumnName("immoblizer_no");
            entity.Property(e => e.ImmoblizerStatus)
                .HasMaxLength(10)
                .HasColumnName("immoblizer_status");
            entity.Property(e => e.InvoiceDate).HasColumnName("invoice_date");
            entity.Property(e => e.InvoiceNo)
                .HasMaxLength(50)
                .HasColumnName("invoice_no");
            entity.Property(e => e.IsAccepted)
                .HasDefaultValue(false)
                .HasColumnName("isAccepted");
            entity.Property(e => e.ItemCode)
                .HasMaxLength(50)
                .HasColumnName("item_code");
            entity.Property(e => e.KeyNo)
                .HasMaxLength(50)
                .HasColumnName("key_no");
            entity.Property(e => e.LocCode)
                .HasMaxLength(20)
                .HasColumnName("loc_code");
            entity.Property(e => e.MfgMonth).HasColumnName("mfg_month");
            entity.Property(e => e.MfgYear).HasColumnName("mfg_year");
            entity.Property(e => e.MotorNo)
                .HasMaxLength(50)
                .HasColumnName("motor_no");
            entity.Property(e => e.Ordertype)
                .HasMaxLength(50)
                .HasColumnName("ordertype");
            entity.Property(e => e.Regnumber)
                .HasMaxLength(20)
                .HasColumnName("regnumber");
            entity.Property(e => e.ServBkno)
                .HasMaxLength(50)
                .HasColumnName("serv_bkno");
            entity.Property(e => e.SoundbarBalMac)
                .HasMaxLength(50)
                .HasColumnName("soundbar_bal_mac");
            entity.Property(e => e.SoundbarSerno)
                .HasMaxLength(50)
                .HasColumnName("soundbar_serno");
            entity.Property(e => e.Startdate)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("startdate");
            entity.Property(e => e.TyreNo1)
                .HasMaxLength(50)
                .HasColumnName("tyre_no1");
            entity.Property(e => e.TyreNo2)
                .HasMaxLength(50)
                .HasColumnName("tyre_no2");
            entity.Property(e => e.Validity)
                .HasMaxLength(20)
                .HasColumnName("validity");
            entity.Property(e => e.Vcu)
                .HasMaxLength(50)
                .HasColumnName("vcu");
            entity.Property(e => e.Voltage)
                .HasMaxLength(20)
                .HasColumnName("voltage");
        });
        modelBuilder.HasSequence("LotNo_Seq");

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
