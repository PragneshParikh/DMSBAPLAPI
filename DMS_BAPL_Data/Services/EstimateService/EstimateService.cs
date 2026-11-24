using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.EstimateRepo;
using DMS_BAPL_Data.Services.EmailService;
using DMS_BAPL_Data.Services.VehicleInfoService;
using DMS_BAPL_Utils.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.EstimateService
{
    public class EstimateService : IEstimateService
    {
        private readonly IEstimateRepo _repo;
        private readonly IEmailService _emailService;
        private readonly IVehicleInfoService _vehicleInfoService;

        public EstimateService(
            IEstimateRepo repo,
            IEmailService emailService,
            IVehicleInfoService vehicleInfoService)
        {
            _repo = repo;
            _emailService = emailService;
            _vehicleInfoService = vehicleInfoService;
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
                    InsuranceId = model.InsuranceId,
                    InsDescription = model.InsDescription,
                    SurveyorName = model.SurveyorName,
                    ContactNumber = model.ContactNumber,
                    PolicyNo = model.PolicyNo,
                    InsValidTill = model.InsValidTill,
                    ZeroDepo = model.ZeroDepo,
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
                    InsuranceId = entity.InsuranceId,
                    InsDescription = entity.InsDescription,
                    SurveyorName = entity.SurveyorName,
                    ContactNumber = entity.ContactNumber,
                    PolicyNo = entity.PolicyNo,
                    InsValidTill = entity.InsValidTill,
                    ZeroDepo = entity.ZeroDepo,

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
            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.Now;
            entity.InsuranceId = model.InsuranceId;
            entity.InsDescription = model.InsDescription;
            entity.SurveyorName = model.SurveyorName;
            entity.ContactNumber = model.ContactNumber;
            entity.PolicyNo = model.PolicyNo;
            entity.InsValidTill = model.InsValidTill;
            entity.ZeroDepo = model.ZeroDepo;

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
            var model = await GetEstimatePrintModelAsync(id);
            if (model == null) return null;

            return GenerateEstimatePdf(model);
        }

        public async Task<byte[]?> EmailEstimatePdfAsync(
            int id,
            string toEmail,
            CancellationToken cancellationToken)
        {
            var model = await GetEstimatePrintModelAsync(id);
            if (model == null) return null;

            var pdfBytes = GenerateEstimatePdf(model);
            var estimateNo = string.IsNullOrWhiteSpace(model.EstimationNo)
                ? id.ToString(CultureInfo.InvariantCulture)
                : model.EstimationNo;
            await _emailService.SendEmailWithPdfAttachmentAsync(
                toEmail,
                $"Estimate {estimateNo}",
                """
                <p>Dear Customer,</p>
                <p>Please find your estimate PDF attached for your review. It has been prepared by BGAUSS AUTO PRIVATE LIMITED based on the vehicle and service details provided.</p>
                <p>For any clarification, please contact your BGAUSS dealership.</p>
                <p>Thank you,<br/>BGAUSS AUTO PRIVATE LIMITED<br/>BGAUSS DMS</p>
                """,
                pdfBytes,
                $"Estimate_{id}.pdf",
                cancellationToken);

            return pdfBytes;
        }

        private async Task<EstimatePrintViewModel?> GetEstimatePrintModelAsync(int id)
        {
            var model = await _repo.GetEstimatePrintDataAsync(id);
            if (model == null) return null;

            if (!string.IsNullOrWhiteSpace(model.ChassisNo))
            {
                var vehicleInfo = await _vehicleInfoService.GetVehicleInfoByRegNoChassis(
                    null,
                    model.ChassisNo,
                    model.DealerCode);
                if (vehicleInfo != null)
                {
                    model.RegistrationNo = vehicleInfo.VehicleDetails?.RegNo;
                    model.VehicleModel = vehicleInfo.VehicleDetails?.ModelName;
                }
            }

            return model;
        }

        private byte[] GenerateEstimatePdf(EstimatePrintViewModel m)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            string Val(string? v) => string.IsNullOrWhiteSpace(v) ? "-" : v!;
            string Money(decimal v) => $"Rs. {v.ToString("N2", CultureInfo.GetCultureInfo("en-IN"))}";

            bool hasInsurance =
                !string.IsNullOrWhiteSpace(m.InsuranceParty) ||
                !string.IsNullOrWhiteSpace(m.InsDescription) ||
                !string.IsNullOrWhiteSpace(m.SurveyorName) ||
                !string.IsNullOrWhiteSpace(m.ContactNumber) ||
                !string.IsNullOrWhiteSpace(m.PolicyNo) ||
                m.InsValidTill.HasValue;

            const string HeaderBg = "#264f72";

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(24);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Content().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(dealer =>
                            {
                                if (!string.IsNullOrWhiteSpace(m.DealerName))
                                    dealer.Item().Text(m.DealerName).Bold().FontSize(11).FontColor(HeaderBg);
                                dealer.Item().Text($"Dealer Code: {Val(m.DealerCode)}").FontSize(7);
                                if (!string.IsNullOrWhiteSpace(m.DealerAddress))
                                    dealer.Item().Text(m.DealerAddress).FontSize(7);
                                dealer.Item().Text($"Phone: {Val(m.DealerPhone)}  |  Email: {Val(m.DealerEmail)}")
                                    .FontSize(7);
                            });
                            row.ConstantItem(180).AlignRight().Column(meta =>
                            {
                                meta.Item().Text("ESTIMATE").Bold().FontSize(12).FontColor(HeaderBg);
                                meta.Item().Text($"Estimate No: {Val(m.EstimationNo)}").FontSize(8);
                                meta.Item().Text($"Date: {m.EstimateDate:dd-MM-yyyy}").FontSize(8);
                                meta.Item().Text($"Job Card No: {Val(m.JobCardNo?.ToString())}").FontSize(8);
                            });
                        });

                        col.Item().PaddingTop(4).LineHorizontal(1).LineColor(HeaderBg);

                        void KV(ColumnDescriptor c, string label, string? value)
                        {
                            c.Item().PaddingVertical(1).Row(r =>
                            {
                                r.ConstantItem(78).Text(label).Bold().FontSize(7);
                                r.RelativeItem().Text($": {Val(value)}").FontSize(7);
                            });
                        }

                        col.Item().PaddingTop(7).Row(row =>
                        {
                            row.RelativeItem().Border(0.5f).BorderColor(Colors.Grey.Medium).Column(box =>
                            {
                                box.Item().Background(HeaderBg).Padding(4)
                                    .Text("CUSTOMER DETAILS").Bold().FontSize(8).FontColor(Colors.White);
                                box.Item().Padding(5).Column(details =>
                                {
                                    KV(details, "Customer Name", m.CustomerName);
                                    KV(details, "Mobile", m.CustomerMobile);
                                    KV(details, "Address", m.CustomerAddress);
                                    KV(details, "City", m.CustomerCity);
                                });
                            });
                            row.ConstantItem(8);
                            row.RelativeItem().Border(0.5f).BorderColor(Colors.Grey.Medium).Column(box =>
                            {
                                box.Item().Background(HeaderBg).Padding(4)
                                    .Text("VEHICLE DETAILS").Bold().FontSize(8).FontColor(Colors.White);
                                box.Item().Padding(5).Column(details =>
                                {
                                    KV(details, "Model", m.VehicleModel);
                                    KV(details, "Registration No", m.RegistrationNo);
                                    KV(details, "Chassis No", m.ChassisNo);
                                    KV(details, "Odometer", m.Kms?.ToString());
                                });
                            });
                        });

                        if (hasInsurance)
                        {
                            col.Item().PaddingTop(7).Border(0.5f).BorderColor(Colors.Grey.Medium)
                                .Column(box =>
                                {
                                    box.Item().Background(HeaderBg).Padding(4)
                                        .Text("INSURANCE DETAILS").Bold().FontSize(8).FontColor(Colors.White);
                                    box.Item().Padding(5).Row(row =>
                                    {
                                        row.RelativeItem().Column(left =>
                                        {
                                            KV(left, "Insurance Party", m.InsuranceParty);
                                            KV(left, "Description", m.InsDescription);
                                            KV(left, "Surveyor Name", m.SurveyorName);
                                        });
                                        row.RelativeItem().Column(right =>
                                        {
                                            KV(right, "Contact Number", m.ContactNumber);
                                            KV(right, "Policy No", m.PolicyNo);
                                            KV(right, "Valid Till", m.InsValidTill?.ToString("dd-MM-yyyy"));
                                            KV(right, "Zero Depreciation", m.ZeroDepo ? "Yes" : "No");
                                        });
                                    });
                                });
                        }

                        void RenderLineTable(
                            string title,
                            List<EstimatePrintLineViewModel> lines,
                            decimal subtotal,
                            bool isPart)
                        {
                            col.Item().PaddingTop(8).Text(title).Bold().FontSize(9).FontColor(HeaderBg);
                            if (lines.Count == 0)
                            {
                                col.Item().Border(0.5f).BorderColor(Colors.Grey.Medium)
                                    .Padding(5)
                                    .Text(isPart ? "No parts added." : "No labour suggested.")
                                    .FontSize(7);
                                return;
                            }

                            col.Item().PaddingTop(3).Table(t =>
                            {
                                t.ColumnsDefinition(c =>
                                {
                                    c.ConstantColumn(22);
                                    c.RelativeColumn(1.3f);
                                    c.RelativeColumn(2.5f);
                                    c.RelativeColumn(0.9f);
                                    c.RelativeColumn(1f);
                                    c.ConstantColumn(34);
                                    c.RelativeColumn(1.2f);
                                });

                                void HCell(string text, bool right = false)
                                {
                                    var cell = t.Cell().Background(HeaderBg)
                                        .Border(0.5f).BorderColor(Colors.Grey.Medium).Padding(4);
                                    var span = (right ? cell.AlignRight() : cell).Text(text);
                                    span.Bold().FontSize(7).FontColor(Colors.White);
                                }

                                t.Header(hr =>
                                {
                                    HCell("#");
                                    HCell(isPart ? "Item Code" : "Labour Code");
                                    HCell("Description");
                                    HCell("HSN");
                                    HCell(isPart ? "MRP" : "Rate", right: true);
                                    HCell("Qty", right: true);
                                    HCell("Amount", right: true);
                                });

                                int sr = 1;
                                foreach (var l in lines)
                                {
                                    IContainer Body(IContainer c) =>
                                        c.Border(0.5f).BorderColor(Colors.Grey.Medium).Padding(4);

                                    Body(t.Cell()).Text(sr++.ToString()).FontSize(7);
                                    Body(t.Cell()).Text(Val(l.ItemCode)).FontSize(7);
                                    Body(t.Cell()).Text(Val(l.ItemDescription)).FontSize(7);
                                    Body(t.Cell()).Text(Val(l.HsnCode)).FontSize(7);
                                    Body(t.Cell()).AlignRight()
                                        .Text(Money(isPart ? l.Mrp ?? l.Rate : l.Rate)).FontSize(7);
                                    Body(t.Cell()).AlignRight().Text(l.Qty.ToString("0.##")).FontSize(7);
                                    Body(t.Cell()).AlignRight().Text(Money(l.Amount)).FontSize(7);
                                }
                            });

                            col.Item().PaddingTop(2).Row(r =>
                            {
                                r.RelativeItem().Text($"{title} Total").Bold().FontSize(8);
                                r.ConstantItem(120).AlignRight().Text(Money(subtotal)).Bold().FontSize(8);
                            });
                        }

                        RenderLineTable("PART DETAILS", m.Parts, m.TotalPartsAmount, isPart: true);
                        RenderLineTable("LABOUR DETAILS", m.Labour, m.TotalLabourAmount, isPart: false);

                        col.Item().PaddingTop(7).Border(1).BorderColor(HeaderBg)
                            .Background(Colors.Grey.Lighten3).PaddingVertical(7).PaddingHorizontal(8)
                            .Row(r =>
                            {
                                r.RelativeItem().Text("Grand Total").Bold().FontSize(10);
                                r.ConstantItem(120).AlignRight().Text(Money(m.GrandTotal)).Bold().FontSize(10);
                            });
                    });
                });
            }).GeneratePdf();
        }
    }
}