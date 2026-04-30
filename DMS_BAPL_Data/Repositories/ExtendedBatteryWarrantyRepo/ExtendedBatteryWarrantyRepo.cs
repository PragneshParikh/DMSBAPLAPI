using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Spreadsheet;
using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.ExtendedBatteryWarrantyRepo
{
    public partial class ExtendedBatteryWarrantyRepo : IExtendedBatteryWarrantyRepo
    {
        private readonly BapldmsvadContext _context;

        public ExtendedBatteryWarrantyRepo(BapldmsvadContext context)
        {
            _context = context;
        }
        async Task<IEnumerable<ExtendedBatteryWarranty>> IExtendedBatteryWarrantyRepo.Get()
        {
            return await _context.ExtendedBatteryWarranties
                .ToListAsync();
        }

        async Task<PagedResponse<object>> IExtendedBatteryWarrantyRepo.GetExtendedBatteryWarrantyByPaged(string? searchTerms, int pageIndex, int pageSize)
        {
            try
            {
                var query = _context.ExtendedBatteryWarranties.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(searchTerms))
                {
                    query = query.Where(c => c.SchemeName.Contains(searchTerms));
                }

                int totalRecords = await query.CountAsync();

                var items = await query
                    .AsNoTracking()
                    .OrderBy(c => c.SchemeName)
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                int startSrNo = (pageIndex * pageSize) + 1;

                var viewModelItems = items.Select((item, index) => new
                {
                    srNo = startSrNo + index,
                    Id = item.Id,
                    OemmodelId = item.OemmodelId,
                    SchemeName = item.SchemeName,
                    RateType = item.RateType,
                    Duration = item.Duration,
                    DurationType = item.DurationType,
                    Kms = item.Kms,
                    DealerPrice = item.DealerPrice,
                    CustomerPrice = item.CustomerPrice,
                    DiscountAmount = item.DiscountAmount,
                    Gstpercentage = item.Gstpercentage,
                    PurchaseValidity = item.PurchaseValidity,
                    FromDate = item.FromDate,
                    ToDate = item.ToDate,
                    CreatedBy = item.CreatedBy,
                    CreatedDate = item.CreatedDate,
                    UpdatedBy = item.UpdatedBy,
                    UpdatedDate = item.UpdatedDate

                })
                .Cast<object>()
                .ToList();

                return new PagedResponse<object>
                {
                    Data = viewModelItems,
                    TotalRecords = totalRecords
                };
            }
            catch { throw; }
        }

        async Task<ExtendedBatteryWarranty?> IExtendedBatteryWarrantyRepo.GetSchemeDetailById(int id)
        {
            try
            {
                return await _context.ExtendedBatteryWarranties
                    .Where(x => x.Id == id)
                    .FirstOrDefaultAsync();
            }
            catch { throw; }
        }

        int IExtendedBatteryWarrantyRepo.Insert(ExtendedBatteryWarrantyViewModel extendedBatteryWarrantyViewModel)
        {
            var entity = new ExtendedBatteryWarranty
            {
                //Id = extendedBatteryWarrantyViewModel.Id,
                OemmodelId = extendedBatteryWarrantyViewModel.OemmodelId,
                SchemeName = extendedBatteryWarrantyViewModel.SchemeName,
                RateType = extendedBatteryWarrantyViewModel.RateType,
                Duration = extendedBatteryWarrantyViewModel.Duration,
                DurationType = extendedBatteryWarrantyViewModel.DurationType,
                Kms = extendedBatteryWarrantyViewModel.Kms,
                DealerPrice = extendedBatteryWarrantyViewModel.DealerPrice,
                CustomerPrice = extendedBatteryWarrantyViewModel.CustomerPrice,
                DiscountAmount = extendedBatteryWarrantyViewModel.DiscountAmount,
                Gstpercentage = extendedBatteryWarrantyViewModel.Gstpercentage,
                PurchaseValidity = extendedBatteryWarrantyViewModel.PurchaseValidity,
                FromDate = extendedBatteryWarrantyViewModel.FromDate,
                ToDate = extendedBatteryWarrantyViewModel.ToDate,
                CreatedBy = extendedBatteryWarrantyViewModel.CreatedBy,
                CreatedDate = extendedBatteryWarrantyViewModel.CreatedDate
            };

            _context.ExtendedBatteryWarranties.Add(entity);
            _context.SaveChanges();

            return entity.Id;
        }
        async Task<int> IExtendedBatteryWarrantyRepo.Update(ExtendedBatteryWarrantyViewModel extendedBatteryWarrntyViewModel)
        {
            var entities = new ExtendedBatteryWarranty
            {
                Id = extendedBatteryWarrntyViewModel.Id,
                OemmodelId = extendedBatteryWarrntyViewModel.OemmodelId,
                SchemeName = extendedBatteryWarrntyViewModel.SchemeName,
                RateType = extendedBatteryWarrntyViewModel.RateType,
                Duration = extendedBatteryWarrntyViewModel.Duration,
                DurationType = extendedBatteryWarrntyViewModel.DurationType,
                Kms = extendedBatteryWarrntyViewModel.Kms,
                DealerPrice = extendedBatteryWarrntyViewModel.DealerPrice,
                CustomerPrice = extendedBatteryWarrntyViewModel.CustomerPrice,
                DiscountAmount = extendedBatteryWarrntyViewModel.DiscountAmount,
                Gstpercentage = extendedBatteryWarrntyViewModel.Gstpercentage,
                PurchaseValidity = extendedBatteryWarrntyViewModel.PurchaseValidity,
                FromDate = extendedBatteryWarrntyViewModel.FromDate,
                ToDate = extendedBatteryWarrntyViewModel.ToDate,
                IsActive = extendedBatteryWarrntyViewModel.IsActive,
                CreatedBy = extendedBatteryWarrntyViewModel.CreatedBy,
                CreatedDate = extendedBatteryWarrntyViewModel.CreatedDate,
                UpdatedBy = extendedBatteryWarrntyViewModel.UpdatedBy,
                UpdatedDate = extendedBatteryWarrntyViewModel.UpdatedDate
            };

            _context.ExtendedBatteryWarranties.Update(entities);
            return await _context.SaveChangesAsync();
        }
    }
}
