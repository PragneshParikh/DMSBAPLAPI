using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using DMS_BAPL_Data.Repositories.UwLineItemRepo;
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
        private readonly IUwLineItemRepo _uwLineItemRepo;

        public WarrantyJobCardClaimRepo(BapldmsvadContext context, IUwLineItemRepo uwLineItemRepo)
        {
            _context = context;
            _uwLineItemRepo = uwLineItemRepo;
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
                    LocationCode = model.LocationCode,   
                    LocationName = model.LocationName,   
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
                    var details = model.repairBillDetails.Select(x =>
                    {
                        bool isLabour = x.ItemType == "Labour";

                        decimal qty = isLabour ? (x.LabourQty ?? 0) : (x.PartItemQty ?? 0);
                        decimal rate = isLabour ? (x.LabourRate ?? 0) : (x.PartItemRate ?? 0);

                        decimal baseAmount = qty * rate;
                        decimal gstAmount = x.IgstAmount;

                        decimal mrp = x.Mrp > 0 ? x.Mrp : rate;

                        decimal totalAmount = baseAmount + gstAmount;

                        return new WarrantyJcclaimDetail
                        {
                            WarrantyJcclaimHeaderId = header.Id,
                            RepairBillDetailId = x.RepairBillDetailsId,
                            ItemType = x.ItemType,
                            MaterialId = x.MaterialId,
                            LabourMasterId = x.LabourId,
                            PartWiseLabourId = x.PartWiseLabourId,
                            PartItemId = x.PartItemId,

                            Qty = qty,
                            Rate = rate,
                            Mrp = mrp,

                            Amount = baseAmount,
                            TaxAmount = gstAmount,
                            TotalAmount = totalAmount,

                            ClaimType = "Warranty",
                            DealerObservation = x.DealerObservation,
                            RootCauseAnalysis = x.RootCauseAnalysis,

                            CreatedBy = userId,
                            CreatedDate = DateTime.Now
                        };
                    }).ToList();
                   
                    _context.WarrantyJcclaimDetails.AddRange(details);
                    await _context.SaveChangesAsync();
                }

                await _uwLineItemRepo.InsertUwLineItem(header.Id, userId);   // <-- the missing call

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

            var chassisBattery = await _context.ChassisBatteryDetails
                .Where(x => x.ChassisNo == claim.ChassisNo)
                .OrderByDescending(x => x.CreatedDate)
                .FirstOrDefaultAsync();

            var motorNo = chassisBattery?.MotorNo ?? "";
            var batteryNo = chassisBattery?.BatteryNo ?? "";
            var chargerNo = chassisBattery?.ChargerNo ?? "";

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

            string sellingDealerName = claim.DealerCode ?? "";
            string sellingDealerCode = claim.DealerCode ?? "";

            var termsAndConditions = await _context.TermandConditionMasters
                .Where(x => x.ConditionModule == 5)
                .OrderBy(x => x.ConditionEffectiveDate)
                .Select(x => x.TermCondition)
                .ToListAsync();

            const float borderWidth = 0.75f;
            var borderColor = Colors.Black;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(15);
                    page.DefaultTextStyle(x => x.FontSize(7));

                    QuestPDF.Infrastructure.IContainer Bordered(QuestPDF.Infrastructure.IContainer c) =>
                        c.Border(borderWidth).BorderColor(borderColor).Padding(3);

                    page.Content().Column(col =>
                    {
                        col.Spacing(0);

                        col.Item().PaddingBottom(4).AlignCenter().Text("WARRANTY CLAIM").FontSize(14).Bold();

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
                                    columns.RelativeColumn(1.3f);
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    for (int i = 0; i < 6; i++) columns.RelativeColumn(0.7f);
                                });

                                Bordered(table.Cell()).Text("MODEL").FontSize(5).Bold();
                                Bordered(table.Cell()).Text("VEHICLE REG. NO.").FontSize(5).Bold();
                                Bordered(table.Cell()).Text("WORKSHOP JOB NO.").FontSize(5).Bold();
                                for (int i = 1; i <= 6; i++)
                                    Bordered(table.Cell()).Text($"{i} SER KM/DT").FontSize(5).Bold();

                                Bordered(table.Cell()).Text(modelName);
                                Bordered(table.Cell()).Text(vehicleRegNo);
                                Bordered(table.Cell()).Text(jobCardNo);
                                for (int i = 0; i < 6; i++) Bordered(table.Cell()).Text("");
                            });
                        });

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1.3f);
                                columns.RelativeColumn(1.1f);
                                columns.RelativeColumn(1.1f);
                                columns.RelativeColumn(1.1f);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn(1.2f);
                            });

                            Bordered(table.Cell()).Text("VIN / CHASSIS NO.").FontSize(6).Bold();
                            Bordered(table.Cell()).Text("MOTOR NO").FontSize(6).Bold();
                            Bordered(table.Cell()).Text("BATTERY NO").FontSize(6).Bold();
                            Bordered(table.Cell()).Text("CHARGER NO").FontSize(6).Bold();
                            Bordered(table.Cell()).Text("DATE OF SALE").FontSize(6).Bold();
                            Bordered(table.Cell()).Text("DATE OF FAILURE").FontSize(6).Bold();
                            Bordered(table.Cell()).Text("DATE OF REPAIR").FontSize(6).Bold();
                            Bordered(table.Cell()).Text("DATE OF CLAIM").FontSize(6).Bold();
                            Bordered(table.Cell()).Text("DAYS / KMS. READING AT REPAIR").FontSize(6).Bold();

                            Bordered(table.Cell()).Text(claim.ChassisNo ?? "");
                            Bordered(table.Cell()).Text(motorNo);
                            Bordered(table.Cell()).Text(batteryNo);
                            Bordered(table.Cell()).Text(chargerNo);
                            Bordered(table.Cell()).Text(dateOfSale);
                            Bordered(table.Cell()).Text("");
                            Bordered(table.Cell()).Text(claim.RepairBillHeader?.CreatedDate?.ToString("dd-MM-yyyy") ?? "");
                            Bordered(table.Cell()).Text(claim.ClaimDate?.ToString("dd-MM-yyyy") ?? "");
                            Bordered(table.Cell()).Text(claim.JobCardHeader?.Vehiclekms?.ToString() ?? "");
                        });

                        // ---- Part Item Details ----
                        var partLines = claim.WarrantyJcclaimDetails.Where(d => d.ItemType == "Part").ToList();

                        col.Item().PaddingTop(6).Text("Part Item Details").FontSize(7).Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(18);    // Sr.No
                                columns.RelativeColumn(1.0f);  // Part Code
                                columns.RelativeColumn(1.3f);  // Part Description
                                columns.ConstantColumn(20);    // Qty
                                columns.RelativeColumn(0.75f); // Rate
                                columns.RelativeColumn(1.0f);  // CGST % / Amt combined
                                columns.RelativeColumn(1.0f);  // SGST % / Amt combined
                                columns.RelativeColumn(1.0f);  // IGST % / Amt combined
                                columns.RelativeColumn(0.85f); // Total GST
                                columns.RelativeColumn(0.85f); // Amount (base)
                                columns.RelativeColumn(0.85f); // Total
                                columns.RelativeColumn(1.6f);  // Observation & RCA - back in-grid
                            });

                            void PH(string t) => Bordered(table.Cell()).Background(Colors.Grey.Lighten2).Text(t).FontSize(5f).Bold();

                            PH("Sr.No"); PH("Part Code"); PH("Part Description"); PH("Qty.");
                            PH("Rate");
                            PH("CGST % / Amt"); PH("SGST % / Amt"); PH("IGST % / Amt");
                            PH("Total GST"); PH("Amount"); PH("Total"); PH("Observation & RCA");

                            int srNo = 1;
                            decimal partAmountTotal = 0, partCgstTotal = 0, partSgstTotal = 0, partIgstTotal = 0, partGstTotal = 0, partGrandTotal = 0;

                            foreach (var d in partLines)
                            {
                                var rbd = d.RepairBillDetail;
                                string code = rbd?.PartItem?.Itemcode ?? "";
                                string desc = rbd?.PartItem?.Itemdesc ?? "";

                                decimal cgstPercent = rbd?.PartItem?.Cgst ?? 0;
                                decimal cgstAmount = rbd?.Cgstamount ?? 0;
                                decimal sgstPercent = rbd?.PartItem?.Sgst ?? 0;
                                decimal sgstAmount = rbd?.Sgstamount ?? 0;
                                decimal igstPercent = rbd?.PartItem?.Igst ?? 0;
                                decimal igstAmount = rbd?.Igstamount ?? 0;

                                decimal totalGstAmount = cgstAmount + sgstAmount + igstAmount;

                                string observationAndRca = string.Join(" / ", new[] { d.DealerObservation, d.RootCauseAnalysis }
                                    .Where(s => !string.IsNullOrWhiteSpace(s)));

                                partAmountTotal += d.Amount ?? 0;
                                partCgstTotal += cgstAmount;
                                partSgstTotal += sgstAmount;
                                partIgstTotal += igstAmount;
                                partGstTotal += totalGstAmount;
                                partGrandTotal += d.TotalAmount ?? 0;

                                Bordered(table.Cell()).Text(srNo.ToString());
                                Bordered(table.Cell()).Text(code);
                                Bordered(table.Cell()).Text(desc);
                                Bordered(table.Cell()).Text((d.Qty ?? 0).ToString());
                                Bordered(table.Cell()).AlignRight().Text((d.Rate ?? 0).ToString("0.00"));
                                Bordered(table.Cell()).AlignRight().Text($"{cgstPercent:0.##}% / {cgstAmount:0.00}");
                                Bordered(table.Cell()).AlignRight().Text($"{sgstPercent:0.##}% / {sgstAmount:0.00}");
                                Bordered(table.Cell()).AlignRight().Text($"{igstPercent:0.##}% / {igstAmount:0.00}");
                                Bordered(table.Cell()).AlignRight().Text(totalGstAmount.ToString("0.00")).Bold();
                                Bordered(table.Cell()).AlignRight().Text((d.Amount ?? 0).ToString("0.00"));
                                Bordered(table.Cell()).AlignRight().Text((d.TotalAmount ?? 0).ToString("0.00"));
                                Bordered(table.Cell()).Text(observationAndRca);

                                srNo++;
                            }

                            if (partLines.Any())
                            {
                                table.Cell().ColumnSpan(4).Padding(3).AlignRight().Text("Total:").Bold();
                                table.Cell().Padding(3);
                                table.Cell().Padding(3).AlignRight().Text(partCgstTotal.ToString("0.00")).Bold();
                                table.Cell().Padding(3).AlignRight().Text(partSgstTotal.ToString("0.00")).Bold();
                                table.Cell().Padding(3).AlignRight().Text(partIgstTotal.ToString("0.00")).Bold();
                                table.Cell().Padding(3).AlignRight().Text(partGstTotal.ToString("0.00")).Bold();
                                table.Cell().Padding(3).AlignRight().Text(partAmountTotal.ToString("0.00")).Bold();
                                table.Cell().Padding(3).AlignRight().Text(partGrandTotal.ToString("0.00")).Bold();
                                table.Cell().Padding(3);
                            }
                        });

                        // ---- Labour Details ----
                        var labourLines = claim.WarrantyJcclaimDetails.Where(d => d.ItemType == "Labour").ToList();

                        col.Item().PaddingTop(6).Text("Labour Details").FontSize(7).Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(18);
                                columns.RelativeColumn(1.0f);
                                columns.RelativeColumn(1.3f);
                                columns.ConstantColumn(20);
                                columns.RelativeColumn(0.75f);
                                columns.RelativeColumn(1.0f);
                                columns.RelativeColumn(1.0f);
                                columns.RelativeColumn(1.0f);
                                columns.RelativeColumn(0.85f);
                                columns.RelativeColumn(0.85f);
                                columns.RelativeColumn(0.85f);
                                columns.RelativeColumn(1.6f);
                            });

                            void LH(string t) => Bordered(table.Cell()).Background(Colors.Grey.Lighten2).Text(t).FontSize(5f).Bold();

                            LH("Sr.No"); LH("Labour Code"); LH("Description"); LH("Qty.");
                            LH("Rate");
                            LH("CGST % / Amt"); LH("SGST % / Amt"); LH("IGST % / Amt");
                            LH("Total GST"); LH("Amount"); LH("Total"); LH("Observation & RCA");

                            int srNo = 1;
                            decimal labourAmountTotal = 0, labourCgstTotal = 0, labourSgstTotal = 0, labourIgstTotal = 0, labourGstTotal = 0, labourGrandTotal = 0;

                            foreach (var d in labourLines)
                            {
                                var rbd = d.RepairBillDetail;
                                string code = rbd?.LabourMaster?.LabourCode ?? "";
                                string desc = rbd?.LabourMaster?.LabourDescription ?? "";

                                decimal cgstPercent = rbd?.LabourMaster?.Cgst ?? 0;
                                decimal cgstAmount = rbd?.Cgstamount ?? 0;
                                decimal sgstPercent = rbd?.LabourMaster?.Sgst ?? 0;
                                decimal sgstAmount = rbd?.Sgstamount ?? 0;
                                decimal igstPercent = rbd?.LabourMaster?.Igst ?? 0;
                                decimal igstAmount = rbd?.Igstamount ?? 0;

                                decimal totalGstAmount = cgstAmount + sgstAmount + igstAmount;

                                string observationAndRca = string.Join(" / ", new[] { d.DealerObservation, d.RootCauseAnalysis }
                                    .Where(s => !string.IsNullOrWhiteSpace(s)));

                                labourAmountTotal += d.Amount ?? 0;
                                labourCgstTotal += cgstAmount;
                                labourSgstTotal += sgstAmount;
                                labourIgstTotal += igstAmount;
                                labourGstTotal += totalGstAmount;
                                labourGrandTotal += d.TotalAmount ?? 0;

                                Bordered(table.Cell()).Text(srNo.ToString());
                                Bordered(table.Cell()).Text(code);
                                Bordered(table.Cell()).Text(desc);
                                Bordered(table.Cell()).Text((d.Qty ?? 0).ToString());
                                Bordered(table.Cell()).AlignRight().Text((d.Rate ?? 0).ToString("0.00"));
                                Bordered(table.Cell()).AlignRight().Text($"{cgstPercent:0.##}% / {cgstAmount:0.00}");
                                Bordered(table.Cell()).AlignRight().Text($"{sgstPercent:0.##}% / {sgstAmount:0.00}");
                                Bordered(table.Cell()).AlignRight().Text($"{igstPercent:0.##}% / {igstAmount:0.00}");
                                Bordered(table.Cell()).AlignRight().Text(totalGstAmount.ToString("0.00")).Bold();
                                Bordered(table.Cell()).AlignRight().Text((d.Amount ?? 0).ToString("0.00"));
                                Bordered(table.Cell()).AlignRight().Text((d.TotalAmount ?? 0).ToString("0.00"));
                                Bordered(table.Cell()).Text(observationAndRca);

                                srNo++;
                            }

                            if (labourLines.Any())
                            {
                                table.Cell().ColumnSpan(4).Padding(3).AlignRight().Text("Total:").Bold();
                                table.Cell().Padding(3);
                                table.Cell().Padding(3).AlignRight().Text(labourCgstTotal.ToString("0.00")).Bold();
                                table.Cell().Padding(3).AlignRight().Text(labourSgstTotal.ToString("0.00")).Bold();
                                table.Cell().Padding(3).AlignRight().Text(labourIgstTotal.ToString("0.00")).Bold();
                                table.Cell().Padding(3).AlignRight().Text(labourGstTotal.ToString("0.00")).Bold();
                                table.Cell().Padding(3).AlignRight().Text(labourAmountTotal.ToString("0.00")).Bold();
                                table.Cell().Padding(3).AlignRight().Text(labourGrandTotal.ToString("0.00")).Bold();
                                table.Cell().Padding(3);
                            }
                        });

                        var grandTotal = partLines.Sum(d => d.TotalAmount ?? 0) + labourLines.Sum(d => d.TotalAmount ?? 0);
                        col.Item().PaddingTop(4).AlignRight().Text($"Grand Total: {grandTotal:0.00}").FontSize(8).Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn();
                            });

                            Bordered(table.Cell()).Text("Voice of Customers : ");
                            Bordered(table.Cell()).Text("ANALYSIS\nENGR.").FontSize(6).Bold();
                        });

                        col.Item().PaddingTop(6).Column(c =>
                        {
                            c.Item().Text("TERMS & CONDITIONS").Bold();

                            if (termsAndConditions.Any())
                            {
                                int termNo = 1;
                                foreach (var term in termsAndConditions)
                                {
                                    c.Item().Text($"{termNo}. {term}");
                                    termNo++;
                                }
                            }
                            else
                            {
                                c.Item().Text("No terms and conditions configured for this module.").FontSize(6).Italic();
                            }
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

        public async Task<bool> UpdateWarrantyJCClaim(WarrantyJCClaimUpdateViewModel model)
        {
            var claim = await _context.WarrantyJcclaims
                .FirstOrDefaultAsync(x => x.Id == model.ClaimId);

            if (claim == null)
                return false;

            foreach (var lineUpdate in model.Lines)
            {
                var detail = await _context.WarrantyJcclaimDetails
                    .FirstOrDefaultAsync(d => d.Id == lineUpdate.DetailId && d.WarrantyJcclaimHeaderId == model.ClaimId);

                if (detail == null)
                    continue;

                detail.DealerObservation = lineUpdate.DealerObservation;
                detail.RootCauseAnalysis = lineUpdate.RootCauseAnalysis;
            }

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<(bool Success, string? ErrorMessage)> DeleteWarrantyJCClaim(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var claim = await _context.WarrantyJcclaims
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (claim == null)
                {
                    await transaction.RollbackAsync();
                    return (false, $"Warranty Claim with Id {id} not found.");
                }

                var orderLinks = await _context.WarrantyOrderDetails
                    .Where(d => d.WarrantyJcclaimId == id)
                    .ToListAsync();
                _context.WarrantyOrderDetails.RemoveRange(orderLinks);
                var gridSnapshotRows = await _context.WarrantyOrderGridDetails
                    .Where(g => g.WarrantyJcclaimId == id)
                    .ToListAsync();
                _context.WarrantyOrderGridDetails.RemoveRange(gridSnapshotRows);

                var details = await _context.WarrantyJcclaimDetails
                    .Where(d => d.WarrantyJcclaimHeaderId == id)
                    .ToListAsync();
                _context.WarrantyJcclaimDetails.RemoveRange(details);

                _context.WarrantyJcclaims.Remove(claim);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return (true, null);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}