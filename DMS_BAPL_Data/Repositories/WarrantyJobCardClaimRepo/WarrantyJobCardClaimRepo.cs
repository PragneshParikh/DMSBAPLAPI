using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DMS_BAPL_Data.Repositories.WarrantyJobCardClaimRepo
{
    public class WarrantyJobCardClaimRepo : IWarrantyJobCardClaimRepo
    {
        private readonly BapldmsvadContext _context;

        public WarrantyJobCardClaimRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        [HttpPost("InsertWarrantyJCClaim")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<int> InsertWarrantyJCClaim(WarrantyJCClaimViewModel model, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                //=========================
                // Insert Header
                //=========================
                var header = new WarrantyJcclaim
                {
                    DealerCode = model.DealerCode,
                    ClaimPrefix = model.ClaimPrefix,
                    ClaimNo = model.ClaimNo,
                    ClaimDate = model.ClaimDate,
                    ChassisNo = model.ChassisNo,
                    SupplierId = model.SupplierId,
                    JobCardHeaderId = model.JobCardHeaderId,
                    CustomerLedgerId = model.CustomerLedgerId,
                    RepairBillHeaderId = model.RepairBillHeaderId,
                    Ffirid = model.FFIRId,
                    ClaimAccount = model.ClaimAccount,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    //IsActive = true
                };

                _context.WarrantyJcclaims.Add(header);
                await _context.SaveChangesAsync();

                //=========================
                // Insert Details
                //=========================
                if (model.repairBillDetails != null && model.repairBillDetails.Any())
                {
                    var details = model.repairBillDetails.Select(x => new WarrantyJcclaimDetail
                    {
                        WarrantyJcclaimHeaderId = header.Id,

                        RepairBillDetailId = x.RepairBillDetailsId,

                        ItemType = x.ItemType,

                        MaterialId = x.MaterialId,
                        LabourMasterId = x.LabourId,
                        PartWiseLabourId = x.PartWiseLabourId,
                        PartItemId = x.PartItemId,

                        Qty = x.ItemType == "Labour"
                         ? (x.LabourQty ?? 0)
                         : x.PartItemQty,

                        Rate = x.ItemType == "Labour"
                         ? (x.LabourRate ?? 0)
                         : (x.PartItemRate ?? 0),

                        // Calculate Amount based on whether it is a Labour or Part item
                        Amount = x.IgstAmount,

                        // Calculate TaxAmount (If you have a tax percentage property, e.g., x.TaxPercentage)
                        // If you don't have tax percentage, you will need to pass it from the model or database.
                        TaxAmount = x.TotalWithTax ?? 0,

                        // Calculate TotalAmount by adding Amount and TaxAmount
                        TotalAmount = x.IgstAmount + x.TotalWithTax,


                        ClaimType = "Warranty",
                        DealerObservation = x.DealerObservation,
                        RootCauseAnalysis = x.RootCauseAnalysis,

                        CreatedBy = userId,
                        CreatedDate = DateTime.Now
                    }).ToList();

                    _context.WarrantyJcclaimDetails.AddRange(details);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                model.RepairBillHeaderId = header.Id;

                return header.Id;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<WarrantyJCClaimListViewModel>> GetAllWarrantyJCClaims(string dealerCode)
        {
            var claims = await _context.WarrantyJcclaims
                .Include(x => x.Supplier)
                .Include(x => x.JobCardHeader)
                .Include(x => x.WarrantyJcclaimDetails)
                .Where(x => x.DealerCode == dealerCode)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return claims.Select(c => new WarrantyJCClaimListViewModel
            {
                Id = c.Id,
                ClaimPrefix = c.ClaimPrefix,
                ClaimNo = c.ClaimNo,
                ClaimDate = c.ClaimDate,
                ChassisNo = c.ChassisNo,
                SupplierName = c.Supplier != null ? c.Supplier.LedgerName : null,
                JobCardNo = c.JobCardHeader != null
                    ? $"{c.JobCardHeader.Jobprefix}{c.JobCardHeader.JobNo}"
                    : null,
                TotalAmount = c.WarrantyJcclaimDetails != null
                    ? c.WarrantyJcclaimDetails.Sum(d => d.TotalAmount ?? 0)
                    : 0
            }).ToList();
        }

        public async Task<WarrantyJCClaimSearchResultViewModel> SearchWarrantyJCClaims(WarrantyJCClaimSearchViewModel filter)
        {
            var query = _context.WarrantyJcclaims
                .Include(x => x.Supplier)
                .Include(x => x.JobCardHeader)
                .Include(x => x.WarrantyJcclaimDetails)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.DealerCode))
                query = query.Where(x => x.DealerCode == filter.DealerCode);

            if (filter.DateFrom.HasValue)
                query = query.Where(x => x.ClaimDate >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(x => x.ClaimDate <= filter.DateTo.Value);

            if (!string.IsNullOrWhiteSpace(filter.ChassisNo))
                query = query.Where(x => x.ChassisNo != null && x.ChassisNo.Contains(filter.ChassisNo));

            if (filter.ClaimNo.HasValue)
                query = query.Where(x => x.ClaimNo == filter.ClaimNo.Value);

            query = query.OrderByDescending(x => x.CreatedDate);

            var totalCount = await query.CountAsync();

            var claims = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var items = claims.Select(c => new WarrantyJCClaimListViewModel
            {
                Id = c.Id,
                ClaimPrefix = c.ClaimPrefix,
                ClaimNo = c.ClaimNo,
                ClaimDate = c.ClaimDate,
                ChassisNo = c.ChassisNo,
                SupplierName = c.Supplier != null ? c.Supplier.LedgerName : null,
                JobCardNo = c.JobCardHeader != null
                    ? $"{c.JobCardHeader.Jobprefix}{c.JobCardHeader.JobNo}"
                    : null,
                TotalAmount = c.WarrantyJcclaimDetails != null
                    ? c.WarrantyJcclaimDetails.Sum(d => d.TotalAmount ?? 0)
                    : 0
            }).ToList();

            return new WarrantyJCClaimSearchResultViewModel
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task<byte[]> GenerateWarrantyJCClaimListPdf(WarrantyJCClaimSearchViewModel filter)
        {
            // Same filters as SearchWarrantyJCClaims, but no paging - a printed
            // report should include every matching record, not just one page.
            var query = _context.WarrantyJcclaims
                .Include(x => x.Supplier)
                .Include(x => x.JobCardHeader)
                .Include(x => x.WarrantyJcclaimDetails)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.DealerCode))
                query = query.Where(x => x.DealerCode == filter.DealerCode);

            if (filter.DateFrom.HasValue)
                query = query.Where(x => x.ClaimDate >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(x => x.ClaimDate <= filter.DateTo.Value);

            if (!string.IsNullOrWhiteSpace(filter.ChassisNo))
                query = query.Where(x => x.ChassisNo != null && x.ChassisNo.Contains(filter.ChassisNo));

            if (filter.ClaimNo.HasValue)
                query = query.Where(x => x.ClaimNo == filter.ClaimNo.Value);

            var claims = await query.OrderByDescending(x => x.CreatedDate).ToListAsync();

            var rows = claims.Select(c => new WarrantyJCClaimListViewModel
            {
                Id = c.Id,
                ClaimPrefix = c.ClaimPrefix,
                ClaimNo = c.ClaimNo,
                ClaimDate = c.ClaimDate,
                ChassisNo = c.ChassisNo,
                SupplierName = c.Supplier != null ? c.Supplier.LedgerName : null,
                JobCardNo = c.JobCardHeader != null
                    ? $"{c.JobCardHeader.Jobprefix}{c.JobCardHeader.JobNo}"
                    : null,
                TotalAmount = c.WarrantyJcclaimDetails != null
                    ? c.WarrantyJcclaimDetails.Sum(d => d.TotalAmount ?? 0)
                    : 0
            }).ToList();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(25);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Warranty Claim List").FontSize(16).Bold();
                        col.Item().PaddingTop(4).Text(
                            $"Date Range: {filter.DateFrom:dd-MM-yyyy} to {filter.DateTo:dd-MM-yyyy}"
                        );
                    });

                    page.Content().PaddingTop(12).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1.6f);
                            columns.RelativeColumn(1.6f);
                            columns.RelativeColumn(2.2f);
                            columns.RelativeColumn(1.3f);
                        });

                        void HeaderCell(QuestPDF.Infrastructure.IContainer c, string text) =>
                            c.Background(Colors.Grey.Lighten2).Padding(4).Text(text).Bold();

                        table.Header(header =>
                        {
                            HeaderCell(header.Cell(), "Sr.No");
                            HeaderCell(header.Cell(), "Claim No / Date");
                            HeaderCell(header.Cell(), "JobCard No");
                            HeaderCell(header.Cell(), "Chassis No");
                            HeaderCell(header.Cell(), "Supplier");
                            HeaderCell(header.Cell(), "Total Amount");
                        });

                        int srNo = 1;
                        decimal grandTotal = 0;

                        foreach (var row in rows)
                        {
                            grandTotal += row.TotalAmount;

                            table.Cell().Padding(4).Text(srNo++.ToString());
                            table.Cell().Padding(4).Text($"{row.ClaimPrefix}{row.ClaimNo}\n{row.ClaimDate:dd-MM-yyyy}");
                            table.Cell().Padding(4).Text(row.JobCardNo ?? "");
                            table.Cell().Padding(4).Text(row.ChassisNo ?? "");
                            table.Cell().Padding(4).Text(row.SupplierName ?? "");
                            table.Cell().Padding(4).AlignRight().Text(row.TotalAmount.ToString("0.00"));
                        }

                        table.Cell().ColumnSpan(5).Padding(4).AlignRight().Text("Grand Total:").Bold();
                        table.Cell().Padding(4).AlignRight().Text(grandTotal.ToString("0.00")).Bold();
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        }

        public async Task<byte[]> GenerateWarrantyJCClaimPdf(int id)
        {
            var claim = await _context.WarrantyJcclaims
                .Include(x => x.Supplier)
                .Include(x => x.JobCardHeader)
                .Include(x => x.RepairBillHeader)
                // ASSUMPTION: CustomerLedger navigation exists on WarrantyJcclaim
                // (CustomerLedgerId is already a FK on this entity). If this
                // include fails to compile, check WarrantyJcclaim.cs for the
                // actual navigation property name and adjust.
                .Include(x => x.CustomerLedger)
                .Include(x => x.WarrantyJcclaimDetails)
                    .ThenInclude(d => d.RepairBillDetail)
                        .ThenInclude(rb => rb.PartItem)
                .Include(x => x.WarrantyJcclaimDetails)
                    .ThenInclude(d => d.RepairBillDetail)
                        .ThenInclude(rb => rb.LabourMaster)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (claim == null)
                throw new InvalidOperationException($"Warranty Claim with Id {id} not found.");

            var jobCardNo = claim.JobCardHeader != null
                ? $"{claim.JobCardHeader.Jobprefix}{claim.JobCardHeader.JobNo}"
                : "";

            var motorNo = await _context.ChassisBatteryDetails
                .Where(x => x.ChassisNo == claim.ChassisNo)
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => x.MotorNo)
                .FirstOrDefaultAsync();

            // ASSUMPTION: LedgerName is confirmed correct on LedgerMaster.
            // City/State on LedgerMaster are FK ints (confirmed via LedgerMasterRepo's
            // own join pattern: "join C in _context.Cities on LM.City equals C.CityId"),
            // not plain text - resolved below via the same join, no new entity added.
            string customerNameAddress = "";
            if (claim.CustomerLedger != null)
            {
                string? cityName = null;
                string? stateName = null;

                if (claim.CustomerLedger.City != null)
                {
                    cityName = await _context.Cities
                        .Where(c => c.CityId == claim.CustomerLedger.City)
                        .Select(c => c.CityName)
                        .FirstOrDefaultAsync();
                }
                if (claim.CustomerLedger.State != null)
                {
                    stateName = await _context.States
                        .Where(s => s.StateId == claim.CustomerLedger.State)
                        .Select(s => s.StateName)
                        .FirstOrDefaultAsync();
                }

                customerNameAddress = string.Join(", ", new[] {
            claim.CustomerLedger.LedgerName,
            claim.CustomerLedger.Address,
            cityName,
            stateName,
            claim.CustomerLedger.Pin
        }.Where(s => !string.IsNullOrWhiteSpace(s)));
            }

            // MODEL / MODEL CODE / VEHICLE REG NO / DATE OF SALE - all resolved via
            // ChassisDetail (keyed by ChassisNo, same table VehicleSaleBillRepo uses
            // for RegNo/SaleDate) then ItemMaster (keyed by ItemCode, same table
            // ChassisRepo/ItemMasterRepo use for Itemname) - no new entities added,
            // just the same joins already used elsewhere in this codebase.
            var chassisDetail = await _context.ChassisDetails
                .FirstOrDefaultAsync(x => x.ChassisNo == claim.ChassisNo);

            string vehicleRegNo = chassisDetail?.RegNo ?? "";
            string dateOfSale = chassisDetail?.SaleDate?.ToString("dd-MM-yyyy") ?? "";
            string modelName = "";
            string modelCode = "";

            if (!string.IsNullOrWhiteSpace(chassisDetail?.ItemCode))
            {
                var item = await _context.ItemMasters
                    .FirstOrDefaultAsync(x => x.Itemcode == chassisDetail.ItemCode);
                modelName = item?.Itemname ?? item?.Displayname ?? "";
                modelCode = item?.Itemcode ?? "";
            }

            // DATE OF FAILURE and the per-line "Observation" column genuinely don't
            // exist anywhere in this data model - no field on WarrantyJcclaim,
            // WarrantyJcclaimDetail, JobCardHeader, or ChassisDetail captures either
            // of these. Left blank rather than guessing at a value; adding them
            // would require a new column, which wasn't wanted here.

            // No Dealer-master join is available in this codebase's current model
            // (DealerCode is stored as a plain string throughout, not a FK with a
            // navigation property) - showing the code only. Swap in a real
            // dealer-name lookup here if a Dealer master table becomes available.
            string sellingDealerName = claim.DealerCode ?? "";
            string sellingDealerCode = claim.DealerCode ?? "";

            // Consistent border color/width for every cell across every table -
            // change once here to restyle the whole form.
            const float borderWidth = 0.75f;
            var borderColor = Colors.Black;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(15);
                    page.DefaultTextStyle(x => x.FontSize(7));

                    // Wraps any cell content in a consistent border, used for every
                    // cell in every table below so the whole form reads as one
                    // properly ruled grid instead of outer-box-only borders.
                    QuestPDF.Infrastructure.IContainer Bordered(QuestPDF.Infrastructure.IContainer c) =>
                        c.Border(borderWidth).BorderColor(borderColor).Padding(3);

                    page.Content().Column(col =>
                    {
                        col.Spacing(0);

                        // ---- Title ----
                        col.Item().PaddingBottom(4).AlignCenter().Text("WARRANTY CLAIM").FontSize(14).Bold();

                        // ---- Dealer Code / Year+Month / Sr.No - three separate
                        // bordered blocks, "NO:" removed entirely ----
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            void Block(string label, string value)
                            {
                                Bordered(table.Cell()).Column(c =>
                                {
                                    c.Item().Text(label).FontSize(6).Bold();
                                    c.Item().Text(value);
                                });
                            }

                            Block("DEALER CODE", claim.DealerCode ?? "");
                            Block("YEAR / MONTH", claim.ClaimDate.HasValue
                                ? $"{claim.ClaimDate:yyyy} / {claim.ClaimDate:MM}"
                                : "");
                            Block("SR.NO.", claim.ClaimNo?.ToString() ?? "");
                        });

                        // ---- Selling Dealer / Customer / Parts Despatch row ----
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn();
                            });

                            Bordered(table.Cell()).Column(c =>
                            {
                                c.Item().Text("SELLING DEALER NAME").FontSize(6).Bold();
                                c.Item().Text(sellingDealerName);
                                c.Item().PaddingTop(4).Text("SELLING DEALER CODE").FontSize(6).Bold();
                                c.Item().Text(sellingDealerCode);
                            });

                            Bordered(table.Cell()).Column(c =>
                            {
                                c.Item().Text("CUSTOMER's NAME & FULL ADDRESS").FontSize(6).Bold();
                                c.Item().Text(customerNameAddress);
                            });

                            Bordered(table.Cell()).Column(c =>
                            {
                                c.Item().Text("PARTS DESPATCH DETAILS (RPP / L.R No.& Dt. or Hand-Delivery Details)").FontSize(6).Bold();
                                c.Item().PaddingTop(10).Table(inner =>
                                {
                                    inner.ColumnsDefinition(cols => { cols.RelativeColumn(); cols.RelativeColumn(); cols.RelativeColumn(); });
                                    Bordered(inner.Cell()).Column(cc => { cc.Item().Text("MODEL CODE").FontSize(6).Bold(); cc.Item().Text(modelCode); });
                                    Bordered(inner.Cell()).Column(cc => { cc.Item().Text("DESCREPANCY CODE").FontSize(6).Bold(); cc.Item().Text(""); });
                                    Bordered(inner.Cell()).Column(cc => { cc.Item().Text("WRC CODE").FontSize(6).Bold(); cc.Item().Text(""); });
                                });
                            });
                        });

                        // ---- Vehicle Repaired By (its own block) + Model/RegNo/JobNo/
                        // Service history (1-6 only) as a separate table alongside it ----
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Table(t =>
                            {
                                t.ColumnsDefinition(c => c.RelativeColumn());

                                Bordered(t.Cell()).Column(c =>
                                {
                                    c.Item().Text("VEHICLE REPAIRED BY:").FontSize(6).Bold();
                                    c.Item().Text(sellingDealerName);
                                    c.Item().PaddingTop(4).Text("REPAIRING DEALER CODE:").FontSize(6).Bold();
                                    c.Item().Text(sellingDealerCode);
                                });
                            });

                            row.RelativeItem(3).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1.3f); // Model
                                    columns.RelativeColumn();     // Vehicle Reg No
                                    columns.RelativeColumn();     // Workshop Job No
                                    for (int i = 0; i < 6; i++) columns.RelativeColumn(0.7f); // 1st-6th Ser KM/DT
                                });

                                Bordered(table.Cell()).Text("MODEL").FontSize(5).Bold();
                                Bordered(table.Cell()).Text("VEHICLE REG. NO.").FontSize(5).Bold();
                                Bordered(table.Cell()).Text("WORKSHOP JOB NO.").FontSize(5).Bold();
                                for (int i = 1; i <= 6; i++)
                                    Bordered(table.Cell()).Text($"{i} SER KM/DT").FontSize(5).Bold();

                                Bordered(table.Cell()).Text(modelName);
                                Bordered(table.Cell()).Text(vehicleRegNo);
                                Bordered(table.Cell()).Text(jobCardNo);
                                for (int i = 0; i < 6; i++) Bordered(table.Cell()).Text(""); // service history - not tracked
                            });
                        });

                        // ---- VIN / Motor / Dates / KMS row ----
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1.4f);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn(1.2f);
                            });

                            Bordered(table.Cell()).Text("VIN / CHASSIS NO.").FontSize(6).Bold();
                            Bordered(table.Cell()).Text("MOTOR NO").FontSize(6).Bold();
                            Bordered(table.Cell()).Text("DATE OF SALE").FontSize(6).Bold();
                            Bordered(table.Cell()).Text("DATE OF FAILURE").FontSize(6).Bold();
                            Bordered(table.Cell()).Text("DATE OF REPAIR").FontSize(6).Bold();
                            Bordered(table.Cell()).Text("DATE OF CLAIM").FontSize(6).Bold();
                            Bordered(table.Cell()).Text("DAYS / KMS. READING AT REPAIR").FontSize(6).Bold();

                            Bordered(table.Cell()).Text(claim.ChassisNo ?? "");
                            Bordered(table.Cell()).Text(motorNo ?? "");
                            Bordered(table.Cell()).Text(dateOfSale);
                            // Date of Failure genuinely doesn't exist anywhere in the
                            // data model (see chat notes) - left blank.
                            Bordered(table.Cell()).Text("");
                            Bordered(table.Cell()).Text(claim.RepairBillHeader?.CreatedDate?.ToString("dd-MM-yyyy") ?? "");
                            Bordered(table.Cell()).Text(claim.ClaimDate?.ToString("dd-MM-yyyy") ?? "");
                            Bordered(table.Cell()).Text(claim.JobCardHeader?.Vehiclekms?.ToString() ?? "");
                        });

                        // ---- Main parts/labour detail table ----
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(20);   // Sr.No
                                columns.RelativeColumn(1.3f); // Part Number
                                columns.RelativeColumn(1.8f); // Part Description
                                columns.RelativeColumn(0.8f); // Inward Serial
                                columns.RelativeColumn(0.8f); // Outward Serial
                                columns.ConstantColumn(25);    // Qty
                                columns.RelativeColumn(2f);   // Dealer Observation & RCA
                                columns.RelativeColumn(0.8f); // Defect Code
                                columns.RelativeColumn(0.8f); // Decision
                                columns.RelativeColumn(0.8f); // Lab code
                                columns.RelativeColumn(0.8f); // Mfg Dt Code
                                columns.RelativeColumn(0.8f); // Vendor Code
                                columns.RelativeColumn(1f);   // Observation
                            });

                            void H(string t) => Bordered(table.Cell()).Background(Colors.Grey.Lighten2).Text(t).FontSize(5.5f).Bold();

                            H("Sr.No"); H("Part Number"); H("Part Description");
                            H("Inward Serial"); H("Outward Serial"); H("Qty.");
                            H("Dealer Observation & RCA");
                            H("Defect Code"); H("Decision"); H("Lab code");
                            H("Mfg. Dt.Code"); H("Vendor Code"); H("Observation");

                            int srNo = 1;
                            foreach (var d in claim.WarrantyJcclaimDetails)
                            {
                                bool isLabour = d.ItemType == "Labour";
                                var rbd = d.RepairBillDetail;
                                string code = isLabour ? (rbd?.LabourMaster?.LabourCode ?? "") : (rbd?.PartItem?.Itemcode ?? "");
                                string desc = isLabour ? (rbd?.LabourMaster?.LabourDescription ?? "") : (rbd?.PartItem?.Itemdesc ?? "");
                                string observationAndRca = string.Join(" / ", new[] { d.DealerObservation, d.RootCauseAnalysis }
                                    .Where(s => !string.IsNullOrWhiteSpace(s)));

                                Bordered(table.Cell()).Text(srNo++.ToString());
                                Bordered(table.Cell()).Text(code);
                                Bordered(table.Cell()).Text(desc);
                                Bordered(table.Cell()).Text(""); // Inward Serial - not captured
                                Bordered(table.Cell()).Text(""); // Outward Serial - not captured
                                Bordered(table.Cell()).Text((d.Qty ?? 0).ToString());
                                Bordered(table.Cell()).Text(observationAndRca);
                                Bordered(table.Cell()).Text(""); // Defect Code - not captured
                                Bordered(table.Cell()).Text(""); // Decision - not captured
                                Bordered(table.Cell()).Text(""); // Lab code - not captured
                                Bordered(table.Cell()).Text(""); // Mfg Dt Code - not captured
                                Bordered(table.Cell()).Text(""); // Vendor Code - not captured
                                Bordered(table.Cell()).Text(""); // Observation - not captured
                            }
                        });

                        // ---- Voice of Customer / Analysis Engr ----
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn();
                            });

                            Bordered(table.Cell()).Text("Voice of Customers : "); // not captured - label only
                            Bordered(table.Cell()).Text("ANALYSIS\nENGR.").FontSize(6).Bold();
                        });

                        // ---- Notes / Instructions footer (no borders - plain text,
                        // matching the reference form) ----
                        col.Item().PaddingTop(6).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("NOTE :").Bold();
                                c.Item().Text("1. This claim form is valid for warranty of all models.");
                                c.Item().Text("2. Please verify vehicle details filled in warranty claim form with WR.");
                                c.Item().Text("3. TAG all warranty parts. Tags should be of specified design only.");
                                c.Item().Text("4. Pack all parts in the same carton box.");
                                c.Item().Text("5. Affix a sticker indicating dealer code & claim no. on each part.");
                                c.Item().Text("6. For warranty packing, preferably re-use the carton box sent during spare dispatch to dealer.");
                                c.Item().Text("7. \"Warranty\" to be written on the boxes so as to identify it as warranty consignment.");
                            });

                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("IMPORTANT INSTRUCTIONS").Bold();
                                c.Item().Text("1. Incomplete claim form in any respect is liable for rejection.");
                                c.Item().Text("2. The claim form should be system generated (DMS).");
                                c.Item().Text("3. The material parcel weight upto 5 kgs should be sent by registered post parcel only.");
                                c.Item().Text("4. All parcels more than 5 kgs to be booked to plant of transporter.");
                                c.Item().Text("5. Material should be sent only through recommended transporters only.");
                                c.Item().Text("6. Do not send material through railway/train.");
                                c.Item().Text("7. Do not change original claim form serial number.");
                            });
                        });

                        col.Item().PaddingTop(10).Row(row =>
                        {
                            row.RelativeItem().Text("Warranty Claim is generated in DMS").Bold();
                            row.RelativeItem().AlignRight().Text("Repairing Center's sign & stamp").Bold();
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}
