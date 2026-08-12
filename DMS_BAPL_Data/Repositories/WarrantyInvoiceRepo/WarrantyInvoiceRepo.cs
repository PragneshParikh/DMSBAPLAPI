using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;

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

            if (filter.DateFrom.HasValue)
                query = query.Where(x => x.InvoiceDate >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(x => x.InvoiceDate <= filter.DateTo.Value);

            if (!string.IsNullOrWhiteSpace(filter.BatchNo))
                query = query.Where(x => x.BatchNo == filter.BatchNo);

            if (!string.IsNullOrWhiteSpace(filter.InvoiceNo))
                query = query.Where(x => x.InvoiceNo == filter.InvoiceNo);

            if (!string.IsNullOrWhiteSpace(filter.ClaimType))
                query = query.Where(x => x.ClaimType == filter.ClaimType);

            if (filter.SupplierId.HasValue)
                query = query.Where(x => x.SupplierId == filter.SupplierId.Value);

            if (!string.IsNullOrWhiteSpace(filter.Location))
                query = query.Where(x => _context.WarrantyInvoiceGridDetails
                    .Any(g => g.WarrantyInvoiceHeaderId == x.Id && g.Location == filter.Location));

            if (filter.IsApproved.HasValue)
                query = query.Where(x => x.IsApproved == filter.IsApproved.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.InvoiceDate)
                .ThenByDescending(x => x.Id) // deterministic tie-breaker - see the same fix applied to SearchWarrantyOrders
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

            // ClaimSubType is derived from each linked line's confirmed
            // ItemType, per invoice - "Warranty -Labour" if every line is
            // Labour, "Warranty -Parts" if every line is Part,
            // "Warranty -Mixed" if both appear. Computed as a
            // post-processing step since expressing this inline within the
            // Select above would need a considerably more complex query.
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

                // First linked order's location - same "first order"
                // convention already used elsewhere for repairBillNo.
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

            var lastBatchSeq = await _context.WarrantyInvoices
                .Where(x => x.DealerCode == dealerCode)
                .CountAsync() + 1;

            var lastInvoiceSeq = lastBatchSeq;

            var batchNo = $"{lastBatchSeq}/BT/{financialYearSuffix}";

            // Fixed format, starting from "WR/26-27/" (year suffix moves
            // with the actual financial year) - auto-generated directly
            // here rather than via PrefixService, since that required an
            // unconfirmed dealer-config entry that may not exist.
            var invoicePrefix = $"WR/{financialYearSuffix}/";

            var invoiceNo = $"{lastInvoiceSeq}";

            return (batchNo, invoicePrefix, invoiceNo);
        }

        private static string GetFinancialYearSuffix(DateTime date)
        {
            int startYear = date.Month >= 4 ? date.Year : date.Year - 1;
            int endYear = startYear + 1;
            return $"{startYear % 100:D2}-{endYear % 100:D2}";
        }
    }
}