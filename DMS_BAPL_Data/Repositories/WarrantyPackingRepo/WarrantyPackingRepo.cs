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

namespace DMS_BAPL_Data.Repositories.WarrantyPackingRepo
{
    public class WarrantyPackingRepo : IWarrantyPackingRepo
    {
        private readonly BapldmsvadContext _context;
        public WarrantyPackingRepo(BapldmsvadContext context)
        {
            _context = context;
        }



        public async Task<List<PackingSlipLineViewModel>> GetPackableLines(int warrantyInvoiceHeaderId)
        {
            var orderIds = await _context.WarrantyInvoiceDetails
                .Where(d => d.WarrantyInvoiceHeaderId == warrantyInvoiceHeaderId)
                .Select(d => d.WarrantyOrderHeaderId)
                .ToListAsync();

            var lines = await _context.WarrantyOrderGridDetails
                .Where(g => orderIds.Contains(g.WarrantyOrderHeaderId) &&
                            (g.ItemType == "Part" || g.ItemType == "Labour"))
                .ToListAsync();

            var lineIds = lines.Select(l => l.Id).ToList();

            // Only counts quantity packed by slips that are still active -
            // a deleted (soft-removed) slip must free up its quantity for
            // repacking, not lock it forever.
            var packedByLineId = await _context.WarrantyPackingSlipDetails
                .Where(d => lineIds.Contains(d.WarrantyOrderGridDetailId) &&
                            d.WarrantyPackingSlipBox.WarrantyPackingSlipHeader.IsActive)
                .GroupBy(d => d.WarrantyOrderGridDetailId)
                .Select(g => new { LineId = g.Key, PackedQty = g.Sum(x => x.Qty) })
                .ToListAsync();

            var packedMap = packedByLineId.ToDictionary(x => x.LineId, x => x.PackedQty);

            return lines
                .Select(l =>
                {
                    var invoicedQty = l.Quantity ?? 0;
                    var packedQty = packedMap.TryGetValue(l.Id, out var p) ? p : 0;
                    var isLabour = l.ItemType == "Labour";

                    return new PackingSlipLineViewModel
                    {
                        WarrantyOrderGridDetailId = l.Id,
                        ClaimNo = l.ClaimNo,
                        ItemType = l.ItemType,
                        PartCode = isLabour ? l.LabourCode : l.PartCode,
                        PartDescription = isLabour ? l.LabourDescription : l.PartDescription,
                        InvoicedQty = invoicedQty,
                        AlreadyPackedQty = packedQty,
                        RemainingQty = invoicedQty - packedQty
                    };
                })
                .Where(x => x.RemainingQty > 0)
                .ToList();
        }

        public async Task<int> InsertWarrantyPackingSlip(WarrantyPackingSlipViewModel model, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var header = new WarrantyPackingSlip
                {
                    DealerCode = model.DealerCode,
                    WarrantyInvoiceHeaderId = model.WarrantyInvoiceHeaderId,
                    SlipPrefix = model.SlipPrefix,
                    SlipNo = model.SlipNo,
                    SlipDate = model.SlipDate,
                    IsActive = true,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now
                };

                _context.WarrantyPackingSlips.Add(header);
                await _context.SaveChangesAsync();

                foreach (var boxModel in model.Boxes)
                {
                    var box = new WarrantyPackingSlipBox
                    {
                        WarrantyPackingSlipHeaderId = header.Id,
                        BoxNumber = boxModel.BoxNumber,
                        BoxType = boxModel.BoxType,
                        Length = boxModel.Length,
                        Width = boxModel.Width,
                        Height = boxModel.Height,
                        Weight = boxModel.Weight,
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now
                    };

                    _context.WarrantyPackingSlipBoxes.Add(box);
                    await _context.SaveChangesAsync();

                    foreach (var detailModel in boxModel.Details)
                    {
                        _context.WarrantyPackingSlipDetails.Add(new WarrantyPackingSlipDetail
                        {
                            WarrantyPackingSlipBoxId = box.Id,
                            WarrantyOrderGridDetailId = detailModel.WarrantyOrderGridDetailId,
                            PrnNo = detailModel.PrnNo,
                            Qty = detailModel.Qty,
                            CreatedBy = userId,
                            CreatedDate = DateTime.Now
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return header.Id;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<WarrantyPackingSlipSearchResultViewModel> SearchWarrantyPackingSlips(WarrantyPackingSlipSearchViewModel filter)
        {
            var query = _context.WarrantyPackingSlips
                .Include(x => x.WarrantyInvoiceHeader)
                .Include(x => x.WarrantyPackingSlipBoxes)
                    .ThenInclude(b => b.WarrantyPackingSlipDetails)
                .AsQueryable();

            if (!filter.IncludeInactive)
                query = query.Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(filter.DealerCode))
                query = query.Where(x => x.DealerCode == filter.DealerCode);

            if (filter.DateFrom.HasValue)
                query = query.Where(x => x.SlipDate >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(x => x.SlipDate <= filter.DateTo.Value);

            if (!string.IsNullOrWhiteSpace(filter.SlipNo))
                query = query.Where(x => (x.SlipPrefix + x.SlipNo) == filter.SlipNo);

            if (!string.IsNullOrWhiteSpace(filter.InvoiceNo))
                query = query.Where(x => (x.WarrantyInvoiceHeader.InvoicePrefix + x.WarrantyInvoiceHeader.InvoiceNo) == filter.InvoiceNo);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.SlipDate)
                .ThenByDescending(x => x.Id)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(x => new WarrantyPackingSlipListViewModel
                {
                    Id = x.Id,
                    SlipPrefix = x.SlipPrefix,
                    SlipNo = x.SlipNo,
                    SlipDate = x.SlipDate,
                    WarrantyInvoiceHeaderId = x.WarrantyInvoiceHeaderId,
                    InvoicePrefix = x.WarrantyInvoiceHeader.InvoicePrefix,
                    InvoiceNo = x.WarrantyInvoiceHeader.InvoiceNo,
                    InvoiceDate = x.WarrantyInvoiceHeader.InvoiceDate,
                    TotalBoxes = x.WarrantyPackingSlipBoxes.Count,
                    TotalQty = x.WarrantyPackingSlipBoxes
                        .SelectMany(b => b.WarrantyPackingSlipDetails)
                        .Sum(d => (decimal?)d.Qty) ?? 0,
                    IsActive = x.IsActive
                })
                .ToListAsync();

            return new WarrantyPackingSlipSearchResultViewModel
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
            };
        }

        public async Task<WarrantyPackingSlipDetailsViewModel?> GetWarrantyPackingSlipById(int id)
        {
            var header = await _context.WarrantyPackingSlips
                .Include(x => x.WarrantyInvoiceHeader)
                .Include(x => x.WarrantyPackingSlipBoxes)
                    .ThenInclude(b => b.WarrantyPackingSlipDetails)
                        .ThenInclude(d => d.WarrantyOrderGridDetail)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (header == null)
                return null;

            return new WarrantyPackingSlipDetailsViewModel
            {
                Id = header.Id,
                DealerCode = header.DealerCode,
                WarrantyInvoiceHeaderId = header.WarrantyInvoiceHeaderId,
                InvoicePrefix = header.WarrantyInvoiceHeader?.InvoicePrefix,
                InvoiceNo = header.WarrantyInvoiceHeader?.InvoiceNo,
                InvoiceDate = header.WarrantyInvoiceHeader?.InvoiceDate,
                SlipPrefix = header.SlipPrefix,
                SlipNo = header.SlipNo,
                SlipDate = header.SlipDate,
                Boxes = header.WarrantyPackingSlipBoxes.Select(b => new WarrantyPackingSlipBoxDetailsViewModel
                {
                    BoxNumber = b.BoxNumber,
                    BoxType = b.BoxType,
                    Length = b.Length,
                    Lines = b.WarrantyPackingSlipDetails.Select(d =>
                    {
                        var isLabour = d.WarrantyOrderGridDetail?.ItemType == "Labour";
                        return new WarrantyPackingSlipLineDetailsViewModel
                        {
                            WarrantyOrderGridDetailId = d.WarrantyOrderGridDetailId,
                            ItemType = d.WarrantyOrderGridDetail?.ItemType,
                            ClaimNo = d.WarrantyOrderGridDetail?.ClaimNo,
                            PartCode = isLabour ? d.WarrantyOrderGridDetail?.LabourCode : d.WarrantyOrderGridDetail?.PartCode,
                            PartDescription = isLabour ? d.WarrantyOrderGridDetail?.LabourDescription : d.WarrantyOrderGridDetail?.PartDescription,
                            Qty = d.Qty
                        };
                    }).ToList()
                }).ToList()
            };
        }

        public async Task<bool> DeleteWarrantyPackingSlip(int id, string userId)
        {
            var header = await _context.WarrantyPackingSlips
                .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

            if (header == null)
                return false;

            header.IsActive = false;
            header.UpdatedBy = userId;
            header.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<WarrantyPackingSlipLineSearchResultViewModel> SearchWarrantyPackingSlipLines(WarrantyPackingSlipLineSearchViewModel filter)
        {
            var query =
                from d in _context.WarrantyPackingSlipDetails
                join b in _context.WarrantyPackingSlipBoxes on d.WarrantyPackingSlipBoxId equals b.Id
                join h in _context.WarrantyPackingSlips on b.WarrantyPackingSlipHeaderId equals h.Id
                join inv in _context.WarrantyInvoices on h.WarrantyInvoiceHeaderId equals inv.Id
                join g in _context.WarrantyOrderGridDetails on d.WarrantyOrderGridDetailId equals g.Id
                select new { d, b, h, inv, g };

            if (!filter.IncludeInactive)
                query = query.Where(x => x.h.IsActive);

            if (!string.IsNullOrWhiteSpace(filter.DealerCode))
                query = query.Where(x => x.h.DealerCode == filter.DealerCode);

            if (filter.DateFrom.HasValue)
                query = query.Where(x => x.h.SlipDate >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(x => x.h.SlipDate <= filter.DateTo.Value);

            if (filter.InvoiceDateFrom.HasValue)
                query = query.Where(x => x.inv.InvoiceDate >= filter.InvoiceDateFrom.Value);

            if (filter.InvoiceDateTo.HasValue)
                query = query.Where(x => x.inv.InvoiceDate <= filter.InvoiceDateTo.Value);

            if (!string.IsNullOrWhiteSpace(filter.InvoiceNo))
                query = query.Where(x => (x.inv.InvoicePrefix + x.inv.InvoiceNo) == filter.InvoiceNo);

            if (!string.IsNullOrWhiteSpace(filter.SlipNo))
                query = query.Where(x => (x.h.SlipPrefix + x.h.SlipNo) == filter.SlipNo);

            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                var text = filter.SearchText;
                query = query.Where(x =>
                    (x.g.ClaimNo != null && x.g.ClaimNo.Contains(text)) ||
                    (x.g.PartCode != null && x.g.PartCode.Contains(text)) ||
                    (x.g.LabourCode != null && x.g.LabourCode.Contains(text)) ||
                    (x.b.BoxNumber != null && x.b.BoxNumber.Contains(text)));
            }

            var totalCount = await query.CountAsync();

            var rows = await query
                .OrderByDescending(x => x.h.SlipDate)
                .ThenByDescending(x => x.h.Id)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var items = rows.Select(x =>
            {
                var isLabour = x.g.ItemType == "Labour";
                return new WarrantyPackingSlipLineListViewModel
                {
                    WarrantyPackingSlipHeaderId = x.h.Id,
                    DetailId = x.d.Id,
                    ClaimNo = x.g.ClaimNo,
                    SlipPrefix = x.h.SlipPrefix,
                    SlipNo = x.h.SlipNo,
                    SlipDate = x.h.SlipDate,
                    InvoicePrefix = x.inv.InvoicePrefix,
                    InvoiceNo = x.inv.InvoiceNo,
                    InvoiceDate = x.inv.InvoiceDate,
                    BoxNumber = x.b.BoxNumber,
                    BoxType = x.b.BoxType,
                    ItemType = x.g.ItemType,
                    PartsNumber = isLabour ? x.g.LabourCode : x.g.PartCode,
                    PartsDescription = isLabour ? x.g.LabourDescription : x.g.PartDescription,
                    Qty = x.d.Qty,
                    Dimension = x.b.Length
                };
            }).ToList();

            return new WarrantyPackingSlipLineSearchResultViewModel
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
            };
        }

        public async Task<List<string>> SearchPackingSlipNos(string? dealerCode, string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return new List<string>();

            var query = _context.WarrantyPackingSlips.Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(dealerCode))
                query = query.Where(x => x.DealerCode == dealerCode);

            return await query
                .Where(x => (x.SlipPrefix + x.SlipNo).Contains(searchText))
                .Select(x => x.SlipPrefix + x.SlipNo)
                .Distinct()
                .OrderByDescending(s => s)
                .Take(20)
                .ToListAsync();
        }


        public async Task<List<string>> SearchPackingInvoiceNos(string? dealerCode, string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return new List<string>();

            var query =
                from h in _context.WarrantyPackingSlips
                join inv in _context.WarrantyInvoices on h.WarrantyInvoiceHeaderId equals inv.Id
                where h.IsActive
                select new { h, inv };

            if (!string.IsNullOrWhiteSpace(dealerCode))
                query = query.Where(x => x.h.DealerCode == dealerCode);

            return await query
                .Where(x => (x.inv.InvoicePrefix + x.inv.InvoiceNo).Contains(searchText))
                .Select(x => x.inv.InvoicePrefix + x.inv.InvoiceNo)
                .Distinct()
                .OrderByDescending(s => s)
                .Take(20)
                .ToListAsync();
        }

        private class PackingSlipPdfLine
        {
            public string? ClaimNo { get; set; }
            public string? ChassisNo { get; set; }
            public string? PartCode { get; set; }
            public string? PartDescription { get; set; }
            public decimal Qty { get; set; }
            public string? BoxType { get; set; }
            public string? BoxNumber { get; set; }
            public string? Dimension { get; set; }
        }

        private class PackingSlipPdfData
        {
            public WarrantyPackingSlip Header { get; set; } = null!;
            public WarrantyInvoice Invoice { get; set; } = null!;
            public DealerMaster? Dealer { get; set; }
            public LedgerMaster? Consignee { get; set; }
            public string? ConsigneeCityName { get; set; }
            public string? ConsigneeStateName { get; set; }
            public List<PackingSlipPdfLine> Lines { get; set; } = new();
        }

        private async Task<PackingSlipPdfData?> BuildPackingSlipPdfData(int packingSlipId)
        {
            var header = await _context.WarrantyPackingSlips
                .Include(x => x.WarrantyInvoiceHeader)
                .Include(x => x.WarrantyPackingSlipBoxes)
                    .ThenInclude(b => b.WarrantyPackingSlipDetails)
                        .ThenInclude(d => d.WarrantyOrderGridDetail)
                .FirstOrDefaultAsync(x => x.Id == packingSlipId);

            if (header == null || header.WarrantyInvoiceHeader == null)
                return null;

            var invoice = header.WarrantyInvoiceHeader;

            var dealer = !string.IsNullOrWhiteSpace(header.DealerCode)
                ? await _context.DealerMasters.FirstOrDefaultAsync(d => d.Dealercode == header.DealerCode)
                : null;

            // "To" party - the invoice's own Supplier, same resolution already used
            // for the Receiver/Consignee block on the Warranty Invoice PDFs
            // (WarrantyInvoiceRepo.BuildInvoicePdfData) - the OEM/manufacturer the
            // dealer sends warranty parts back to.
            var consignee = invoice.SupplierId.HasValue
                ? await _context.LedgerMasters.FirstOrDefaultAsync(l => l.Id == invoice.SupplierId.Value)
                : null;

            string? consigneeCityName = null;
            string? consigneeStateName = null;
            if (consignee != null)
            {
                if (consignee.City.HasValue)
                    consigneeCityName = await _context.Cities
                        .Where(c => c.CityId == consignee.City.Value)
                        .Select(c => c.CityName)
                        .FirstOrDefaultAsync();

                if (consignee.State.HasValue)
                    consigneeStateName = await _context.States
                        .Where(s => s.StateId == consignee.State.Value)
                        .Select(s => s.StateName)
                        .FirstOrDefaultAsync();
            }

            var lines = new List<PackingSlipPdfLine>();
            foreach (var box in header.WarrantyPackingSlipBoxes)
            {
                foreach (var detail in box.WarrantyPackingSlipDetails)
                {
                    var g = detail.WarrantyOrderGridDetail;
                    var isLabour = g?.ItemType == "Labour";

                    lines.Add(new PackingSlipPdfLine
                    {
                        ClaimNo = g?.ClaimNo,
                        ChassisNo = g?.ChassisNo,
                        PartCode = isLabour ? g?.LabourCode : g?.PartCode,
                        PartDescription = isLabour ? g?.LabourDescription : g?.PartDescription,
                        Qty = detail.Qty,
                        BoxType = box.BoxType,
                        BoxNumber = box.BoxNumber,
                        Dimension = box.Length
                    });
                }
            }

            return new PackingSlipPdfData
            {
                Header = header,
                Invoice = invoice,
                Dealer = dealer,
                Consignee = consignee,
                ConsigneeCityName = consigneeCityName,
                ConsigneeStateName = consigneeStateName,
                Lines = lines
            };
        }

        public async Task<byte[]> GenerateWarrantyPackingSlipPdf(int packingSlipId)
        {
            var data = await BuildPackingSlipPdfData(packingSlipId);
            if (data == null)
                throw new InvalidOperationException($"Warranty Packing Slip with Id {packingSlipId} not found.");

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(8));

                    page.Content().Column(col =>
                    {
                        col.Item().AlignCenter().Text(data.Dealer?.Compname ?? data.Header.DealerCode ?? "").FontSize(13).Bold();
                        col.Item().AlignCenter().Text(string.Join(", ", new[] {
                    data.Dealer?.Adress1, data.Dealer?.Adress2, data.Dealer?.City, data.Dealer?.State, data.Dealer?.Pin
                }.Where(s => !string.IsNullOrWhiteSpace(s)))).FontSize(8);

                        col.Item().PaddingTop(6).AlignCenter().Text("Warranty Packing Slip").FontSize(12).Bold();

                        col.Item().PaddingTop(8).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("From").Bold().Underline();
                                c.Item().Text($"Dlr Code : {data.Header.DealerCode}");
                                c.Item().Text($"Dlr Name : {data.Dealer?.Compname}");
                                c.Item().Text($"Dlr Address : {string.Join(", ", new[] { data.Dealer?.Adress1, data.Dealer?.Adress2 }.Where(s => !string.IsNullOrWhiteSpace(s)))}");
                                c.Item().Text($"City : {data.Dealer?.City}");
                                c.Item().Text($"State : {data.Dealer?.State}");
                                c.Item().Text($"GSTIN : {data.Dealer?.CompgstinNo}");
                            });

                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("To").Bold().Underline();
                                c.Item().Text($"Name : {data.Consignee?.LedgerName}");
                                c.Item().Text($"Address : {data.Consignee?.Address}");
                                c.Item().Text($"City : {data.ConsigneeCityName}");
                                c.Item().Text($"State : {data.ConsigneeStateName}");
                                c.Item().Text($"GSTIN : {data.Consignee?.Gstno}");
                            });
                        });

                        col.Item().PaddingTop(8).Row(row =>
                        {
                            row.RelativeItem().Text($"Packing Slip No : {data.Header.SlipPrefix}{data.Header.SlipNo}");
                            row.RelativeItem().Text($"Invoice / Sales Order No. : {data.Invoice.InvoicePrefix}{data.Invoice.InvoiceNo}");
                            row.RelativeItem().Text($"Batch No. : {data.Invoice.BatchNo}");
                        });
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"Packing Slip Date : {data.Header.SlipDate:dd-MM-yyyy}");
                            row.RelativeItem().Text($"Invoice / Sales Order Date : {data.Invoice.InvoiceDate:dd-MM-yyyy}");
                            row.RelativeItem().Text($"Batch Date : {data.Invoice.BatchDate:dd-MM-yyyy}");
                        });

                        col.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(22);    // Sr.No
                                columns.RelativeColumn(1.6f);  // Claim No / Chassis No
                                columns.RelativeColumn(2.2f);  // Part Code/Name / Description
                                columns.RelativeColumn(0.6f);  // Qty
                                columns.RelativeColumn(0.8f);  // Box Type
                                columns.RelativeColumn(0.8f);  // Box Number
                                columns.RelativeColumn(1.0f);  // Dimension (cms)
                                columns.RelativeColumn(1.0f);  // Remark
                            });

                            void HeaderCell(string text) => table.Cell().Border(1).Background(Colors.Grey.Lighten3).Padding(3).Text(text).Bold();

                            HeaderCell("Sr.No");
                            HeaderCell("Claim No.\nChassis No");
                            HeaderCell("Part Code/Name\nDescription");
                            HeaderCell("Qty");
                            HeaderCell("Box Type");
                            HeaderCell("Box Number");
                            HeaderCell("Dimension (Cms)");
                            HeaderCell("Remark");

                            int srNo = 1;
                            foreach (var line in data.Lines)
                            {
                                table.Cell().Border(1).Padding(3).Text(srNo++.ToString());
                                table.Cell().Border(1).Padding(3).Text($"{line.ClaimNo}\n{line.ChassisNo}");
                                table.Cell().Border(1).Padding(3).Text($"{line.PartCode}\n{line.PartDescription}");
                                table.Cell().Border(1).AlignCenter().Padding(3).Text(line.Qty.ToString("0.##"));
                                table.Cell().Border(1).Padding(3).Text(line.BoxType ?? "");
                                table.Cell().Border(1).Padding(3).Text(line.BoxNumber ?? "");
                                table.Cell().Border(1).Padding(3).Text(line.Dimension ?? "");
                                // Remark - GENUINELY UNCONFIRMED, no source column exists
                                // on WarrantyPackingSlipDetail; left blank.
                                table.Cell().Border(1).Padding(3).Text("");
                            }
                        });

                        col.Item().PaddingTop(10).Text("This is system generated Warranty Packing Slip.").FontSize(7).Italic();
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}