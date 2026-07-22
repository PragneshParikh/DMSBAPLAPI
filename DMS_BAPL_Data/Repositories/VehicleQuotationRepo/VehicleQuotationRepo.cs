using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.VehicleQuotationRepo
{
    public class VehicleQuotationRepo : IVehicleQuotationRepo
    {
        private readonly BapldmsvadContext _context;

        public VehicleQuotationRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        Task<bool> IVehicleQuotationRepo.DeleteAsync(long id) => DeleteInternal(id);

        Task<string> IVehicleQuotationRepo.GenerateQuotationNo() => GenerateQuotationNoInternal();
        Task<List<VehicleQuotationViewModel>> IVehicleQuotationRepo.GetAllAsync(string? dealerCode) => GetAllInternal(dealerCode);

        Task<VehicleQuotationViewModel> IVehicleQuotationRepo.GetByIdAsync(long id) => GetByIdInternal(id);

        Task<long> IVehicleQuotationRepo.InsertAsync(AddVehicleQuotationViewModel model) => InsertInternal(model);

        Task<bool> IVehicleQuotationRepo.UpdateAsync(AddVehicleQuotationViewModel model, string userId) => UpdateInternal(model, userId);

        private async Task<string> GenerateQuotationNoInternal()
        {
            try
            {
                string prefix = $"VQ{DateTime.Now:yyyy}";

                var lastQuotationNo = await _context.VehicleQuotations
                    .AsNoTracking()
                    .Where(q => q.QuotationNo.StartsWith(prefix))
                    .OrderByDescending(q => q.QuotationNo)
                    .Select(q => q.QuotationNo)
                    .FirstOrDefaultAsync();

                int nextSeq = 1;
                if (!string.IsNullOrEmpty(lastQuotationNo))
                {
                    var seqPart = lastQuotationNo.Substring(prefix.Length);
                    if (int.TryParse(seqPart, out int lastSeq))
                        nextSeq = lastSeq + 1;
                }

                return $"{prefix}{nextSeq:D5}";
            }
            catch { throw; }
        }

        private async Task<List<VehicleQuotationViewModel>> GetAllInternal(string? dealerCode = null)
        {
            try
            {
                var query = from q in _context.VehicleQuotations.AsNoTracking()
                            join color in _context.ColorMasters.AsNoTracking()
                                on q.ColorId equals color.Id into colorJoin
                            from color in colorJoin.DefaultIfEmpty()
                            join dealer in _context.DealerMasters.AsNoTracking()
                                on q.DealerId equals dealer.Id into dealerJoin
                            from dealer in dealerJoin.DefaultIfEmpty()
                            join model in _context.OemmodelMasters.AsNoTracking()
                                on q.ModelId equals model.Id into modelJoin
                            from model in modelJoin.DefaultIfEmpty()
                            join state in _context.States
                                on q.StateId equals state.StateId into stateJoin
                            from state in stateJoin.DefaultIfEmpty()
                            join city in _context.Cities
                                on q.CityId equals city.CityId into cityJoin
                            from city in cityJoin.DefaultIfEmpty()
                            where string.IsNullOrWhiteSpace(dealerCode)
                                || (dealer != null && dealer.Dealercode == dealerCode)
                            orderby q.CreatedDate descending
                            select new VehicleQuotationViewModel
                            {
                                VehicleQuotationId = q.Id,
                                QuotationNo = q.QuotationNo,
                                QuotationDate = q.QuotationDate,
                                DealerId = q.DealerId,
                                DealerCode = dealer != null ? dealer.Dealercode : null,
                                DealerName = dealer != null ? dealer.Compname : null,
                                Status = q.Status,
                                CustomerId = q.CustomerId,
                                CustomerName = q.CustomerName,
                                MobileNo = q.MobileNo,
                                EmailId = q.EmailId,
                                Address = q.Address,
                                CustomerGSTNo = q.CustomerGstno,
                                CustomerPanNo = q.CustomerPanNo,
                                StateId = q.StateId,
                                StateName = state != null ? state.StateName : null,
                                CityId = q.CityId,
                                CityName = city != null ? city.CityName : null,
                                ModelId = q.ModelId,
                                ModelName = model != null ? model.ModelName : null,
                                VariantId = q.VariantId,
                                VariantName = null,
                                ColorId = q.ColorId,
                                ColorName = color != null ? color.Colorname : null,
                                CustPrice = q.CustPrice ?? 0,
                                Fame2Amount = q.Fame2Amount ?? 0,
                                SgstAmount = q.SgstAmount ?? 0,
                                CgstAmount = q.CgstAmount ?? 0,
                                IgstAmount = q.IgstAmount ?? 0,
                                TaxAmount = q.TaxAmount,
                                ExShowroomPrice = q.ExShowroomPrice,
                                InsuranceAmount = q.InsuranceAmount,
                                RegistrationAmount = q.Rtocharges,
                                AccessoriesAmount = q.AccessoriesAmount,
                                ExtendedWarrantyAmount = q.ExtendedWarrantyAmount,
                                OtherCharges = q.OtherCharges,
                                DiscountAmount = q.DiscountAmount,
                                HypothecationAmount = q.HypothecationAmount,
                                PlateAmount = q.PlateAmount,
                                HandlingCharges = q.HandlingCharges,
                                TotalAmount = q.TotalAmount,
                                ValidTill = q.ValidTillDate != null
                                    ? new DateTime(q.ValidTillDate.Value.Year, q.ValidTillDate.Value.Month, q.ValidTillDate.Value.Day)
                                    : q.QuotationDate,
                                Remarks = q.Remarks,
                                IsApproved = false,
                                IsActive = q.IsActive,
                                CreatedBy = q.CreatedBy,
                                CreatedDate = q.CreatedDate,
                                UpdatedBy = q.ModifiedBy,
                                UpdatedDate = q.ModifiedDate,

                                // FIX: previously missing — see class-level comment
                                IsExchange = q.IsExchange,
                                ExchangeAmount = q.ExchangeAmount,
                                IsFinance = q.IsFinance,
                                FinanceCompanyId = q.FinanceCompanyId,
                                LoanAmount = q.LoanAmount,
                                DownPayment = q.DownPayment,

                                // NEW
                                OldCompanyName = q.OldCompanyName,
                                OldModelName = q.OldModelName
                            };

                return await query.ToListAsync();
            }
            catch { throw; }
        }

        private async Task<VehicleQuotationViewModel> GetByIdInternal(long id)
        {
            try
            {
                var query = from q in _context.VehicleQuotations.AsNoTracking()
                            join color in _context.ColorMasters.AsNoTracking()
                                on q.ColorId equals color.Id into colorJoin
                            from color in colorJoin.DefaultIfEmpty()
                            join dealer in _context.DealerMasters.AsNoTracking()
                                on q.DealerId equals dealer.Id into dealerJoin
                            from dealer in dealerJoin.DefaultIfEmpty()
                            join model in _context.OemmodelMasters.AsNoTracking()
                                on q.ModelId equals model.Id into modelJoin
                            from model in modelJoin.DefaultIfEmpty()
                            join state in _context.States
                                on q.StateId equals state.StateId into stateJoin
                            from state in stateJoin.DefaultIfEmpty()
                            join city in _context.Cities
                                on q.CityId equals city.CityId into cityJoin
                            from city in cityJoin.DefaultIfEmpty()
                            where q.Id == id
                            select new VehicleQuotationViewModel
                            {
                                VehicleQuotationId = q.Id,
                                QuotationNo = q.QuotationNo,
                                QuotationDate = q.QuotationDate,
                                DealerId = q.DealerId,
                                DealerCode = dealer != null ? dealer.Dealercode : null,
                                DealerName = dealer != null ? dealer.Compname : null,
                                Status = q.Status,
                                CustomerId = q.CustomerId,
                                CustomerName = q.CustomerName,
                                MobileNo = q.MobileNo,
                                EmailId = q.EmailId,
                                Address = q.Address,
                                CustomerGSTNo = q.CustomerGstno,
                                CustomerPanNo = q.CustomerPanNo,
                                StateId = q.StateId,
                                StateName = state != null ? state.StateName : null,
                                CityId = q.CityId,
                                CityName = city != null ? city.CityName : null,
                                ModelId = q.ModelId,
                                ModelName = model != null ? model.ModelName : null,
                                VariantId = q.VariantId,
                                VariantName = null, // TODO: join Variant table once entity/DbSet is available
                                ColorId = q.ColorId,
                                ColorName = color != null ? color.Colorname : null,
                                CustPrice = q.CustPrice ?? 0,
                                Fame2Amount = q.Fame2Amount ?? 0,
                                SgstAmount = q.SgstAmount ?? 0,
                                CgstAmount = q.CgstAmount ?? 0,
                                IgstAmount = q.IgstAmount ?? 0,
                                TaxAmount = q.TaxAmount,
                                ExShowroomPrice = q.ExShowroomPrice,
                                InsuranceAmount = q.InsuranceAmount,
                                RegistrationAmount = q.Rtocharges,
                                AccessoriesAmount = q.AccessoriesAmount,
                                ExtendedWarrantyAmount = q.ExtendedWarrantyAmount,
                                OtherCharges = q.OtherCharges,
                                DiscountAmount = q.DiscountAmount,
                                HypothecationAmount = q.HypothecationAmount,
                                PlateAmount = q.PlateAmount,
                                HandlingCharges = q.HandlingCharges,
                                ValidTill = q.ValidTillDate != null ? new DateTime(q.ValidTillDate.Value.Year, q.ValidTillDate.Value.Month, q.ValidTillDate.Value.Day) : q.QuotationDate,
                                TotalAmount = q.TotalAmount,
                                Remarks = q.Remarks,
                                IsApproved = false,
                                IsActive = q.IsActive,
                                CreatedBy = q.CreatedBy,
                                CreatedDate = q.CreatedDate,
                                UpdatedBy = q.ModifiedBy,
                                UpdatedDate = q.ModifiedDate,

                                // FIX: previously missing — see class-level comment
                                IsExchange = q.IsExchange,
                                ExchangeAmount = q.ExchangeAmount,
                                IsFinance = q.IsFinance,
                                FinanceCompanyId = q.FinanceCompanyId,
                                LoanAmount = q.LoanAmount,
                                DownPayment = q.DownPayment,

                                // NEW
                                OldCompanyName = q.OldCompanyName,
                                OldModelName = q.OldModelName
                            };

                return await query.FirstOrDefaultAsync();
            }
            catch { throw; }
        }

        private async Task<long> InsertInternal(AddVehicleQuotationViewModel model)
        {
            var quotation = new VehicleQuotation
            {
                QuotationNo = model.QuotationNo,
                QuotationDate = model.QuotationDate,
                DealerId = model.DealerId,
                CustomerId = model.CustomerId,
                CustomerName = model.CustomerName,
                MobileNo = model.MobileNo,
                EmailId = model.EmailId,
                Address = model.Address,
                CustomerGstno = model.CustomerGSTNo,
                CustomerPanNo = model.CustomerPanNo,
                ModelId = model.ModelId,
                VariantId = model.VariantId,
                StateId = model.StateId,
                CityId = model.CityId,
                ColorId = model.ColorId,
                CustPrice = model.CustPrice,
                Fame2Amount = model.Fame2Amount,
                SgstAmount = model.SgstAmount,
                CgstAmount = model.CgstAmount,
                IgstAmount = model.IgstAmount,
                ExShowroomPrice = model.ExShowroomPrice,
                Rtocharges = model.RTOCharges,
                InsuranceAmount = model.InsuranceAmount,
                AccessoriesAmount = model.AccessoriesAmount,
                ExtendedWarrantyAmount = model.ExtendedWarrantyAmount,
                Amcamount = model.AMCAmount,
                OtherCharges = model.OtherCharges,
                DiscountAmount = model.DiscountAmount,
                TaxAmount = model.TaxAmount,
                TotalAmount = model.TotalAmount,
                IsExchange = model.IsExchange,
                ExchangeAmount = model.ExchangeAmount,
                // NEW
                OldCompanyName = model.OldCompanyName,
                OldModelName = model.OldModelName,
                IsFinance = model.IsFinance,
                FinanceCompanyId = model.FinanceCompanyId,
                LoanAmount = model.LoanAmount,
                DownPayment = model.DownPayment,
                Status = model.Status,
                Remarks = model.Remarks,
                HypothecationAmount = model.HypothecationAmount,
                PlateAmount = model.PlateAmount,
                HandlingCharges = model.HandlingCharges,
                ValidTillDate = model.ValidTillDate.HasValue ? DateOnly.FromDateTime(model.ValidTillDate.Value) : null,
                IsActive = true,
                CreatedBy = model.CreatedBy,
                CreatedDate = DateTime.Now
            };

            _context.VehicleQuotations.Add(quotation);
            await _context.SaveChangesAsync();
            return quotation.Id;
        }

        private async Task<bool> UpdateInternal(AddVehicleQuotationViewModel model, string userId)
        {
            var existing = await _context.VehicleQuotations
                .FirstOrDefaultAsync(q => q.Id == model.Id);
            if (existing == null) return false;

            existing.QuotationDate = model.QuotationDate;
            existing.DealerId = model.DealerId;
            existing.CustomerId = model.CustomerId;
            existing.CustomerName = model.CustomerName;
            existing.MobileNo = model.MobileNo;
            existing.EmailId = model.EmailId;
            existing.Address = model.Address;
            existing.CustomerGstno = model.CustomerGSTNo;
            existing.CustomerPanNo = model.CustomerPanNo;
            existing.ModelId = model.ModelId;
            existing.StateId = model.StateId;
            existing.CityId = model.CityId;
            existing.VariantId = model.VariantId;
            existing.ColorId = model.ColorId;
            existing.CustPrice = model.CustPrice;
            existing.Fame2Amount = model.Fame2Amount;
            existing.SgstAmount = model.SgstAmount;
            existing.CgstAmount = model.CgstAmount;
            existing.IgstAmount = model.IgstAmount;
            existing.ExShowroomPrice = model.ExShowroomPrice;
            existing.Rtocharges = model.RTOCharges;
            existing.InsuranceAmount = model.InsuranceAmount;
            existing.AccessoriesAmount = model.AccessoriesAmount;
            existing.ExtendedWarrantyAmount = model.ExtendedWarrantyAmount;
            existing.Amcamount = model.AMCAmount;
            existing.OtherCharges = model.OtherCharges;
            existing.DiscountAmount = model.DiscountAmount;
            existing.TaxAmount = model.TaxAmount;
            existing.TotalAmount = model.TotalAmount;
            existing.IsExchange = model.IsExchange;
            existing.ExchangeAmount = model.ExchangeAmount;
            // NEW
            existing.OldCompanyName = model.OldCompanyName;
            existing.OldModelName = model.OldModelName;
            existing.IsFinance = model.IsFinance;
            existing.FinanceCompanyId = model.FinanceCompanyId;
            existing.LoanAmount = model.LoanAmount;
            existing.DownPayment = model.DownPayment;
            existing.Status = model.Status;
            existing.Remarks = model.Remarks;
            existing.HypothecationAmount = model.HypothecationAmount;
            existing.PlateAmount = model.PlateAmount;
            existing.HandlingCharges = model.HandlingCharges;
            existing.ValidTillDate = model.ValidTillDate.HasValue ? DateOnly.FromDateTime(model.ValidTillDate.Value) : null;
            existing.ModifiedBy = userId;
            existing.ModifiedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<bool> DeleteInternal(long id)
        {
            try
            {
                var affectedRows = await _context.VehicleQuotations
                    .Where(q => q.Id == id)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(q => q.IsActive, false)
                        .SetProperty(q => q.ModifiedDate, DateTime.Now)
                    );

                return affectedRows > 0;
            }
            catch { throw; }
        }

        public async Task<VehicleQuotationPrintViewModel> GetPrintQuotationAsync(long quotationId)
        {
            var quotation = await _context.VehicleQuotations
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == quotationId);

            if (quotation == null) return null;

            var dealer = await _context.DealerMasters
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == quotation.DealerId);

            var model = await _context.OemmodelMasters
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == quotation.ModelId);

            var item = await _context.ItemMasters
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == quotation.VariantId);

            var color = quotation.ColorId.HasValue
                ? await _context.ColorMasters.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == quotation.ColorId.Value)
                : null;

            var state = quotation.StateId.HasValue
                ? await _context.States.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.StateId == quotation.StateId.Value)
                : null;

            var city = quotation.CityId.HasValue
                ? await _context.Cities.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.CityId == quotation.CityId.Value)
                : null;

            var finance = quotation.FinanceCompanyId.HasValue
                ? await _context.LedgerMasters.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == quotation.FinanceCompanyId.Value)
                : null;

            decimal custPrice = quotation.CustPrice ?? 0;
            decimal fame2Amount = quotation.Fame2Amount ?? 0;
            decimal sgstAmount = quotation.SgstAmount ?? 0;
            decimal cgstAmount = quotation.CgstAmount ?? 0;
            decimal igstAmount = quotation.IgstAmount ?? 0;

            decimal PercentOf(decimal amount) =>
                custPrice > 0 ? Math.Round(amount / custPrice * 100, 2) : 0;

            return new VehicleQuotationPrintViewModel
            {
                Id = quotation.Id,

                QuotationNo = quotation.QuotationNo,
                QuotationDate = quotation.QuotationDate,
                ValidTill = quotation.ValidTillDate.HasValue
                    ? quotation.ValidTillDate.Value.ToDateTime(TimeOnly.MinValue)
                    : (DateTime?)null,
                Status = quotation.Status,

                DealerId = quotation.DealerId,
                DealerCode = dealer?.Dealercode,
                DealerName = dealer?.Compname,

                DealerAddress = dealer == null ? "" :
                    (dealer.Adress1 ?? "") + " " +
                    (dealer.Adress2 ?? "") + ", " +
                    (dealer.City ?? "") + ", " +
                    (dealer.State ?? "") + " - " +
                    (dealer.Pin ?? ""),

                DealerMobile = dealer?.Mobile,
                DealerEmail = dealer?.Email,
                DealerGSTNo = dealer?.CompgstinNo,

                CustomerId = quotation.CustomerId,
                CustomerName = quotation.CustomerName,
                MobileNo = quotation.MobileNo,
                EmailId = quotation.EmailId,
                Address = quotation.Address,

                StateId = quotation.StateId,
                StateName = state?.StateName ?? "",

                CityId = quotation.CityId,
                CityName = city?.CityName ?? "",

                CustomerGSTNo = quotation.CustomerGstno,
                CustomerPanNo = quotation.CustomerPanNo,

                ModelId = quotation.ModelId,
                ModelName = model?.ModelName,

                VariantId = quotation.VariantId,
                VariantName = item?.Itemname ?? "",

                ColorId = quotation.ColorId,
                ColorName = color?.Colorname ?? "",

                CustPrice = custPrice,
                Fame2Amount = fame2Amount,

                SGST = PercentOf(sgstAmount),
                CGST = PercentOf(cgstAmount),
                IGST = PercentOf(igstAmount),

                SGSTAmount = sgstAmount,
                CGSTAmount = cgstAmount,
                IGSTAmount = igstAmount,

                TaxAmount = quotation.TaxAmount,

                ExShowroomPrice = quotation.ExShowroomPrice,

                RTOCharges = quotation.Rtocharges,

                InsuranceAmount = quotation.InsuranceAmount,

                AccessoriesAmount = quotation.AccessoriesAmount,

                ExtendedWarrantyAmount = quotation.ExtendedWarrantyAmount,

                AmcAmount = quotation.Amcamount,

                OtherCharges = quotation.OtherCharges,

                DiscountAmount = quotation.DiscountAmount,

                ExchangeAmount = quotation.ExchangeAmount,

                // NEW
                OldCompanyName = quotation.OldCompanyName,
                OldModelName = quotation.OldModelName,

                HypothecationAmount = quotation.HypothecationAmount ?? 0,

                PlateAmount = quotation.PlateAmount ?? 0,

                HandlingCharges = quotation.HandlingCharges ?? 0,

                IsFinance = quotation.IsFinance,

                FinanceCompanyId = quotation.FinanceCompanyId,

                FinanceCompanyName = finance?.LedgerName ?? "",

                LoanAmount = quotation.LoanAmount ?? 0,

                DownPayment = quotation.DownPayment ?? 0,

                TotalAmount = quotation.TotalAmount,

                Remarks = quotation.Remarks
            };
        }
    }
}