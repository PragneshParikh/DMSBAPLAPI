using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.InvoiceDispatchRepo
{
    public class InvoiceDispatchRepo : IInvoiceDispatchRepo
    {
        private readonly BapldmsvadContext _context;

        // Confirmed from real data (dbo.InvoiceHeader): InvoiceType is
        // "Invoice" / "Proforma Invoice" (draft vs finalized) — NOT a
        // Part/Vehicle split. ServiceType is what actually distinguishes
        // them: "Counter Bill" (parts) vs "Vehicle Sale Bill" (vehicles).
        private const string PartServiceType = "Counter Bill";
        private const string VehicleServiceType = "Vehicle Sale Bill";

        public InvoiceDispatchRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<PartDispatchListViewModel>> GetPartDispatchList(InvoiceDispatchViewModel filter)
        {
            try
            {
                var query = from h in _context.InvoiceHeaders.AsNoTracking()
                            where h.ServiceType == PartServiceType

                            from d in h.InvoiceDetails

                            join dm in _context.DealerMasters
                                on h.DealerCode equals dm.Dealercode into dmGroup
                            from dm in dmGroup.DefaultIfEmpty()

                            select new
                            {
                                Header = h,
                                Detail = d,
                                dm.Compname
                            };

                if (!string.IsNullOrWhiteSpace(filter.DealerCode))
                    query = query.Where(x => x.Header.DealerCode == filter.DealerCode);

                if (filter.FromDate.HasValue)
                    query = query.Where(x => x.Header.CreatedDate >= filter.FromDate.Value.Date);

                if (filter.ToDate.HasValue)
                    query = query.Where(x => x.Header.CreatedDate <= filter.ToDate.Value.Date);

                var totalCount = await query.CountAsync();

                var rows = await query
                    .OrderByDescending(x => x.Header.CreatedDate)
                    .ThenByDescending(x => x.Detail.Id)
                    .Skip((filter.PageIndex - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(x => new PartDispatchListViewModel
                    {
                        Id = x.Detail.Id,
                        InvoiceHeaderId = x.Header.Id,
                        InvoiceNo = x.Header.InvoiceNo ?? x.Header.DocumentNo ?? string.Empty,
                        InvoiceDate = x.Header.CreatedDate ?? default(DateTime),
                        Description = x.Detail.Description,
                        Quantity = x.Detail.Quantity ?? 0m,
                        Rate = x.Detail.Rate ?? 0m,
                        Amount = x.Detail.Amount ?? 0m,
                        TaxPercent = x.Detail.TaxPercent ?? 0m,
                        DealerCode = x.Header.DealerCode,
                        DealerName = x.Compname,
                        Status = x.Header.Status ?? "-",
                        CreatedDate = x.Header.CreatedDate ?? default(DateTime),
                        CreatedBy = x.Header.CreatedBy
                    })
                    .ToListAsync();

                for (int i = 0; i < rows.Count; i++)
                    rows[i].SrNo = ((filter.PageIndex - 1) * filter.PageSize) + i + 1;

                return new PagedResult<PartDispatchListViewModel>
                {
                    TotalCount = totalCount,
                    PageIndex = filter.PageIndex,
                    PageSize = filter.PageSize,
                    Data = rows
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<PagedResult<VehicleDispatchListViewModel>> GetVehicleDispatchList(InvoiceDispatchViewModel filter)
        {
            try
            {
                var query = from h in _context.InvoiceHeaders.AsNoTracking()
                            where h.ServiceType == VehicleServiceType

                            from d in h.InvoiceDetails

                            join dm in _context.DealerMasters
                                on h.DealerCode equals dm.Dealercode into dmGroup
                            from dm in dmGroup.DefaultIfEmpty()

                            select new
                            {
                                Header = h,
                                Detail = d,
                                dm.Compname
                            };

                if (!string.IsNullOrWhiteSpace(filter.DealerCode))
                    query = query.Where(x => x.Header.DealerCode == filter.DealerCode);

                if (filter.FromDate.HasValue)
                    query = query.Where(x => x.Header.CreatedDate >= filter.FromDate.Value.Date);

                if (filter.ToDate.HasValue)
                    query = query.Where(x => x.Header.CreatedDate <= filter.ToDate.Value.Date);

                var totalCount = await query.CountAsync();

                var rows = await query
                    .OrderByDescending(x => x.Header.CreatedDate)
                    .ThenByDescending(x => x.Detail.Id)
                    .Skip((filter.PageIndex - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(x => new VehicleDispatchListViewModel
                    {
                        Id = x.Detail.Id,
                        InvoiceHeaderId = x.Header.Id,
                        InvoiceNo = x.Header.InvoiceNo ?? x.Header.DocumentNo ?? string.Empty,
                        InvoiceDate = x.Header.CreatedDate ?? default(DateTime),
                        Description = x.Detail.Description,
                        Quantity = x.Detail.Quantity ?? 0m,
                        Rate = x.Detail.Rate ?? 0m,
                        Amount = x.Detail.Amount ?? 0m,
                        TaxPercent = x.Detail.TaxPercent ?? 0m,
                        DealerCode = x.Header.DealerCode,
                        DealerName = x.Compname,
                        Status = x.Header.Status ?? "-",
                        CreatedDate = x.Header.CreatedDate ?? default(DateTime),
                        CreatedBy = x.Header.CreatedBy
                    })
                    .ToListAsync();

                for (int i = 0; i < rows.Count; i++)
                    rows[i].SrNo = ((filter.PageIndex - 1) * filter.PageSize) + i + 1;

                return new PagedResult<VehicleDispatchListViewModel>
                {
                    TotalCount = totalCount,
                    PageIndex = filter.PageIndex,
                    PageSize = filter.PageSize,
                    Data = rows
                };
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}