using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.EstimateRepo;
using DMS_BAPL_Utils.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.EstimateService
{
    public class EstimateService : IEstimateService
    {
        private readonly IEstimateRepo _repo;

        public EstimateService(IEstimateRepo repo)
        {
            _repo = repo;
        }

        public async Task<int> CreateAsync(EstimateCreateViewModel model, string userId)
        {
            try
            {
                var entity = new EstimateHeader
                {
                    EstimationNo = model.EstimationNo,
                    EstimateDate = model.EstimateDate,
                    ChassisNo = model.ChassisNo,
                    CustomerName = model.CustomerName,
                    CustomerMobile = model.CustomerMobile,
                    CustomerAddress = model.CustomerAddress,
                    CustomerPin = model.CustomerPin,
                    CustomerEmail = model.CustomerEmail,
                    CustomerCity = model.CustomerCity,
                    CustomerState = model.CustomerState,
                    Kms = model.Kms,
                    JobTypeId = model.JobTypeId,
                    DealerCode = model.DealerCode,
                    Status = "Open",
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    EstimateDetails = model.Details.Select(d => new EstimateDetail
                    {
                        ItemType = d.ItemType,
                        ItemCode = d.ItemCode,
                        ItemDescription = d.ItemDescription,
                        Qty = d.Qty,
                        Rate = d.Rate,
                        DiscountPercent = d.DiscountPercent,
                        DiscountAmount = Math.Round(d.Qty * d.Rate * d.DiscountPercent / 100, 2),
                        CgstPercent = d.CgstPercent,
                        SgstPercent = d.SgstPercent,
                        IgstPercent = d.IgstPercent,
                        CgstAmount = Math.Round(d.Qty * d.Rate * d.CgstPercent / 100, 2),
                        SgstAmount = Math.Round(d.Qty * d.Rate * d.SgstPercent / 100, 2),
                        IgstAmount = Math.Round(d.Qty * d.Rate * d.IgstPercent / 100, 2),
                        Amount = d.Amount
                    }).ToList()
                };

                return await _repo.CreateAsync(entity);
            }
            catch
            {
                throw;
            }
        }

        public async Task<EstimateResponseViewModel?> GetByIdAsync(int id)
        {
            try
            {
                var entity = await _repo.GetByIdAsync(id);
                if (entity == null) return null;

                var jobTypeName = await _repo.GetJobTypeNameAsync(entity.JobTypeId);

                return new EstimateResponseViewModel
                {
                    Id = entity.Id,
                    EstimationNo = entity.EstimationNo,
                    EstimateDate = entity.EstimateDate,
                    ChassisNo = entity.ChassisNo,
                    CustomerName = entity.CustomerName,
                    CustomerMobile = entity.CustomerMobile,
                    CustomerAddress = entity.CustomerAddress,
                    CustomerPin = entity.CustomerPin,
                    CustomerEmail = entity.CustomerEmail,
                    CustomerCity = entity.CustomerCity,
                    CustomerState = entity.CustomerState,
                    Kms = entity.Kms,
                    JobTypeId = entity.JobTypeId,
                    JobTypeName = jobTypeName,
                    DealerCode = entity.DealerCode,
                    Status = entity.Status,
                    CreatedDate = entity.CreatedDate,
                    Details = entity.EstimateDetails.Select(d => new EstimateDetailViewModel
                    {
                        Id = d.Id,
                        ItemType = d.ItemType,
                        ItemCode = d.ItemCode,
                        ItemDescription = d.ItemDescription,
                        Qty = d.Qty,
                        Rate = d.Rate,
                        DiscountPercent = d.DiscountPercent,
                        CgstPercent = d.CgstPercent,
                        SgstPercent = d.SgstPercent,
                        IgstPercent = d.IgstPercent,
                        Amount = d.Amount
                    }).ToList()
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<EstimatePagedResponse> GetAllAsync(EstimateFilterModel filter)
        {
            filter ??= new EstimateFilterModel();
            if (filter.PageIndex < 1) filter.PageIndex = 1;
            if (filter.PageSize < 1) filter.PageSize = 20;
            return await _repo.GetAllAsync(filter);
        }

        public async Task UpdateAsync(int id, EstimateCreateViewModel model, string userId)
        {
            var entity = await _repo.GetByIdAsync(id); // tracked, EstimateDetails included
            if (entity == null) throw new Exception("Estimate not found");

            entity.EstimationNo = model.EstimationNo;
            entity.EstimateDate = model.EstimateDate;
            entity.ChassisNo = model.ChassisNo;
            entity.CustomerName = model.CustomerName;
            entity.CustomerMobile = model.CustomerMobile;
            entity.CustomerAddress = model.CustomerAddress;
            entity.CustomerPin = model.CustomerPin;
            entity.CustomerEmail = model.CustomerEmail;
            entity.CustomerCity = model.CustomerCity;
            entity.CustomerState = model.CustomerState;
            entity.Kms = model.Kms;
            entity.JobTypeId = model.JobTypeId;
            // DealerCode is intentionally NOT overwritten here — it's set once at
            // creation and should never change on edit, regardless of what a
            // client sends.
            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;

            // ── Reconcile EstimateDetails: previously this method didn't touch
            // Details at all, so editing Parts/Labour lines was silently lost.
            // Now: remove lines no longer present, update existing lines by Id,
            // add brand-new lines (Id == 0). ──
            var incomingIds = model.Details.Where(d => d.Id > 0).Select(d => d.Id).ToHashSet();

            var toRemove = entity.EstimateDetails.Where(d => !incomingIds.Contains(d.Id)).ToList();
            foreach (var r in toRemove)
                entity.EstimateDetails.Remove(r);

            foreach (var d in model.Details)
            {
                if (d.Id > 0)
                {
                    var existingDetail = entity.EstimateDetails.FirstOrDefault(x => x.Id == d.Id);
                    if (existingDetail != null)
                    {
                        existingDetail.ItemType = d.ItemType;
                        existingDetail.ItemCode = d.ItemCode;
                        existingDetail.ItemDescription = d.ItemDescription;
                        existingDetail.Qty = d.Qty;
                        existingDetail.Rate = d.Rate;
                        existingDetail.DiscountPercent = d.DiscountPercent;
                        existingDetail.DiscountAmount = Math.Round(d.Qty * d.Rate * d.DiscountPercent / 100, 2);
                        existingDetail.CgstPercent = d.CgstPercent;
                        existingDetail.SgstPercent = d.SgstPercent;
                        existingDetail.IgstPercent = d.IgstPercent;
                        existingDetail.CgstAmount = Math.Round(d.Qty * d.Rate * d.CgstPercent / 100, 2);
                        existingDetail.SgstAmount = Math.Round(d.Qty * d.Rate * d.SgstPercent / 100, 2);
                        existingDetail.IgstAmount = Math.Round(d.Qty * d.Rate * d.IgstPercent / 100, 2);
                        existingDetail.Amount = d.Amount;
                    }
                }
                else
                {
                    entity.EstimateDetails.Add(new EstimateDetail
                    {
                        ItemType = d.ItemType,
                        ItemCode = d.ItemCode,
                        ItemDescription = d.ItemDescription,
                        Qty = d.Qty,
                        Rate = d.Rate,
                        DiscountPercent = d.DiscountPercent,
                        DiscountAmount = Math.Round(d.Qty * d.Rate * d.DiscountPercent / 100, 2),
                        CgstPercent = d.CgstPercent,
                        SgstPercent = d.SgstPercent,
                        IgstPercent = d.IgstPercent,
                        CgstAmount = Math.Round(d.Qty * d.Rate * d.CgstPercent / 100, 2),
                        SgstAmount = Math.Round(d.Qty * d.Rate * d.SgstPercent / 100, 2),
                        IgstAmount = Math.Round(d.Qty * d.Rate * d.IgstPercent / 100, 2),
                        Amount = d.Amount
                    });
                }
            }

            await _repo.UpdateAsync(entity);
        }
        public async Task<bool> DeleteAsync(int id) => await _repo.DeleteAsync(id);

        public async Task<string> GenerateNextEstimationNoAsync()
        {
            var last = await _repo.GetLastEstimationNoAsync();
            if (string.IsNullOrEmpty(last)) return "EST001";

            var numberPart = last.Replace("EST", "");
            if (!int.TryParse(numberPart, out int number)) return "EST001";

            number++;
            return $"EST{number.ToString("D3")}";
        }

        public async Task<List<JobTypeDropdownItem>> GetJobTypesAsync()
        {
            return await _repo.GetJobTypesAsync();
        }

        public async Task<List<PartSearchResultViewModel>> SearchPartsAsync(string query)
         => await _repo.SearchPartsAsync(query);

        public async Task<List<LabourSearchResultViewModel>> SearchLabourAsync(string query)
            => await _repo.SearchLabourAsync(query);

        public async Task<List<string>> GetEstimationNumbersAsync(string? dealerCode)
        {
            return await _repo.GetEstimationNumbersAsync(dealerCode);
        }

        public async Task<List<PartSearchResultViewModel>> SearchPartsAsync(string query, int maxResults = 20)
             => await _repo.SearchPartsAsync(query, maxResults);

        public async Task<List<LabourSearchResultViewModel>> SearchLabourAsync(string query, int maxResults = 20)
            => await _repo.SearchLabourAsync(query, maxResults);

        public async Task<byte[]?> DownloadEstimatePdfAsync(int id)
        {
            var model = await _repo.GetEstimatePrintDataAsync(id);
            if (model == null) return null;

            return GenerateEstimatePdf(model);
        }

        private byte[] GenerateEstimatePdf(EstimatePrintViewModel m)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            string Val(string? v) => string.IsNullOrWhiteSpace(v) ? "-" : v!;
            string Money(decimal v) => v.ToString("N2");

            const string HeaderBg = "#6F72A0";

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(24);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Content().Column(col =>
                    {
                        // -- Dealer header --
                        col.Item().AlignCenter().Column(h =>
                        {
                            if (!string.IsNullOrWhiteSpace(m.DealerName))
                                h.Item().AlignCenter().Text(m.DealerName).Bold().FontSize(12);
                            if (!string.IsNullOrWhiteSpace(m.DealerAddress))
                                h.Item().AlignCenter().Text(m.DealerAddress).FontSize(8);
                            h.Item().AlignCenter().Text($"Phone: {Val(m.DealerPhone)}   |   Email: {Val(m.DealerEmail)}").FontSize(8);
                            h.Item().AlignCenter()
                                .Text($"GSTIN: {Val(m.DealerGstin)}   |   PAN: {Val(m.DealerPan)}   |   Trade Cert No: {Val(m.DealerTradeCertNo)}").FontSize(8);
                        });

                        col.Item().PaddingTop(4).LineHorizontal(1);
                        col.Item().AlignCenter().Text("ESTIMATE").Bold().FontSize(13);
                        col.Item().LineHorizontal(1);

                        // -- Estimate meta + customer details --
                        void KV(ColumnDescriptor c, string label, string? value)
                        {
                            c.Item().PaddingVertical(1).Row(r =>
                            {
                                r.ConstantItem(110).Text(label).Bold().FontSize(8);
                                r.RelativeItem().Text($": {Val(value)}").FontSize(8);
                            });
                        }

                        col.Item().PaddingTop(6).Row(row =>
                        {
                            row.RelativeItem().Column(left =>
                            {
                                KV(left, "Customer Name", m.CustomerName);
                                KV(left, "Mobile", m.CustomerMobile);
                                KV(left, "Address", m.CustomerAddress);
                                KV(left, "City / State", $"{m.CustomerCity ?? ""} {m.CustomerState ?? ""}".Trim());
                                KV(left, "Pin", m.CustomerPin);
                                KV(left, "Email", m.CustomerEmail);
                            });
                            row.RelativeItem().Column(right =>
                            {
                                KV(right, "Estimate No", m.EstimationNo);
                                KV(right, "Estimate Date", m.EstimateDate.ToString("dd-MM-yyyy"));
                                KV(right, "Chassis No", m.ChassisNo);
                                KV(right, "Kms", m.Kms?.ToString());
                                KV(right, "Job Type", m.JobTypeName);
                            });
                        });

                        // -- Line item table (shared layout for Parts & Labour) --
                        void RenderLineTable(string title, List<EstimatePrintLineViewModel> lines, decimal subtotal)
                        {
                            if (lines.Count == 0) return;

                            col.Item().PaddingTop(10).Text(title).Bold().FontSize(10);

                            col.Item().PaddingTop(3).Table(t =>
                            {
                                t.ColumnsDefinition(c =>
                                {
                                    c.ConstantColumn(22);    // S.No
                                    c.RelativeColumn(1.4f);  // Code
                                    c.RelativeColumn(2.4f);  // Description
                                    c.RelativeColumn(0.8f);  // Qty
                                    c.RelativeColumn(1f);    // Rate
                                    c.RelativeColumn(0.9f);  // Disc %
                                    c.RelativeColumn(1.1f);  // CGST
                                    c.RelativeColumn(1.1f);  // SGST
                                    c.RelativeColumn(1.1f);  // IGST
                                    c.RelativeColumn(1.2f);  // Amount
                                });

                                void HCell(string text, bool right = false)
                                {
                                    var cell = t.Cell().Background(HeaderBg)
                                        .Border(0.5f).BorderColor(Colors.Grey.Medium).Padding(3);
                                    var span = (right ? cell.AlignRight() : cell).Text(text);
                                    span.Bold().FontSize(7.5f).FontColor(Colors.White);
                                }

                                t.Header(hr =>
                                {
                                    HCell("#"); HCell("Code"); HCell("Description");
                                    HCell("Qty", right: true); HCell("Rate", right: true);
                                    HCell("Disc%", right: true);
                                    HCell("CGST", right: true); HCell("SGST", right: true); HCell("IGST", right: true);
                                    HCell("Amount", right: true);
                                });

                                int sr = 1;
                                foreach (var l in lines)
                                {
                                    IContainer Body(IContainer c) =>
                                        c.Border(0.5f).BorderColor(Colors.Grey.Medium).Padding(3);

                                    Body(t.Cell()).Text(sr++.ToString()).FontSize(7.5f);
                                    Body(t.Cell()).Text(Val(l.ItemCode)).FontSize(7.5f);
                                    Body(t.Cell()).Text(Val(l.ItemDescription)).FontSize(7.5f);
                                    Body(t.Cell()).AlignRight().Text(l.Qty.ToString("0.##")).FontSize(7.5f);
                                    Body(t.Cell()).AlignRight().Text(Money(l.Rate)).FontSize(7.5f);
                                    Body(t.Cell()).AlignRight().Text(l.DiscountPercent.ToString("0.##")).FontSize(7.5f);
                                    Body(t.Cell()).AlignRight().Text(Money(l.CgstAmount)).FontSize(7.5f);
                                    Body(t.Cell()).AlignRight().Text(Money(l.SgstAmount)).FontSize(7.5f);
                                    Body(t.Cell()).AlignRight().Text(Money(l.IgstAmount)).FontSize(7.5f);
                                    Body(t.Cell()).AlignRight().Text(Money(l.Amount)).FontSize(7.5f);
                                }
                            });

                            col.Item().PaddingTop(2).Row(r =>
                            {
                                r.RelativeItem().Text($"Sub Total ({title})").Bold().FontSize(9);
                                r.ConstantItem(100).AlignRight().Text(Money(subtotal)).Bold().FontSize(9);
                            });
                        }

                        RenderLineTable("Parts", m.Parts, m.TotalPartsAmount);
                        RenderLineTable("Labour", m.Labour, m.TotalLabourAmount);

                        // -- Grand Total --
                        col.Item().PaddingTop(8).Border(1).BorderColor(Colors.Grey.Darken1)
                            .Background(Colors.Grey.Lighten3).PaddingVertical(6).PaddingHorizontal(8)
                            .Row(r =>
                            {
                                r.RelativeItem().Text("Grand Total").Bold().FontSize(11);
                                r.ConstantItem(120).AlignRight().Text(Money(m.GrandTotal)).Bold().FontSize(11);
                            });

                        // -- Terms (generic — no confirmed TermandConditionMaster
                        // module id exists for Estimate; adjust if one should be
                        // pulled from that master table instead) --
                        col.Item().PaddingTop(12).Text("Terms & Conditions").Bold().FontSize(9);
                        col.Item().PaddingLeft(12).Column(tc =>
                        {
                            tc.Item().Text("1.  This is an estimate only and not a final invoice. Actual charges may vary.").FontSize(8);
                            tc.Item().Text("2.  Prices are valid for 7 days from the estimate date.").FontSize(8);
                            tc.Item().Text("3.  Taxes as applicable.").FontSize(8);
                        });

                        // -- Signatures --
                        col.Item().PaddingTop(30).Row(r =>
                        {
                            r.RelativeItem().Text("Customer Signature").Bold().FontSize(9);
                            r.RelativeItem().AlignRight().Text($"For {Val(m.DealerName)}").Bold().FontSize(9);
                        });
                    });
                });
            }).GeneratePdf();
        }
    }
}