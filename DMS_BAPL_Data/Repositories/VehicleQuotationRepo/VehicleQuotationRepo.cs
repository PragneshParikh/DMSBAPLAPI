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
                                DealerCode = dealer != null ? dealer.Dealercode : null,
                                DealerName = dealer != null ? dealer.Compname : null,
                                CustomerId = q.CustomerId,
                                CustomerName = q.CustomerName,
                                MobileNo = q.MobileNo,
                                EmailId = q.EmailId,
                                Address = q.Address,
                                StateId = q.StateId,                                          // add
                                StateName = state != null ? state.StateName : null,           // add
                                CityId = q.CityId,                                            // add
                                CityName = city != null ? city.CityName : null,               // add
                                ModelId = q.ModelId,
                                ModelName = model != null ? model.ModelName : null,
                                VariantId = q.VariantId,
                                VariantName = null,
                                ColorId = q.ColorId,
                                ColorName = color != null ? color.Colorname : null,
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
                                //ValidTill = q.ValidTillDate ?? q.QuotationDate,
                                ValidTill = q.ValidTillDate != null
    ? new DateTime(q.ValidTillDate.Value.Year, q.ValidTillDate.Value.Month, q.ValidTillDate.Value.Day)
    : q.QuotationDate,
                                Remarks = q.Remarks,
                                IsApproved = false, // column no longer exists on VehicleQuotation
                                IsActive = q.IsActive,
                                CreatedBy = q.CreatedBy,
                                CreatedDate = q.CreatedDate,
                                UpdatedBy = q.ModifiedBy,
                                UpdatedDate = q.ModifiedDate
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
                                DealerId = q.DealerId,              // <-- add this
                                DealerCode = dealer != null ? dealer.Dealercode : null,
                                DealerName = dealer != null ? dealer.Compname : null,
                                Status = q.Status,                  // <-- add this
                                CustomerId = q.CustomerId,
                                CustomerName = q.CustomerName,
                                MobileNo = q.MobileNo,
                                EmailId = q.EmailId,
                                Address = q.Address,
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
                                //ValidTill = q.ValidTillDate ?? q.QuotationDate,
                                ValidTill = q.ValidTillDate != null ? new DateTime(q.ValidTillDate.Value.Year, q.ValidTillDate.Value.Month, q.ValidTillDate.Value.Day) : q.QuotationDate,
                                TotalAmount = q.TotalAmount,
                                Remarks = q.Remarks,
                                IsApproved = false, // column no longer exists on VehicleQuotation
                                IsActive = q.IsActive,
                                CreatedBy = q.CreatedBy,
                                CreatedDate = q.CreatedDate,
                                UpdatedBy = q.ModifiedBy,
                                UpdatedDate = q.ModifiedDate
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
                ModelId = model.ModelId,
                VariantId = model.VariantId,
                StateId = model.StateId,
                CityId = model.CityId,
                ColorId = model.ColorId,
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
                IsFinance = model.IsFinance,
                FinanceCompanyId = model.FinanceCompanyId,
                LoanAmount = model.LoanAmount,
                DownPayment = model.DownPayment,
                Status = model.Status,
                Remarks = model.Remarks,
                HypothecationAmount = model.HypothecationAmount,
                PlateAmount = model.PlateAmount,
                HandlingCharges = model.HandlingCharges,
                //ValidTillDate = model.ValidTillDate,
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
            existing.ModelId = model.ModelId;
            existing.StateId = model.StateId;
            existing.CityId = model.CityId;
            existing.VariantId = model.VariantId;
            existing.ColorId = model.ColorId;
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
            existing.IsFinance = model.IsFinance;
            existing.FinanceCompanyId = model.FinanceCompanyId;
            existing.LoanAmount = model.LoanAmount;
            existing.DownPayment = model.DownPayment;
            existing.Status = model.Status;
            existing.Remarks = model.Remarks;
            existing.HypothecationAmount = model.HypothecationAmount;
            existing.PlateAmount = model.PlateAmount;
            existing.HandlingCharges = model.HandlingCharges;
            //existing.ValidTillDate = model.ValidTillDate;
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
            var result = await
            (
                from q in _context.VehicleQuotations.AsNoTracking()

                join dealer in _context.DealerMasters.AsNoTracking()
                    on q.DealerId equals dealer.Id

                join model in _context.OemmodelMasters.AsNoTracking()
                    on q.ModelId equals model.Id

                join item in _context.ItemMasters.AsNoTracking()
                    on q.VariantId equals item.Id into itemJoin
                from item in itemJoin.DefaultIfEmpty()

                join color in _context.ColorMasters.AsNoTracking()
                    on q.ColorId equals color.Id into colorJoin
                from color in colorJoin.DefaultIfEmpty()

                join state in _context.States.AsNoTracking()
                    on q.StateId equals state.StateId into stateJoin
                from state in stateJoin.DefaultIfEmpty()

                join city in _context.Cities.AsNoTracking()
                    on q.CityId equals city.CityId into cityJoin
                from city in cityJoin.DefaultIfEmpty()

                join finance in _context.LedgerMasters.AsNoTracking()
                    on q.FinanceCompanyId equals finance.Id into financeJoin
                from finance in financeJoin.DefaultIfEmpty()

                where q.Id == quotationId

                select new VehicleQuotationPrintViewModel
                {
                    Id = q.Id,

                    QuotationNo = q.QuotationNo,
                    QuotationDate = q.QuotationDate,
                    ValidTill = Convert.ToDateTime(q.ValidTillDate),
                    Status = q.Status,

                    DealerId = q.DealerId,
                    DealerCode = dealer.Dealercode,
                    DealerName = dealer.Compname,

                    DealerAddress =
                        (dealer.Adress1 ?? "") + " " +
                        (dealer.Adress2 ?? "") + ", " +
                        (dealer.City ?? "") + ", " +
                        (dealer.State ?? "") + " - " +
                        (dealer.Pin ?? ""),

                    DealerMobile = dealer.Mobile,
                    DealerEmail = dealer.Email,
                    DealerGSTNo = dealer.CompgstinNo,

                    CustomerId = q.CustomerId,
                    CustomerName = q.CustomerName,
                    MobileNo = q.MobileNo,
                    EmailId = q.EmailId,
                    Address = q.Address,

                    StateId = q.StateId,
                    StateName = state != null ? state.StateName : "",

                    CityId = q.CityId,
                    CityName = city != null ? city.CityName : "",

                    ModelId = q.ModelId,
                    ModelName = model.ModelName,

                    // Variant from Item Master
                    VariantId = q.VariantId,
                    VariantName = item != null ? item.Itemdesc : "",

                    ColorId = q.ColorId,
                    ColorName = color != null ? color.Colorname : "",

                    //--------------------------------------------------
                    // Item Master
                    //--------------------------------------------------

                    CustPrice = item != null ? item.Custprice : 0,

                    Fame2Amount = item != null ? item.Fame2amount : 0,

                    SGST = item != null ? item.Sgst : 0,

                    CGST = item != null ? item.Cgst : 0,

                    IGST = item != null ? item.Igst : 0,

                    SGSTAmount = item != null
                        ? (item.Custprice * item.Sgst) / 100
                        : 0,

                    CGSTAmount = item != null
                        ? (item.Custprice * item.Cgst) / 100
                        : 0,

                    IGSTAmount = item != null
                        ? (item.Custprice * item.Igst) / 100
                        : 0,

                    TaxAmount =
                        item != null
                        ? ((item.Custprice * item.Sgst) / 100)
                        + ((item.Custprice * item.Cgst) / 100)
                        + ((item.Custprice * item.Igst) / 100)
                        : 0,

                    //--------------------------------------------------
                    // Charges
                    //--------------------------------------------------

                    ExShowroomPrice = q.ExShowroomPrice,

                    RTOCharges = q.Rtocharges,

                    InsuranceAmount = q.InsuranceAmount,

                    AccessoriesAmount = q.AccessoriesAmount,

                    ExtendedWarrantyAmount = q.ExtendedWarrantyAmount,

                    AmcAmount = q.Amcamount,

                    OtherCharges = q.OtherCharges,

                    DiscountAmount = q.DiscountAmount,

                    ExchangeAmount = q.ExchangeAmount,

                    HypothecationAmount = Convert.ToDecimal(q.HypothecationAmount),

                    PlateAmount = Convert.ToDecimal(q.PlateAmount),

                    HandlingCharges = Convert.ToDecimal(q.HandlingCharges),

                    //--------------------------------------------------
                    // Finance
                    //--------------------------------------------------

                    IsFinance = q.IsFinance,

                    FinanceCompanyId = q.FinanceCompanyId,

                    FinanceCompanyName =
                        finance != null ? finance.LedgerName : "",

                    LoanAmount = Convert.ToDecimal(q.LoanAmount),

                    DownPayment = Convert.ToDecimal(q.DownPayment),

                    //--------------------------------------------------

                    TotalAmount = q.TotalAmount,

                    Remarks = q.Remarks
                }

            ).FirstOrDefaultAsync();

            return result;
        }
    }
}