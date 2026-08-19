using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using DMS_BAPL_Utils.Helpers;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DMS_BAPL_Data.Repositories.WarrantyInvoiceRepo
{


    public class WarrantyInvoiceRepo : IWarrantyInvoiceRepo
    {
        private readonly BapldmsvadContext _context;

        public WarrantyInvoiceRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        // --- Insert ---------------------------------------------------------

        public async Task<int> InsertWarrantyInvoice(WarrantyInvoiceViewModel model, string userId)
        {
            var header = new WarrantyInvoice
            {
                DealerCode = model.DealerCode!,
                DateFrom = model.DateFrom!.Value,
                DateTo = model.DateTo!.Value,
                BatchNo = model.BatchNo,
                BatchDate = model.BatchDate,
                InvoicePrefix = model.InvoicePrefix,
                InvoiceNo = model.InvoiceNo,
                InvoiceDate = model.InvoiceDate,
                ClaimType = model.ClaimType,
                SupplierId = model.SupplierId,
                IsApproved = model.IsApproved,
                IsActive = true,
                CreatedBy = userId,
                CreatedDate = DateTime.Now
            };

            _context.WarrantyInvoices.Add(header);
            await _context.SaveChangesAsync();

            await InsertOrderLinksAndSnapshot(header.Id, model, userId);

            return header.Id;
        }

        // --- Update -----------------------------------------------------------
        // Wholesale-replaces WarrantyInvoiceDetail AND WarrantyInvoiceGridDetail,
        // same pattern as UpdateWarrantyOrder - simpler and less error-prone
        // than diffing which orders were added/removed.

        public async Task<bool> UpdateWarrantyInvoice(WarrantyInvoiceViewModel model, string userId)
        {
            var header = await _context.WarrantyInvoices
                .FirstOrDefaultAsync(x => x.Id == model.Id && x.IsActive);

            if (header == null)
                return false;

            header.DateFrom = model.DateFrom!.Value;
            header.DateTo = model.DateTo!.Value;
            header.BatchNo = model.BatchNo;
            header.BatchDate = model.BatchDate;
            header.InvoicePrefix = model.InvoicePrefix;
            header.InvoiceNo = model.InvoiceNo;
            header.InvoiceDate = model.InvoiceDate;
            header.ClaimType = model.ClaimType;
            header.SupplierId = model.SupplierId;
            header.IsApproved = model.IsApproved;
            header.UpdatedBy = userId;
            header.UpdatedDate = DateTime.Now;

            var oldDetails = await _context.WarrantyInvoiceDetails
                .Where(d => d.WarrantyInvoiceHeaderId == header.Id)
                .ToListAsync();
            _context.WarrantyInvoiceDetails.RemoveRange(oldDetails);

            var oldGridRows = await _context.WarrantyInvoiceGridDetails
                .Where(g => g.WarrantyInvoiceHeaderId == header.Id)
                .ToListAsync();
            _context.WarrantyInvoiceGridDetails.RemoveRange(oldGridRows);

            await _context.SaveChangesAsync();

            await InsertOrderLinksAndSnapshot(header.Id, model, userId);

            return true;
        }

        // Shared by Insert and Update - creates the WarrantyInvoiceDetail link
        // rows and snapshots each linked order's resolved data into
        // WarrantyInvoiceGridDetail.
        private async Task InsertOrderLinksAndSnapshot(int invoiceHeaderId, WarrantyInvoiceViewModel model, string userId)
        {
            var approvalByOrderId = model.OrderApprovals?.ToDictionary(a => a.OrderId, a => a.IsApproved)
                ?? new Dictionary<int, bool>();

            foreach (var orderId in model.WarrantyOrderIds)
            {
                var isApproved = approvalByOrderId.TryGetValue(orderId, out var approved) && approved;

                _context.WarrantyInvoiceDetails.Add(new WarrantyInvoiceDetail
                {
                    WarrantyInvoiceHeaderId = invoiceHeaderId,
                    WarrantyOrderHeaderId = orderId,
                    IsApproved = isApproved,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now
                });

                await SnapshotOrderGridRow(invoiceHeaderId, orderId);
            }

            await _context.SaveChangesAsync();
        }
        private async Task SnapshotOrderGridRow(int invoiceHeaderId, int orderId)
        {
            var order = await _context.WarrantyOrders
                .Include(o => o.WarrantyOrderDetails)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return;

            var locationName = !string.IsNullOrWhiteSpace(order.Location)
                ? await _context.LocationMasters
                    .Where(l => l.Loccode == order.Location)
                    .Select(l => l.Locname)
                    .FirstOrDefaultAsync()
                : null;
            var partyName = order.SupplierId > 0
                ? await _context.LedgerMasters
                    .Where(l => l.Id == order.SupplierId)
                    .Select(l => l.LedgerName)
                    .FirstOrDefaultAsync()
                : null;

            var totalAmount = await _context.WarrantyOrderGridDetails
                .Where(g => g.WarrantyOrderHeaderId == orderId)
                .SumAsync(g => (decimal?)g.TotalAmount) ?? 0;

            var totalMrp = await _context.WarrantyOrderGridDetails
                .Where(g => g.WarrantyOrderHeaderId == orderId)
                .SumAsync(g => (decimal?)g.Mrp) ?? 0;

            _context.WarrantyInvoiceGridDetails.Add(new WarrantyInvoiceGridDetail
            {
                WarrantyInvoiceHeaderId = invoiceHeaderId,
                WarrantyOrderHeaderId = orderId,
                OrderNo = order.OrderNo,
                OrderDate = order.OrderDate,
                BatchNo = order.BatchNo,
                BatchDate = order.BatchDate,
                Location = order.Location,
                LocationName = locationName,
                ClaimType = order.ClaimType,
                SupplierId = order.SupplierId,
                PartyName = partyName,
                TotalClaims = order.WarrantyOrderDetails.Count,
                TotalAmount = totalAmount,
                TotalMrp = totalMrp
            });
        }


        public async Task<bool> DeleteWarrantyInvoice(int id, string userId)
        {
            var header = await _context.WarrantyInvoices
                .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

            if (header == null)
                return false;

            header.IsActive = false;
            header.UpdatedBy = userId;
            header.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<WarrantyInvoiceViewModel?> GetWarrantyInvoiceById(int id)
        {
            var header = await _context.WarrantyInvoices
                .FirstOrDefaultAsync(x => x.Id == id);

            if (header == null)
                return null;

            var approvalByOrderId = await _context.WarrantyInvoiceDetails
                .Where(d => d.WarrantyInvoiceHeaderId == id)
                .ToDictionaryAsync(d => d.WarrantyOrderHeaderId, d => d.IsApproved);

            var gridRows = await _context.WarrantyInvoiceGridDetails
                .Where(g => g.WarrantyInvoiceHeaderId == id)
                .ToListAsync();

            var orders = new List<WarrantyOrderSummaryViewModel>();
            foreach (var g in gridRows)
            {
                orders.Add(new WarrantyOrderSummaryViewModel
                {
                    Id = g.WarrantyOrderHeaderId,
                    OrderNo = g.OrderNo,
                    OrderDate = g.OrderDate,
                    BatchNo = g.BatchNo,
                    BatchDate = g.BatchDate,
                    Location = g.Location,
                    LocationName = g.LocationName,
                    ClaimType = g.ClaimType,
                    SupplierId = g.SupplierId,
                    PartyName = g.PartyName,
                    TotalClaims = g.TotalClaims,
                    TotalAmount = g.TotalAmount,
                    TotalMrp = g.TotalMrp,
                    IsApproved = approvalByOrderId.TryGetValue(g.WarrantyOrderHeaderId, out var approved) && approved,
                    InvoicePrefix = header.InvoicePrefix,
                    InvoiceNo = header.InvoiceNo,
                    InvoiceBatchNo = header.BatchNo,
                    Claims = await GetClaimsForOrder(g.WarrantyOrderHeaderId)
                });
            }

            return new WarrantyInvoiceViewModel
            {
                Id = header.Id,
                DealerCode = header.DealerCode,
                DateFrom = header.DateFrom,
                DateTo = header.DateTo,
                BatchNo = header.BatchNo,
                BatchDate = header.BatchDate,
                InvoicePrefix = header.InvoicePrefix,
                InvoiceNo = header.InvoiceNo,
                InvoiceDate = header.InvoiceDate,
                ClaimType = header.ClaimType,
                SupplierId = header.SupplierId,
                IsApproved = header.IsApproved,
                IsActive = header.IsActive,
                WarrantyOrderIds = orders.Select(o => o.Id).ToList(),
                Orders = orders
            };
        }
        private async Task<List<WarrantyJCClaimFullViewModel>> GetClaimsForOrder(int orderId)
        {
            var orderGridRows = await _context.WarrantyOrderGridDetails
                .Where(g => g.WarrantyOrderHeaderId == orderId)
                .ToListAsync();

            var approvalByClaimId = await _context.WarrantyOrderDetails
                .Where(d => d.WarrantyOrderHeaderId == orderId)
                .ToDictionaryAsync(d => d.WarrantyJcclaimId, d => d.IsApproved);

            return orderGridRows
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
                                Mrp = g.Mrp

                            }).ToList()
                    };
                }).ToList();
        }

        // --- Search / List ----------------------------------------------------

        public async Task<WarrantyInvoiceSearchResultViewModel> SearchWarrantyInvoices(WarrantyInvoiceSearchViewModel filter)
        {
            var query = _context.WarrantyInvoices.AsQueryable();

            if (!filter.IncludeInactive)
                query = query.Where(x => x.IsActive);

            // Scopes results to the requesting dealer - this was missing
            // entirely before, so SearchWarrantyInvoices was returning every
            // dealer's invoices globally rather than just the caller's own.
            if (!string.IsNullOrWhiteSpace(filter.DealerCode))
                query = query.Where(x => x.DealerCode == filter.DealerCode);

            if (filter.DateFrom.HasValue)
                query = query.Where(x => x.InvoiceDate >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(x => x.InvoiceDate <= filter.DateTo.Value);

            if (!string.IsNullOrWhiteSpace(filter.BatchNo))
                query = query.Where(x => x.BatchNo == filter.BatchNo);

            // Single InvoiceNo check, matched against InvoicePrefix+InvoiceNo
            // concatenated - the same value shown in the list grid and returned
            // by the typeahead (e.g. "WR/26-27/1") - not InvoiceNo alone.
            if (!string.IsNullOrWhiteSpace(filter.InvoiceNo))
                query = query.Where(x => (x.InvoicePrefix + x.InvoiceNo) == filter.InvoiceNo);

            if (!string.IsNullOrWhiteSpace(filter.ClaimType))
                query = query.Where(x => x.ClaimType == filter.ClaimType);

            if (filter.SupplierId.HasValue)
                query = query.Where(x => x.SupplierId == filter.SupplierId.Value);

            if (!string.IsNullOrWhiteSpace(filter.Location))
                query = query.Where(x => _context.WarrantyInvoiceGridDetails
                    .Any(g => g.WarrantyInvoiceHeaderId == x.Id && g.Location == filter.Location));

            if (filter.IsApproved.HasValue)
                query = query.Where(x => x.IsApproved == filter.IsApproved.Value);

            if (!string.IsNullOrWhiteSpace(filter.ClaimInvoiceNo))
                query = query.Where(x => _context.WarrantyInvoiceDetails
                    .Any(d => d.WarrantyInvoiceHeaderId == x.Id &&
                         _context.WarrantyOrderGridDetails.Any(g =>
                             g.WarrantyOrderHeaderId == d.WarrantyOrderHeaderId && g.InvoiceNo == filter.ClaimInvoiceNo)));

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.InvoiceDate)
                .ThenByDescending(x => x.Id)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(x => new WarrantyInvoiceListViewModel
                {
                    Id = x.Id,
                    BatchNo = x.BatchNo,
                    BatchDate = x.BatchDate,
                    InvoicePrefix = x.InvoicePrefix,
                    InvoiceNo = x.InvoiceNo,
                    InvoiceDate = x.InvoiceDate,
                    ClaimType = x.ClaimType,
                    SupplierId = x.SupplierId,
                    TotalOrders = x.WarrantyInvoiceDetails.Count,
                    IsApproved = x.IsApproved,
                    IsActive = x.IsActive,
                    DateFrom = x.DateFrom,
                    DateTo = x.DateTo
                })
                .ToListAsync();

            foreach (var item in items)
            {
                var itemTypes = await (
                    from d in _context.WarrantyInvoiceDetails
                    join g in _context.WarrantyOrderGridDetails
                        on d.WarrantyOrderHeaderId equals g.WarrantyOrderHeaderId
                    where d.WarrantyInvoiceHeaderId == item.Id && g.ItemType != null
                    select g.ItemType
                ).Distinct().ToListAsync();

                item.ClaimSubType = itemTypes.Count switch
                {
                    0 => null,
                    1 => $"Warranty -{(itemTypes[0] == "Labour" ? "Labour" : "Parts")}",
                    _ => "Warranty -Mixed"
                };

                item.LocationName = await _context.WarrantyInvoiceGridDetails
                    .Where(g => g.WarrantyInvoiceHeaderId == item.Id)
                    .OrderBy(g => g.Id)
                    .Select(g => g.LocationName)
                    .FirstOrDefaultAsync();
            }

            return new WarrantyInvoiceSearchResultViewModel
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
            };
        }

        public async Task<(string BatchNo, string InvoicePrefix, string InvoiceNo)> GetNextInvoiceNumbers(string dealerCode)
        {
            var today = DateTime.Now;
            var financialYearSuffix = GetFinancialYearSuffix(today);
            var batchPrefix = $"BT/{financialYearSuffix}/";

            var existingBatchNos = await _context.WarrantyInvoices
                .Where(x => x.DealerCode == dealerCode && x.BatchNo.StartsWith(batchPrefix))
                .Select(x => x.BatchNo)
                .ToListAsync();

            int maxBatchSeq = 0;
            foreach (var b in existingBatchNos)
            {
                var parts = b.Split('/');
                var numPart = parts.Length > 0 ? parts[parts.Length - 1] : null;
                if (int.TryParse(numPart, out int seq) && seq > maxBatchSeq)
                    maxBatchSeq = seq;
            }

            var batchNo = $"{batchPrefix}{maxBatchSeq + 1}";
            var invoicePrefix = $"WR/{financialYearSuffix}/";
            var invoiceNo = $"{maxBatchSeq + 1}";

            return (batchNo, invoicePrefix, invoiceNo);
        }

        private static string GetFinancialYearSuffix(DateTime date)
        {
            int startYear = date.Month >= 4 ? date.Year : date.Year - 1;
            int endYear = startYear + 1;
            return $"{startYear % 100:D2}-{endYear % 100:D2}";
        }


        private class InvoicePdfLine
        {
            public string? ClaimNoDisplay { get; set; }
            public DateTime? ClaimDate { get; set; }
            public string? ChassisNo { get; set; }
            public string? ModelName { get; set; }
            public string? BatteryType { get; set; }
            public string? PartCode { get; set; }
            public string? PartDescription { get; set; }
            public string? LabourCode { get; set; }
            public string? LabourDescription { get; set; }
            public string? Hsn { get; set; }
            public decimal Qty { get; set; }
            public decimal Rate { get; set; }
            public decimal IgstPercent { get; set; }
            public decimal IgstAmount { get; set; }
            public decimal TotalAmount { get; set; }
            public string? DealerCode { get; set; }
            public string? JobCardNo { get; set; }
            public DateTime? SaleDate { get; set; }
            public DateTime? FailureDate { get; set; }
            public int? Kms { get; set; }
        }

        private class InvoicePdfData
        {
            public WarrantyInvoice Header { get; set; } = null!;
            public DealerMaster? Dealer { get; set; }
            public LedgerMaster? Supplier { get; set; }
            public string? SupplierCityName { get; set; }
            public string? SupplierStateName { get; set; }
            public List<InvoicePdfLine> PartLines { get; set; } = new();
            public List<InvoicePdfLine> LabourLines { get; set; } = new();
        }

        private async Task<InvoicePdfData?> BuildInvoicePdfData(int invoiceId)
        {
            var header = await _context.WarrantyInvoices.FirstOrDefaultAsync(x => x.Id == invoiceId);
            if (header == null)
                return null;

            var dealer = !string.IsNullOrWhiteSpace(header.DealerCode)
                ? await _context.DealerMasters.FirstOrDefaultAsync(d => d.Dealercode == header.DealerCode)
                : null;

            var supplier = header.SupplierId.HasValue
                ? await _context.LedgerMasters.FirstOrDefaultAsync(l => l.Id == header.SupplierId.Value)
                : null;

            string? supplierCityName = null;
            string? supplierStateName = null;
            if (supplier != null)
            {
                if (supplier.City.HasValue)
                    supplierCityName = await _context.Cities
                        .Where(c => c.CityId == supplier.City.Value)
                        .Select(c => c.CityName)
                        .FirstOrDefaultAsync();

                if (supplier.State.HasValue)
                    supplierStateName = await _context.States
                        .Where(s => s.StateId == supplier.State.Value)
                        .Select(s => s.StateName)
                        .FirstOrDefaultAsync();
            }

            var orderIds = await _context.WarrantyInvoiceDetails
                .Where(d => d.WarrantyInvoiceHeaderId == invoiceId)
                .Select(d => d.WarrantyOrderHeaderId)
                .ToListAsync();

            var gridRows = await _context.WarrantyOrderGridDetails
                .Where(g => orderIds.Contains(g.WarrantyOrderHeaderId) && g.ItemType != null)
                .ToListAsync();

            var partLines = new List<InvoicePdfLine>();
            var labourLines = new List<InvoicePdfLine>();

            foreach (var g in gridRows)
            {
                var isLabour = g.ItemType == "Labour";

                string? modelName = null;
                var chassisDetail = !string.IsNullOrWhiteSpace(g.ChassisNo)
                    ? await _context.ChassisDetails.FirstOrDefaultAsync(c => c.ChassisNo == g.ChassisNo)
                    : null;

                // Battery chemistry ("Battery Type" on the reference layout) -
                // latest ChassisBatteryDetails row for this chassis, same
                // lookup pattern GenerateWarrantyJCClaimPdf already uses for
                // Motor/Battery/Charger numbers.
                string? batteryType = !string.IsNullOrWhiteSpace(g.ChassisNo)
                    ? await _context.ChassisBatteryDetails
                        .Where(x => x.ChassisNo == g.ChassisNo)
                        .OrderByDescending(x => x.CreatedDate)
                        .Select(x => x.BatteryChemical)
                        .FirstOrDefaultAsync()
                    : null;

                // Date of Failure - via this line's own claim -> FFIR
                // (WarrantyJcclaim.Ffirid -> Ffirheader.FailureDate).
                // WarrantyOrderGridDetail carries WarrantyJcclaimId (confirmed
                // real - already used to group claims in GetWarrantyOrderById),
                // so this resolves per-line correctly rather than assuming one
                // claim per invoice.
                DateTime? failureDate = null;
                var claimFfirId = await _context.WarrantyJcclaims
                    .Where(c => c.Id == g.WarrantyJcclaimId)
                    .Select(c => c.Ffirid)
                    .FirstOrDefaultAsync();
                if (claimFfirId.HasValue)
                {
                    failureDate = await _context.Ffirheaders
                        .Where(f => f.Id == claimFfirId.Value)
                        .Select(f => f.FailureDate)
                        .FirstOrDefaultAsync();
                }

                if (!string.IsNullOrWhiteSpace(chassisDetail?.ItemCode))
                {
                    var item = await _context.ItemMasters.FirstOrDefaultAsync(i => i.Itemcode == chassisDetail.ItemCode);
                    modelName = item?.Itemname ?? item?.Displayname;
                }

                string? hsn = null;
                if (isLabour && !string.IsNullOrWhiteSpace(g.LabourCode))
                {
                    hsn = await _context.LabourMasters
                        .Where(l => l.LabourCode == g.LabourCode)
                        .Select(l => l.Hsncode)
                        .FirstOrDefaultAsync();
                }
                else if (!isLabour && !string.IsNullOrWhiteSpace(g.PartCode))
                {
                    hsn = await _context.ItemMasters
                        .Where(i => i.Itemcode == g.PartCode)
                        .Select(i => i.Hsncode)
                        .FirstOrDefaultAsync();
                }

                var line = new InvoicePdfLine
                {
                    ClaimNoDisplay = g.ClaimNo,
                    ClaimDate = g.ClaimDate,
                    ChassisNo = g.ChassisNo,
                    ModelName = modelName,
                    BatteryType = batteryType,
                    PartCode = g.PartCode,
                    PartDescription = g.PartDescription,
                    LabourCode = g.LabourCode,
                    LabourDescription = g.LabourDescription,
                    Hsn = hsn,
                    Qty = g.Quantity ?? 0,
                    Rate = g.TotalAmount.HasValue && (g.Quantity ?? 0) > 0
        ? Math.Round((g.TotalAmount.Value - (g.IgstAmount ?? 0)) / (g.Quantity ?? 1), 2)
        : 0,
                    IgstPercent = g.IgstPercent ?? 0,
                    IgstAmount = g.IgstAmount ?? 0,
                    TotalAmount = g.TotalAmount ?? 0,
                    DealerCode = header.DealerCode,
                    JobCardNo = g.JobCardNo,
                    SaleDate = chassisDetail?.SaleDate,
                    FailureDate = failureDate,
                    Kms = (int?)g.Kms,
                };

                if (isLabour) labourLines.Add(line);
                else partLines.Add(line);
            }

            return new InvoicePdfData
            {
                Header = header,
                Dealer = dealer,
                Supplier = supplier,
                SupplierCityName = supplierCityName,
                SupplierStateName = supplierStateName,
                PartLines = partLines,
                LabourLines = labourLines
            };
        }

        // Shared header block (dealer + receiver/consignee) - identical
        // structure across the Part and Labour PDFs, per the reference layout.
        private void RenderInvoiceHeader(QuestPDF.Infrastructure.IContainer container, InvoicePdfData data, string title)
        {
            container.Column(col =>
            {
                col.Item().AlignCenter().Text(data.Dealer?.Compname ?? data.Header.DealerCode ?? "").FontSize(14).Bold();
                col.Item().AlignCenter().Text(string.Join(", ", new[] {
                    data.Dealer?.Adress1, data.Dealer?.Adress2, data.Dealer?.City, data.Dealer?.State, data.Dealer?.Pin
                }.Where(s => !string.IsNullOrWhiteSpace(s)))).FontSize(8);
                col.Item().AlignCenter().Text($"Phone No. {data.Dealer?.PhoneOff} {data.Dealer?.Mobile}").FontSize(8);
                col.Item().AlignCenter().Text($"GSTIN No. :{data.Dealer?.CompgstinNo}").FontSize(8);
                col.Item().AlignCenter().Text($"PAN NO:- {data.Dealer?.Pan}").FontSize(8);

                col.Item().PaddingTop(6).AlignCenter().Text(title).FontSize(12).Bold();

                col.Item().PaddingTop(6).Row(row =>
                {
                    row.RelativeItem().Text($"Invoice No : {data.Header.InvoicePrefix}{data.Header.InvoiceNo}");
                    row.RelativeItem().AlignRight().Text($"Invoice Date : {data.Header.InvoiceDate:dd-MM-yyyy}");
                });
                col.Item().Row(row =>
                {
                    row.RelativeItem().Text($"Batch No : {data.Header.BatchNo}");
                    row.RelativeItem().AlignRight().Text($"Batch Date : {data.Header.BatchDate:dd-MM-yyyy}");
                });

            
                col.Item().PaddingTop(6).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Details of Receiver (Billed to)").Bold().Underline();
                        c.Item().Text($"Name : {data.Supplier?.LedgerName}");
                        c.Item().Text($"Address : {data.Supplier?.Address}");
                        c.Item().Text($"State : {data.SupplierStateName}");
                        c.Item().Text($"City : {data.SupplierCityName}");
                        c.Item().Text($"GSTIN : {data.Supplier?.Gstno}");
                    });
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Consignee Address").Bold().Underline();
                        c.Item().Text($"Name : {data.Supplier?.LedgerName}");
                        c.Item().Text($"Address : {data.Supplier?.Address}");
                        c.Item().Text($"State : {data.SupplierStateName}");
                        c.Item().Text($"City : {data.SupplierCityName}");
                        c.Item().Text($"GSTIN : {data.Supplier?.Gstno}");
                    });
                });
            });
        }

        private byte[] RenderLineItemInvoicePdf(InvoicePdfData data, List<InvoicePdfLine> lines, string title, bool isLabour)
        {
            decimal totalQty = lines.Sum(l => l.Qty);
            decimal totalRate = lines.Sum(l => l.Rate);
            decimal totalIgst = lines.Sum(l => l.IgstAmount);
            decimal totalAmount = lines.Sum(l => l.TotalAmount);
            decimal roundedAmount = Math.Round(totalAmount, MidpointRounding.AwayFromZero);
            decimal roundOff = roundedAmount - totalAmount;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(8));

                    page.Content().Column(col =>
                    {
                        col.Item().Element(c => RenderInvoiceHeader(c, data, "Warranty Invoice"));

                        // FIX: restructured from 11 columns to 10, matching the
                        // reference layout - Qty and Rate are merged into a
                        // single two-line cell, and Model Name now includes
                        // Battery Type on a second line beneath it.
                        col.Item().PaddingTop(8).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(25);    // Sr.No
                                columns.RelativeColumn(1.3f);  // Claim No/Date
                                columns.RelativeColumn(1.2f);  // Chassis No
                                columns.RelativeColumn(1.1f);  // Model Name / Battery Type
                                columns.RelativeColumn(2.0f);  // Part/Labour Number Description
                                columns.RelativeColumn(0.9f);  // Inward/Outward Serial
                                columns.RelativeColumn(0.8f);  // HSN/SAC
                                columns.RelativeColumn(0.9f);  // Qty / Rate (merged)
                                columns.RelativeColumn(0.9f);  // IGST
                                columns.RelativeColumn(0.9f);  // Amount
                            });

                            void HeaderCell(string text) => table.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(3).Text(text).Bold();

                            HeaderCell("Sr.No");
                            HeaderCell("Claim No\nClaim Date");
                            HeaderCell("Chassis No");
                            HeaderCell("Model Name\nBattery Type");
                            HeaderCell(isLabour ? "Labour Code\nDescription" : "Part Number\nDescription");
                            HeaderCell("Inward Serial\nOutward Serial");
                            HeaderCell("HSN/SAC Code");
                            HeaderCell("Qty\nRate");
                            HeaderCell("IGST");
                            HeaderCell("Amount");

                            int srNo = 1;
                            foreach (var line in lines)
                            {
                                table.Cell().Border(1).Padding(3).Text(srNo++.ToString());
                                table.Cell().Border(1).Padding(3).Text($"{line.ClaimNoDisplay}\n{line.ClaimDate:dd-MM-yyyy}");
                                table.Cell().Border(1).Padding(3).Text(line.ChassisNo ?? "");
                                table.Cell().Border(1).Padding(3).Text(
                                    string.IsNullOrWhiteSpace(line.BatteryType) ? (line.ModelName ?? "") : $"{line.ModelName}\n{line.BatteryType}");
                                table.Cell().Border(1).Padding(3).Text(
                                    isLabour ? $"{line.LabourCode}\n{line.LabourDescription}" : $"{line.PartCode}\n{line.PartDescription}");
                                // Inward/Outward Serial - GENUINELY UNCONFIRMED, left blank.
                                table.Cell().Border(1).Padding(3).Text("");
                                table.Cell().Border(1).Padding(3).Text(line.Hsn ?? "");
                                table.Cell().Border(1).AlignRight().Padding(3).Text($"{line.Qty:0.##}\n{line.Rate:0.00}");
                                table.Cell().Border(1).AlignRight().Padding(3).Text($"({line.IgstPercent:0.##})\n{line.IgstAmount:0.00}");
                                table.Cell().Border(1).AlignRight().Padding(3).Text(line.TotalAmount.ToString("0.00"));
                            }

                            table.Cell().ColumnSpan(7).Border(1).Padding(3).AlignRight().Text("TOTAL").Bold();
                            table.Cell().Border(1).AlignRight().Padding(3).Text($"{totalQty:0.##}\n{totalRate:0.00}").Bold();
                            table.Cell().Border(1).AlignRight().Padding(3).Text(totalIgst.ToString("0.00")).Bold();
                            table.Cell().Border(1).AlignRight().Padding(3).Text(totalAmount.ToString("0.00")).Bold();

                            table.Cell().ColumnSpan(9).Border(1).Padding(3).AlignRight().Text("Round Off");
                            table.Cell().Border(1).AlignRight().Padding(3).Text(roundOff.ToString("0.00"));

                            table.Cell().ColumnSpan(9).Border(1).Padding(3).AlignRight().Text("Net Amount").Bold();
                            table.Cell().Border(1).AlignRight().Padding(3).Text(roundedAmount.ToString("0.00")).Bold();
                        });

                        // ---- Tax Summary HSN Wise ----
                        // Groups by HSN code. CGST/SGST always 0 - the
                        // confirmed data model only ever populates IGST for
                        // warranty claims (InsertWarrantyJCClaim only ever
                        // maps IgstAmount, never CGST/SGST separately).
                        col.Item().PaddingTop(8).Text("Tax Summary HSN Wise").Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(25);   // Sr.No
                                columns.RelativeColumn(1.2f); // HSN Code
                                columns.RelativeColumn(1.2f); // Taxable Value
                                columns.RelativeColumn(1.0f); // SGST Rate
                                columns.RelativeColumn(1.0f); // SGST Amount
                                columns.RelativeColumn(1.0f); // CGST Rate
                                columns.RelativeColumn(1.0f); // CGST Amount
                                columns.RelativeColumn(1.0f); // IGST Rate
                                columns.RelativeColumn(1.0f); // IGST Amount
                                columns.RelativeColumn(1.2f); // Total Tax Amount
                            });

                            void HHeader(string text) => table.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(3).Text(text).Bold();

                            HHeader("Sr.No"); HHeader("HSN Code"); HHeader("Taxable Value");
                            HHeader("SGST Rate%"); HHeader("SGST Amount");
                            HHeader("CGST Rate%"); HHeader("CGST Amount");
                            HHeader("IGST Rate%"); HHeader("IGST Amount");
                            HHeader("Total Tax Amount");

                            var hsnGroups = lines
                                .GroupBy(l => l.Hsn ?? "")
                                .Select(g => new
                                {
                                    Hsn = g.Key,
                                    TaxableValue = g.Sum(l => l.Rate * l.Qty),
                                    IgstPercent = g.First().IgstPercent,
                                    IgstAmount = g.Sum(l => l.IgstAmount)
                                })
                                .ToList();

                            int hsnSrNo = 1;
                            decimal hsnTaxableTotal = 0, hsnIgstTotal = 0;

                            foreach (var g in hsnGroups)
                            {
                                hsnTaxableTotal += g.TaxableValue;
                                hsnIgstTotal += g.IgstAmount;

                                table.Cell().Border(1).Padding(3).Text(hsnSrNo++.ToString());
                                table.Cell().Border(1).Padding(3).Text(g.Hsn);
                                table.Cell().Border(1).AlignRight().Padding(3).Text(g.TaxableValue.ToString("0.00"));
                                table.Cell().Border(1).AlignRight().Padding(3).Text("0%");
                                table.Cell().Border(1).AlignRight().Padding(3).Text("0");
                                table.Cell().Border(1).AlignRight().Padding(3).Text("0%");
                                table.Cell().Border(1).AlignRight().Padding(3).Text("0");
                                table.Cell().Border(1).AlignRight().Padding(3).Text($"{g.IgstPercent:0.##}%");
                                table.Cell().Border(1).AlignRight().Padding(3).Text(g.IgstAmount.ToString("0.00"));
                                table.Cell().Border(1).AlignRight().Padding(3).Text(g.IgstAmount.ToString("0.00"));
                            }

                            table.Cell().ColumnSpan(2).Border(1).Padding(3).Text("Total:").Bold();
                            table.Cell().Border(1).AlignRight().Padding(3).Text(hsnTaxableTotal.ToString("0.000")).Bold();
                            table.Cell().ColumnSpan(2).Border(1).AlignRight().Padding(3).Text("0.000").Bold();
                            table.Cell().ColumnSpan(2).Border(1).AlignRight().Padding(3).Text("0.000").Bold();
                            table.Cell().ColumnSpan(2).Border(1).AlignRight().Padding(3).Text(hsnIgstTotal.ToString("0.000")).Bold();
                            table.Cell().Border(1).AlignRight().Padding(3).Text(hsnIgstTotal.ToString("0.000")).Bold();
                        });
                        col.Item().PaddingTop(8).Text("Tax Summary GST Wise").Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(25);   // Sr.No
                                columns.RelativeColumn(1.2f); // Taxable Value
                                columns.RelativeColumn(1.0f); // SGST Rate
                                columns.RelativeColumn(1.0f); // SGST Amount
                                columns.RelativeColumn(1.0f); // CGST Rate
                                columns.RelativeColumn(1.0f); // CGST Amount
                                columns.RelativeColumn(1.0f); // IGST Rate
                                columns.RelativeColumn(1.0f); // IGST Amount
                                columns.RelativeColumn(1.2f); // Total Tax Amount
                            });

                            void GHeader(string text) => table.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(3).Text(text).Bold();

                            GHeader("Sr.No"); GHeader("Taxable Value");
                            GHeader("SGST Rate%"); GHeader("SGST Amount");
                            GHeader("CGST Rate%"); GHeader("CGST Amount");
                            GHeader("IGST Rate%"); GHeader("IGST Amount");
                            GHeader("Total Tax Amount");

                            var gstGroups = lines
                                .GroupBy(l => l.IgstPercent)
                                .Select(g => new
                                {
                                    IgstPercent = g.Key,
                                    TaxableValue = g.Sum(l => l.Rate * l.Qty),
                                    IgstAmount = g.Sum(l => l.IgstAmount)
                                })
                                .ToList();

                            int gstSrNo = 1;
                            decimal gstTaxableTotal = 0, gstIgstTotal = 0;

                            foreach (var g in gstGroups)
                            {
                                gstTaxableTotal += g.TaxableValue;
                                gstIgstTotal += g.IgstAmount;

                                table.Cell().Border(1).Padding(3).Text(gstSrNo++.ToString());
                                table.Cell().Border(1).AlignRight().Padding(3).Text(g.TaxableValue.ToString("0.00"));
                                table.Cell().Border(1).AlignRight().Padding(3).Text("0%");
                                table.Cell().Border(1).AlignRight().Padding(3).Text("0");
                                table.Cell().Border(1).AlignRight().Padding(3).Text("0%");
                                table.Cell().Border(1).AlignRight().Padding(3).Text("0");
                                table.Cell().Border(1).AlignRight().Padding(3).Text($"{g.IgstPercent:0.##}%");
                                table.Cell().Border(1).AlignRight().Padding(3).Text(g.IgstAmount.ToString("0.00"));
                                table.Cell().Border(1).AlignRight().Padding(3).Text(g.IgstAmount.ToString("0.00"));
                            }

                            table.Cell().Border(1).Padding(3).Text("Total:").Bold();
                            table.Cell().Border(1).AlignRight().Padding(3).Text(gstTaxableTotal.ToString("0.000")).Bold();
                            table.Cell().ColumnSpan(2).Border(1).AlignRight().Padding(3).Text("0.000").Bold();
                            table.Cell().ColumnSpan(2).Border(1).AlignRight().Padding(3).Text("0.000").Bold();
                            table.Cell().ColumnSpan(2).Border(1).AlignRight().Padding(3).Text(gstIgstTotal.ToString("0.000")).Bold();
                            table.Cell().Border(1).AlignRight().Padding(3).Text(gstIgstTotal.ToString("0.000")).Bold();
                        });

                        col.Item().PaddingTop(6).Text($"Amount in words: {NumberToWordsHelper.Convert(roundedAmount)}");

                        col.Item().PaddingTop(10).AlignRight().Text($"For : {data.Dealer?.Compname}").Bold();
                        col.Item().PaddingTop(4).Text("This is system generated invoice signature and stamp is not required").FontSize(7).Italic();
                    });
                });
            });

            return document.GeneratePdf();
        }

        public async Task<byte[]> GenerateWarrantyInvoicePartPdf(int invoiceId)
        {
            var data = await BuildInvoicePdfData(invoiceId);
            if (data == null)
                throw new InvalidOperationException($"Warranty Invoice with Id {invoiceId} not found.");

            return RenderLineItemInvoicePdf(data, data.PartLines, "Warranty Invoice (Parts)", isLabour: false);
        }

        public async Task<byte[]> GenerateWarrantyInvoiceLabourPdf(int invoiceId)
        {
            var data = await BuildInvoicePdfData(invoiceId);
            if (data == null)
                throw new InvalidOperationException($"Warranty Invoice with Id {invoiceId} not found.");
            return RenderLineItemInvoicePdf(data, data.LabourLines, "Warranty Invoice (Labour)", isLabour: true);
        }
        public async Task<byte[]> GenerateWarrantyClaimTagPdf(int invoiceId)
        {
            var data = await BuildInvoicePdfData(invoiceId);
            if (data == null)
                throw new InvalidOperationException($"Warranty Invoice with Id {invoiceId} not found.");

            var document = Document.Create(container =>
            {

                var tagLines = data.PartLines.Count > 0
                    ? data.PartLines
                    : new List<InvoicePdfLine> { new InvoicePdfLine { DealerCode = data.Header.DealerCode } };

                foreach (var line in tagLines)
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A5);
                        page.Margin(20);
                        page.DefaultTextStyle(x => x.FontSize(9));

                        page.Content().Column(col =>
                        {
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Text("BGAUSS").FontSize(14).Bold();
                                row.RelativeItem().AlignRight().Text("WARRANTY CLAIM TAG").FontSize(12).Bold();
                            });

                            col.Item().PaddingTop(6).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1.3f);
                                });

                                void Row(string label1, string value1, string? label2 = null, string? value2 = null)
                                {
                                    table.Cell().Border(1).Padding(4).Text(label1).Bold();
                                    table.Cell().Border(1).Padding(4).Text(value1 ?? "");
                                    if (label2 != null)
                                        table.Cell().Border(1).Padding(4).Text($"{label2}: {value2}");
                                    else
                                        table.Cell().Border(1).Padding(4).Text("");
                                }

                                // FIX: Date of Sale, Date of Failure, and Failure
                                // Kms now use the values resolved onto the line
                                // in BuildInvoicePdfData, instead of staying
                                // permanently blank.
                                Row("WTY.CLAIM NO.", line.ClaimNoDisplay ?? "", "MODEL", line.ModelName ?? "");
                                Row("JOB CARD NO.", line.JobCardNo ?? "", "DATE OF FAILURE", line.FailureDate?.ToString("dd-MM-yyyy") ?? "");
                                Row("DATE OF SALE", line.SaleDate?.ToString("dd-MM-yyyy") ?? "", "FAILURE Kms", line.Kms?.ToString() ?? "");
                                Row("DATE OF REPAIR", line.ClaimDate?.ToString("dd-MM-yyyy") ?? "");
                                Row("VIN", line.ChassisNo ?? "");
                                Row("PART NO.", line.PartCode ?? "");
                                Row("DESCRIPTION", line.PartDescription ?? "");
                                Row("Inward Serial", ""); // GENUINELY UNCONFIRMED
                                Row("Outward Serial", ""); // GENUINELY UNCONFIRMED
                                Row("DEFECT", ""); // GENUINELY UNCONFIRMED
                                Row("DEALER CODE", line.DealerCode ?? "");
                                Row("DEALER NAME & LOCATION", data.Dealer?.Compname ?? "");
                            });
                        });
                    });
                }
            });

            return document.GeneratePdf();
        }



        public async Task<List<string>> SearchInvoiceBatchNos(string dealerCode, string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return new List<string>();

            return await _context.WarrantyInvoices
                .Where(x => x.DealerCode == dealerCode && x.BatchNo.Contains(searchText))
                .Select(x => x.BatchNo)
                .Distinct()
                .OrderByDescending(b => b)
                .Take(20)
                .ToListAsync();
        }

        public async Task<List<string>> SearchInvoiceNos(string dealerCode, string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return new List<string>();
            return await _context.WarrantyInvoices
                .Where(x => x.DealerCode == dealerCode && (x.InvoicePrefix + x.InvoiceNo).Contains(searchText))
                .Select(x => x.InvoicePrefix + x.InvoiceNo)
                .Distinct()
                .OrderByDescending(o => o)
                .Take(20)
                .ToListAsync();
        }

        public async Task<List<LocationDropdownItemViewModel>> GetDistinctInvoiceLocations(string dealerCode)
        {
            var distinctCodes = await (
                from g in _context.WarrantyInvoiceGridDetails
                join h in _context.WarrantyInvoices on g.WarrantyInvoiceHeaderId equals h.Id
                where h.DealerCode == dealerCode && h.IsActive && !string.IsNullOrWhiteSpace(g.Location)
                select g.Location
            ).Distinct().ToListAsync();

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
        public async Task<List<string>> SearchClaimInvoiceNos(string dealerCode, string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return new List<string>();

            return await (
                from d in _context.WarrantyInvoiceDetails
                join h in _context.WarrantyInvoices on d.WarrantyInvoiceHeaderId equals h.Id
                join g in _context.WarrantyOrderGridDetails on d.WarrantyOrderHeaderId equals g.WarrantyOrderHeaderId
                where h.DealerCode == dealerCode && g.InvoiceNo != null && g.InvoiceNo.Contains(searchText)
                select g.InvoiceNo
            )
            .Distinct()
            .OrderByDescending(c => c)
            .Take(20)
            .ToListAsync();
        }

        // Builds the per-line payload to submit to the ERP for a given Warranty
        // Invoice. Field mapping follows the ONLY confirmed ERP contract
        // available (the GET /erpreport/wcjo report shape) - see
        // ErpWarrantyClaimLineViewModel for which fields are solid vs
        // genuinely unconfirmed/blank.
        public async Task<List<ErpWarrantyClaimLineViewModel>> BuildErpWarrantyClaimPayload(int invoiceId)
        {
            var header = await _context.WarrantyInvoices.FirstOrDefaultAsync(x => x.Id == invoiceId);
            if (header == null)
                throw new InvalidOperationException($"Warranty Invoice with Id {invoiceId} not found.");

            var dealer = !string.IsNullOrWhiteSpace(header.DealerCode)
                ? await _context.DealerMasters.FirstOrDefaultAsync(d => d.Dealercode == header.DealerCode)
                : null;

            var orderIds = await _context.WarrantyInvoiceDetails
                .Where(d => d.WarrantyInvoiceHeaderId == invoiceId)
                .Select(d => d.WarrantyOrderHeaderId)
                .ToListAsync();

            var gridRows = await _context.WarrantyOrderGridDetails
                .Where(g => orderIds.Contains(g.WarrantyOrderHeaderId) && g.ItemType != null)
                .ToListAsync();

            var lines = new List<ErpWarrantyClaimLineViewModel>();
            int srNo = 1;

            foreach (var g in gridRows)
            {
                var isLabour = g.ItemType == "Labour";

                var chassisDetail = !string.IsNullOrWhiteSpace(g.ChassisNo)
                    ? await _context.ChassisDetails.FirstOrDefaultAsync(c => c.ChassisNo == g.ChassisNo)
                    : null;

                string? modelName = null;
                if (!string.IsNullOrWhiteSpace(chassisDetail?.ItemCode))
                {
                    var item = await _context.ItemMasters.FirstOrDefaultAsync(i => i.Itemcode == chassisDetail.ItemCode);
                    modelName = item?.Itemname ?? item?.Displayname;
                }

                // Same claim -> FFIR resolution already used for the PDF's Failure Date.
                DateTime? failureDate = null;
                var claimFfirId = await _context.WarrantyJcclaims
                    .Where(c => c.Id == g.WarrantyJcclaimId)
                    .Select(c => c.Ffirid)
                    .FirstOrDefaultAsync();
                if (claimFfirId.HasValue)
                {
                    failureDate = await _context.Ffirheaders
                        .Where(f => f.Id == claimFfirId.Value)
                        .Select(f => f.FailureDate)
                        .FirstOrDefaultAsync();
                }

                decimal totalTax = (g.CgstAmount ?? 0) + (g.SgstAmount ?? 0) + (g.IgstAmount ?? 0);
                decimal qty = g.Quantity ?? 0;
                decimal rate = qty > 0 ? Math.Round(((g.TotalAmount ?? 0) - totalTax) / qty, 2) : 0;

                lines.Add(new ErpWarrantyClaimLineViewModel
                {
                    SlNo = (srNo++).ToString(),
                    DealerName = dealer?.Compname,
                    DealerCode = header.DealerCode,
                    Location = g.LocationName,
                    LocationCity = null, // UNCONFIRMED - see ViewModel comment
                    JobNo = g.JobCardNo,
                    JobDate = g.JobCardDate?.ToString("dd-MM-yyyy"),
                    ClaimNo = g.ClaimNo,
                    ClaimDate = g.ClaimDate?.ToString("dd-MM-yyyy"),
                    Kms = g.Kms?.ToString(),
                    VehicleSaleDate = chassisDetail?.SaleDate?.ToString("dd-MM-yyyy"),
                    PartFailureDate = failureDate?.ToString("dd-MM-yyyy"),
                    ServiceType = g.ServiceHead, // best-effort stand-in, see ViewModel comment
                    ChassisNo = g.ChassisNo,
                    ModelName = modelName,
                    Variants = null, // UNCONFIRMED
                    PartCode = isLabour ? g.LabourCode : g.PartCode,
                    PartName = isLabour ? g.LabourDescription : g.PartName,
                    Qty = qty.ToString("0.##"),
                    Rate = rate.ToString("0.00"),
                    CgstPercent = (g.CgstPercent ?? 0).ToString("0.##"),
                    CgstAmount = (g.CgstAmount ?? 0).ToString("0.00"),
                    SgstPercent = (g.SgstPercent ?? 0).ToString("0.##"),
                    SgstAmount = (g.SgstAmount ?? 0).ToString("0.00"),
                    IgstPercent = (g.IgstPercent ?? 0).ToString("0.##"),
                    IgstAmount = (g.IgstAmount ?? 0).ToString("0.00"),
                    Amount = (g.TotalAmount ?? 0).ToString("0.00"),
                    DealerObservation = null, // UNCONFIRMED - see ViewModel comment
                    Rca = null,               // UNCONFIRMED
                    InvoiceRefNo = null,      // UNCONFIRMED
                    InvoiceNo = g.InvoiceNo,
                    InvoiceDate = g.InvoiceDate?.ToString("dd-MM-yyyy"),
                    DocNo = $"{header.InvoicePrefix}{header.InvoiceNo}", // ASSUMPTION - see ViewModel comment
                    DocDate = header.InvoiceDate?.ToString("dd-MM-yyyy"),
                    VendorPoNo = null,   // UNCONFIRMED
                    VendorPoDate = null, // UNCONFIRMED
                    Total = null         // UNCONFIRMED
                });
            }

            return lines;
        }
    }
}