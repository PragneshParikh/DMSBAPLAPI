using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.WarrantyOrderRepo
{
    public class WarrantyOrderRepo : IWarrantyOrderRepo
    {
        private readonly BapldmsvadContext _context;

        public WarrantyOrderRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        public async Task<int> InsertWarrantyOrder(WarrantyOrderViewModel model, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var header = new WarrantyOrder
                {
                    DealerCode = model.DealerCode,
                    DateFrom = model.DateFrom!.Value,
                    DateTo = model.DateTo!.Value,
                    BatchNo = model.BatchNo!,
                    BatchDate = model.BatchDate!.Value,
                    OrderNo = model.OrderNo!,
                    OrderDate = model.OrderDate!.Value,
                    Location = model.Location!,
                    ClaimType = model.ClaimType!,
                    SupplierId = model.SupplierId!.Value,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    IsApproved = model.IsApproved
                };

                _context.WarrantyOrders.Add(header);
                await _context.SaveChangesAsync();

                if (model.WarrantyClaimIds != null && model.WarrantyClaimIds.Any())
                {
                    var details = model.WarrantyClaimIds.Select(claimId => new WarrantyOrderDetail
                    {
                        WarrantyOrderHeaderId = header.Id,
                        WarrantyJcclaimId = claimId,
                        IsApproved = model.ClaimApprovals?.FirstOrDefault(a => a.ClaimId == claimId)?.IsApproved ?? false,
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now
                    }).ToList();

                    _context.WarrantyOrderDetails.AddRange(details);
                    await _context.SaveChangesAsync();
                    foreach (var claimId in model.WarrantyClaimIds)
                    {
                        await SnapshotClaimGridRows(header.Id, claimId, userId);
                    }
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return header.Id;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateWarrantyOrder(WarrantyOrderViewModel model, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var header = await _context.WarrantyOrders
                    .Include(x => x.WarrantyOrderDetails)
                    .FirstOrDefaultAsync(x => x.Id == model.Id && x.IsActive);

                if (header == null)
                    return false;

                header.DateFrom = model.DateFrom!.Value;
                header.DateTo = model.DateTo!.Value;
                header.BatchNo = model.BatchNo!;
                header.BatchDate = model.BatchDate!.Value;
                header.OrderNo = model.OrderNo!;
                header.OrderDate = model.OrderDate!.Value;
                header.Location = model.Location!;
                header.ClaimType = model.ClaimType!;
                header.SupplierId = model.SupplierId!.Value;
                header.IsApproved = model.IsApproved;
                header.UpdatedBy = userId;
                header.UpdatedDate = DateTime.Now;
                _context.WarrantyOrderDetails.RemoveRange(header.WarrantyOrderDetails);
                var oldGridRows = await _context.WarrantyOrderGridDetails
                    .Where(g => g.WarrantyOrderHeaderId == header.Id)
                    .ToListAsync();
                _context.WarrantyOrderGridDetails.RemoveRange(oldGridRows);

                if (model.WarrantyClaimIds != null && model.WarrantyClaimIds.Any())
                {
                    var newDetails = model.WarrantyClaimIds.Select(claimId => new WarrantyOrderDetail
                    {
                        WarrantyOrderHeaderId = header.Id,
                        WarrantyJcclaimId = claimId,
                        IsApproved = model.ClaimApprovals?.FirstOrDefault(a => a.ClaimId == claimId)?.IsApproved ?? false,
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now
                    }).ToList();

                    await _context.WarrantyOrderDetails.AddRangeAsync(newDetails);
                    await _context.SaveChangesAsync();

                    foreach (var claimId in model.WarrantyClaimIds)
                    {
                        await SnapshotClaimGridRows(header.Id, claimId, userId);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DeleteWarrantyOrder(int id, string userId)
        {
            var header = await _context.WarrantyOrders
                .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

            if (header == null)
                return false;
            header.IsActive = false;
            header.UpdatedBy = userId;
            header.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<WarrantyOrderViewModel?> GetWarrantyOrderById(int id)
        {
            var header = await _context.WarrantyOrders
                .Include(x => x.WarrantyOrderDetails)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (header == null)
                return null;
            var gridRows = await _context.WarrantyOrderGridDetails
                .Where(g => g.WarrantyOrderHeaderId == id)
                .ToListAsync();
            var approvalByClaimId = header.WarrantyOrderDetails
                .ToDictionary(d => d.WarrantyJcclaimId, d => d.IsApproved);

            var claims = gridRows
                .GroupBy(g => g.WarrantyJcclaimId)
                .Select(grp =>
                {
                    var first = grp.First();
                    return new WarrantyJCClaimFullViewModel
                    {
                        Id = grp.Key,
                        ClaimPrefix = first.ClaimNo,
                        ClaimNo = null,
                        ClaimDate = first.ClaimDate,
                        JobCardNo = first.JobCardNo,
                        JobCardDate = first.JobCardDate,
                        InvoiceNo = first.InvoiceNo,
                        InvoiceDate = first.InvoiceDate,
                        ServiceHead = first.ServiceHead,
                        Kms = first.Kms,
                        LocationName = first.LocationName,
                        ChassisNo = first.ChassisNo,
                        MotorNo = first.MotorNo,
                        PartyName = first.PartyName,
                        IsApproved = approvalByClaimId.TryGetValue(grp.Key, out var approved) && approved,
                        Details = grp
                            .Where(g => g.ItemType != null) 
                            .Select(g => new WarrantyJCClaimDetailLineViewModel
                            {
                                ItemType = g.ItemType,
                                PartCode = g.PartCode,
                                PartName = g.PartName,
                                PartDescription = g.PartDescription,
                                LabourCode = g.LabourCode,
                                LabourDescription = g.LabourDescription,
                                Quantity = g.Quantity ?? 0,
                                CgstPercent = g.CgstPercent ?? 0,
                                CgstAmount = g.CgstAmount ?? 0,
                                SgstPercent = g.SgstPercent ?? 0,
                                SgstAmount = g.SgstAmount ?? 0,
                                IgstPercent = g.IgstPercent ?? 0,
                                IgstAmount = g.IgstAmount ?? 0,
                                TotalAmount = g.TotalAmount ?? 0,
                                Mrp = g.Mrp,
                                Rate = g.Mrp
                            }).ToList()
                    };
                })
                .ToList();

            var headerLocationName = !string.IsNullOrWhiteSpace(header.Location)
                ? await _context.LocationMasters
                    .Where(l => l.Loccode == header.Location)
                    .Select(l => l.Locname)
                    .FirstOrDefaultAsync()
                : null;

            return new WarrantyOrderViewModel
            {
                Id = header.Id,
                DealerCode = header.DealerCode,
                DateFrom = header.DateFrom,
                DateTo = header.DateTo,
                BatchNo = header.BatchNo,
                BatchDate = header.BatchDate,
                OrderNo = header.OrderNo,
                OrderDate = header.OrderDate,
                Location = header.Location,
                LocationName = headerLocationName,
                ClaimType = header.ClaimType,
                SupplierId = header.SupplierId,
                IsApproved = header.IsApproved,
                IsActive = header.IsActive,
                WarrantyClaimIds = header.WarrantyOrderDetails.Select(d => d.WarrantyJcclaimId).ToList(),
                Claims = claims
            };
        }

        public async Task<WarrantyOrderSearchResultViewModel> SearchWarrantyOrders(WarrantyOrderSearchViewModel filter)
        {
            var query = _context.WarrantyOrders
                .Include(x => x.WarrantyOrderDetails)
                .AsQueryable();

            if (!filter.IncludeInactive)
                query = query.Where(x => x.IsActive);

            if (filter.DateFrom.HasValue)
                query = query.Where(x => x.OrderDate >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(x => x.OrderDate <= filter.DateTo.Value);

            if (!string.IsNullOrWhiteSpace(filter.BatchNo))
                query = query.Where(x => x.BatchNo.Contains(filter.BatchNo));

            if (filter.BatchDate.HasValue)
                query = query.Where(x => x.BatchDate.Date == filter.BatchDate.Value.Date);

            if (!string.IsNullOrWhiteSpace(filter.OrderNo))
                query = query.Where(x => x.OrderNo.Contains(filter.OrderNo));

            if (filter.OrderDate.HasValue)
                query = query.Where(x => x.OrderDate.Date == filter.OrderDate.Value.Date);

            if (!string.IsNullOrWhiteSpace(filter.Location))
                query = query.Where(x => x.Location == filter.Location);

            if (!string.IsNullOrWhiteSpace(filter.ClaimType))
                query = query.Where(x => x.ClaimType == filter.ClaimType);

            if (filter.SupplierId.HasValue)
                query = query.Where(x => x.SupplierId == filter.SupplierId.Value);

            if (filter.IsApproved.HasValue)
                query = query.Where(x => x.IsApproved == filter.IsApproved.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.OrderDate)
                .ThenByDescending(x => x.Id)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(x => new WarrantyOrderListViewModel
                {
                    Id = x.Id,
                    BatchNo = x.BatchNo,
                    BatchDate = x.BatchDate,
                    OrderNo = x.OrderNo,
                    OrderDate = x.OrderDate,
                    Location = x.Location,
                    ClaimType = x.ClaimType,
                    SupplierId = x.SupplierId,
                    TotalClaims = x.WarrantyOrderDetails.Count,
                    TotalMrp = _context.WarrantyOrderGridDetails
                        .Where(g => g.WarrantyOrderHeaderId == x.Id)
                        .Sum(g => (decimal?)g.Mrp) ?? 0,
                    IsApproved = x.IsApproved,
                    IsActive = x.IsActive
                })
                .ToListAsync();
            return new WarrantyOrderSearchResultViewModel
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task<NextOrderNumberViewModel> GetNextOrderNumbers(string dealerCode)
        {
            var today = DateTime.Now;
            int fyStartYear = today.Month >= 4 ? today.Year : today.Year - 1;
            int fyEndYear = fyStartYear + 1;
            string fySuffix = $"{fyStartYear % 100:D2}-{fyEndYear % 100:D2}";
            string batchSuffix = $"BT/{fySuffix}";
            var existingBatchNos = await _context.WarrantyOrders
                .Where(x => x.DealerCode == dealerCode && x.BatchNo.EndsWith("/" + batchSuffix))
                .Select(x => x.BatchNo)
                .ToListAsync();

            int maxBatchSeq = 0;
            foreach (var b in existingBatchNos)
            {
                var numPart = b.Split('/')[0];
                if (int.TryParse(numPart, out int seq) && seq > maxBatchSeq)
                    maxBatchSeq = seq;
            }
            string nextBatchNo = $"{maxBatchSeq + 1}/{batchSuffix}";
            var existingOrderNos = await _context.WarrantyOrders
                .Where(x => x.DealerCode == dealerCode)
                .Select(x => x.OrderNo)
                .ToListAsync();

            int maxOrderSeq = 0;
            foreach (var o in existingOrderNos)
            {
                if (int.TryParse(o, out int seq) && seq > maxOrderSeq)
                    maxOrderSeq = seq;
            }
            string nextOrderNo = (maxOrderSeq + 1).ToString();

            return new NextOrderNumberViewModel
            {
                BatchNo = nextBatchNo,
                OrderNo = nextOrderNo
            };
        }

        public async Task<WarrantyJCClaimFullViewModel?> GetWarrantyJCClaimById(int id)
        {
            return await BuildClaimFullViewModel(id);
        }

        private async Task<WarrantyJCClaimFullViewModel?> BuildClaimFullViewModel(int id)
        {
            var claim = await _context.WarrantyJcclaims
                .Include(x => x.Supplier)
                .Include(x => x.JobCardHeader)
                    .ThenInclude(jc => jc.ServiceheadNavigation)
                .Include(x => x.RepairBillHeader)
                .Include(x => x.WarrantyJcclaimDetails)
                    .ThenInclude(d => d.RepairBillDetail)
                        .ThenInclude(rb => rb.PartItem)
                .Include(x => x.WarrantyJcclaimDetails)
                    .ThenInclude(d => d.RepairBillDetail)
                        .ThenInclude(rb => rb.LabourMaster)
                .Include(x => x.WarrantyJcclaimDetails)
                    .ThenInclude(d => d.RepairBillDetail)
                        .ThenInclude(rb => rb.PartWiseLabour)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (claim == null)
                return null;
            var motorNo = await _context.ChassisBatteryDetails
                .Where(x => x.ChassisNo == claim.ChassisNo)
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => x.MotorNo)
                .FirstOrDefaultAsync();

            var locationName = claim.JobCardHeader != null
                ? await _context.LocationMasters
                    .Where(l => l.Loccode == claim.JobCardHeader.Serviceloc)
                    .Select(l => l.Locname)
                    .FirstOrDefaultAsync()
                : null;

            return new WarrantyJCClaimFullViewModel
            {
                Id = claim.Id,
                ClaimPrefix = claim.ClaimPrefix,
                ClaimNo = claim.ClaimNo,
                ClaimDate = claim.ClaimDate,
                ChassisNo = claim.ChassisNo,

                JobCardNo = claim.JobCardHeader != null
                    ? $"{claim.JobCardHeader.Jobprefix}{claim.JobCardHeader.JobNo}"
                    : null,
                JobCardDate = claim.JobCardHeader?.JobinDate?.ToDateTime(TimeOnly.MinValue),

                InvoiceNo = claim.RepairBillHeader != null
                    ? $"{claim.RepairBillHeader.Prefix}{claim.RepairBillHeader.BillNo}"
                    : null,
                InvoiceDate = claim.RepairBillHeader?.CreatedDate,

                ServiceHead = claim.JobCardHeader?.ServiceheadNavigation?.ServiceHeadName,
                Kms = claim.JobCardHeader?.Vehiclekms,

                MotorNo = motorNo,
                PartyName = claim.Supplier?.LedgerName,

                SupplierId = claim.SupplierId,
                ServiceLocation = claim.JobCardHeader?.Serviceloc,
                LocationName = locationName,

                Details = claim.WarrantyJcclaimDetails.Select(d =>
                {
                    bool isLabour = d.ItemType == "Labour";
                    var rbd = d.RepairBillDetail;

                    var cgstAmount = rbd?.Cgstamount ?? 0;
                    var sgstAmount = rbd?.Sgstamount ?? 0;
                    var dlrPrice = !isLabour
                        ? (rbd?.PartItem?.Dlrprice ?? 0)
                        : ((rbd?.LabourMaster?.LabourRate ?? rbd?.PartWiseLabour?.LabourRate) ?? 0);
                    var mrp = cgstAmount + sgstAmount + dlrPrice;

                    return new WarrantyJCClaimDetailLineViewModel
                    {
                        Id = d.Id,
                        ItemType = d.ItemType,

                        PartCode = !isLabour ? rbd?.PartItem?.Itemcode : null,
                        PartName = !isLabour ? rbd?.PartItem?.Itemname : null,
                        PartDescription = !isLabour ? rbd?.PartItem?.Itemdesc : null,

                        LabourCode = isLabour
                            ? (rbd?.LabourMaster?.LabourCode ?? rbd?.PartWiseLabour?.LabourCode)
                            : null,
                        LabourDescription = isLabour
                            ? (rbd?.LabourMaster?.LabourDescription ?? rbd?.PartWiseLabour?.LabourName)
                            : null,

                        Quantity = d.Qty ?? 0,

                        CgstPercent = isLabour
                            ? (rbd?.LabourMaster?.Cgst ?? rbd?.PartWiseLabour?.Cgst ?? 0)
                            : (rbd?.PartItem?.Cgst ?? 0),
                        CgstAmount = cgstAmount,

                        SgstPercent = isLabour
                            ? (rbd?.LabourMaster?.Sgst ?? rbd?.PartWiseLabour?.Sgst ?? 0)
                            : (rbd?.PartItem?.Sgst ?? 0),
                        SgstAmount = sgstAmount,

                        IgstPercent = isLabour
                            ? (rbd?.LabourMaster?.Igst ?? rbd?.PartWiseLabour?.Igst ?? 0)
                            : (rbd?.PartItem?.Igst ?? 0),
                        IgstAmount = rbd?.Igstamount ?? 0,

                        TotalAmount = d.TotalAmount ?? 0,
                        Mrp = mrp,
                        Rate = mrp,
                        DealerObservation = d.DealerObservation,
                        RootCauseAnalysis = d.RootCauseAnalysis
                    };
                }).ToList()
            };
        }

    
        private async Task SnapshotClaimGridRows(int orderHeaderId, int claimId, string userId)
        {
            var claim = await BuildClaimFullViewModel(claimId);
            if (claim == null)
                return;

            var claimNoDisplay = $"{claim.ClaimPrefix}{claim.ClaimNo}";

            if (claim.Details == null || !claim.Details.Any())
            {
                _context.WarrantyOrderGridDetails.Add(new WarrantyOrderGridDetail
                {
                    WarrantyOrderHeaderId = orderHeaderId,
                    WarrantyJcclaimId = claimId,
                    ClaimNo = claimNoDisplay,
                    ClaimDate = claim.ClaimDate,
                    JobCardNo = claim.JobCardNo,
                    JobCardDate = claim.JobCardDate,
                    InvoiceNo = claim.InvoiceNo,
                    InvoiceDate = claim.InvoiceDate,
                    ServiceHead = claim.ServiceHead,
                    Kms = claim.Kms,
                    LocationName = claim.LocationName,
                    ChassisNo = claim.ChassisNo,
                    MotorNo = claim.MotorNo,
                    PartyName = claim.PartyName,
                    ItemType = null, 
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now
                });
                return;
            }

            foreach (var line in claim.Details)
            {
                _context.WarrantyOrderGridDetails.Add(new WarrantyOrderGridDetail
                {
                    WarrantyOrderHeaderId = orderHeaderId,
                    WarrantyJcclaimId = claimId,
                    ClaimNo = claimNoDisplay,
                    ClaimDate = claim.ClaimDate,
                    JobCardNo = claim.JobCardNo,
                    JobCardDate = claim.JobCardDate,
                    InvoiceNo = claim.InvoiceNo,
                    InvoiceDate = claim.InvoiceDate,
                    ServiceHead = claim.ServiceHead,
                    Kms = claim.Kms,
                    LocationName = claim.LocationName,
                    ChassisNo = claim.ChassisNo,
                    MotorNo = claim.MotorNo,
                    PartyName = claim.PartyName,
                    ItemType = line.ItemType,
                    PartName = line.PartName,
                    PartDescription = line.PartDescription,
                    PartCode = line.PartCode,
                    LabourCode = line.LabourCode,
                    LabourDescription = line.LabourDescription,
                    Quantity = line.Quantity,
                    CgstPercent = line.CgstPercent,
                    CgstAmount = line.CgstAmount,
                    SgstPercent = line.SgstPercent,
                    SgstAmount = line.SgstAmount,
                    IgstPercent = line.IgstPercent,
                    IgstAmount = line.IgstAmount,
                    TotalAmount = line.TotalAmount,
                    Mrp = line.Mrp,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now
                });
            }
        }

        public async Task<byte[]> GenerateWarrantyOrderPdf(int id)
        {
            var order = await GetWarrantyOrderById(id);
            if (order == null)
                throw new InvalidOperationException($"Warranty Order with Id {id} not found.");

            var claims = order.Claims ?? new List<WarrantyJCClaimFullViewModel>();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(7));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Warranty Order").FontSize(14).Bold();
                        col.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().Text($"Batch No: {order.BatchNo}");
                            row.RelativeItem().Text($"Batch Date: {order.BatchDate:dd-MM-yyyy}");
                            row.RelativeItem().Text($"Order No: {order.OrderNo}");
                            row.RelativeItem().Text($"Order Date: {order.OrderDate:dd-MM-yyyy}");
                        });
                        col.Item().PaddingTop(2).Row(row =>
                        {
                            row.RelativeItem().Text($"Date From: {order.DateFrom:dd-MM-yyyy}");
                            row.RelativeItem().Text($"Date To: {order.DateTo:dd-MM-yyyy}");
                            row.RelativeItem().Text($"Location: {order.Location}");
                            row.RelativeItem().Text($"Claim Type: {order.ClaimType}");
                        });
                    });

                    page.Content().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(20);   // Sr.No
                            columns.RelativeColumn(1.3f); // Claim No/Date
                            columns.RelativeColumn(1.3f); // JobCard No/Date
                            columns.RelativeColumn(1.3f); // Invoice No/Date
                            columns.RelativeColumn(1.1f); // Service Head/KMS
                            columns.RelativeColumn(1.2f); // Location Name
                            columns.RelativeColumn(1.3f); // Chassis No
                            columns.RelativeColumn(1.1f); // Motor No
                            columns.RelativeColumn(1.4f); // Party
                            columns.RelativeColumn(1.1f); // Part Name
                            columns.RelativeColumn(1.3f); // Part Description
                            columns.RelativeColumn(0.9f); // Part Code
                            columns.RelativeColumn(0.9f); // Labor Code
                            columns.RelativeColumn(1.2f); // Labor Description
                            columns.ConstantColumn(30);   // Qty
                            columns.RelativeColumn(0.9f); // CGST
                            columns.RelativeColumn(0.9f); // SGST
                            columns.RelativeColumn(0.9f); // IGST
                            columns.RelativeColumn(1.0f); // Total
                        });

                        void HeaderCell(QuestPDF.Infrastructure.IContainer c, string text) =>
                            c.Background(Colors.Grey.Lighten2).Padding(2).Text(text).Bold();

                        table.Header(header =>
                        {
                            HeaderCell(header.Cell(), "Sr.No");
                            HeaderCell(header.Cell(), "Claim No / Date");
                            HeaderCell(header.Cell(), "JobCard No / Date");
                            HeaderCell(header.Cell(), "Invoice No / Date");
                            HeaderCell(header.Cell(), "Service Head / KMS");
                            HeaderCell(header.Cell(), "Location Name");
                            HeaderCell(header.Cell(), "Chassis No");
                            HeaderCell(header.Cell(), "Motor No");
                            HeaderCell(header.Cell(), "Party");
                            HeaderCell(header.Cell(), "Part Name");
                            HeaderCell(header.Cell(), "Part Description");
                            HeaderCell(header.Cell(), "Part Code");
                            HeaderCell(header.Cell(), "Labor Code");
                            HeaderCell(header.Cell(), "Labor Description");
                            HeaderCell(header.Cell(), "Qty");
                            HeaderCell(header.Cell(), "CGST");
                            HeaderCell(header.Cell(), "SGST");
                            HeaderCell(header.Cell(), "IGST");
                            HeaderCell(header.Cell(), "Total Amount");
                        });

                        int srNo = 1;
                        decimal grandTotal = 0;

                        foreach (var claim in claims)
                        {
                            var lines = (claim.Details != null && claim.Details.Any())
                                ? claim.Details
                                : new List<WarrantyJCClaimDetailLineViewModel> { new WarrantyJCClaimDetailLineViewModel() };

                            foreach (var line in lines)
                            {
                                grandTotal += line.TotalAmount;

                                table.Cell().Padding(2).Text(srNo++.ToString());
                                table.Cell().Padding(2).Text($"{claim.ClaimPrefix}{claim.ClaimNo}\n{claim.ClaimDate:dd-MM-yyyy}");
                                table.Cell().Padding(2).Text($"{claim.JobCardNo}\n{claim.JobCardDate:dd-MM-yyyy}");
                                table.Cell().Padding(2).Text($"{claim.InvoiceNo}\n{claim.InvoiceDate:dd-MM-yyyy}");
                                table.Cell().Padding(2).Text($"{claim.ServiceHead}\n{claim.Kms} km");
                                table.Cell().Padding(2).Text(claim.LocationName);
                                table.Cell().Padding(2).Text(claim.ChassisNo);
                                table.Cell().Padding(2).Text(claim.MotorNo);
                                table.Cell().Padding(2).Text(claim.PartyName);
                                table.Cell().Padding(2).Text(line.PartName);
                                table.Cell().Padding(2).Text(line.PartDescription);
                                table.Cell().Padding(2).Text(line.PartCode);
                                table.Cell().Padding(2).Text(line.LabourCode);
                                table.Cell().Padding(2).Text(line.LabourDescription);
                                table.Cell().Padding(2).AlignRight().Text(line.Quantity.ToString());
                                table.Cell().Padding(2).AlignRight().Text($"{line.CgstPercent}%\n{line.CgstAmount:0.00}");
                                table.Cell().Padding(2).AlignRight().Text($"{line.SgstPercent}%\n{line.SgstAmount:0.00}");
                                table.Cell().Padding(2).AlignRight().Text($"{line.IgstPercent}%\n{line.IgstAmount:0.00}");
                                table.Cell().Padding(2).AlignRight().Text(line.TotalAmount.ToString("0.00"));
                            }
                        }

                        // Grand total row
                        table.Cell().ColumnSpan(18).Padding(2).AlignRight().Text("Grand Total:").Bold();
                        table.Cell().Padding(2).AlignRight().Text(grandTotal.ToString("0.00")).Bold();
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

   

        public async Task<List<string>> SearchBatchNos(string dealerCode, string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return new List<string>();

            return await _context.WarrantyOrders
                .Where(x => x.DealerCode == dealerCode && x.BatchNo.Contains(searchText))
                .Select(x => x.BatchNo)
                .Distinct()
                .OrderByDescending(b => b)
                .Take(20)
                .ToListAsync();
        }

        public async Task<List<string>> SearchOrderNos(string dealerCode, string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return new List<string>();

            return await _context.WarrantyOrders
                .Where(x => x.DealerCode == dealerCode && x.OrderNo.Contains(searchText))
                .Select(x => x.OrderNo)
                .Distinct()
                .OrderByDescending(o => o)
                .Take(20)
                .ToListAsync();
        }
        public async Task<List<LocationDropdownItemViewModel>> GetDistinctOrderLocations(string dealerCode)
        {
            var distinctCodes = await _context.WarrantyOrders
                .Where(x => x.DealerCode == dealerCode && x.IsActive && !string.IsNullOrWhiteSpace(x.Location))
                .Select(x => x.Location)
                .Distinct()
                .ToListAsync();

            if (distinctCodes.Count == 0)
                return new List<LocationDropdownItemViewModel>();

            var names = await _context.LocationMasters
                .Where(l => distinctCodes.Contains(l.Loccode))
                .Select(l => new { l.Loccode, l.Locname })
                .ToListAsync();

            var nameByCode = names.ToDictionary(n => n.Loccode, n => n.Locname);

            return distinctCodes
                .Select(code => new LocationDropdownItemViewModel
                {
                    Loccode = code,
                    Locname = nameByCode.TryGetValue(code, out var name) ? name : code
                })
                .OrderBy(l => l.Locname)
                .ToList();
        }
    }
}