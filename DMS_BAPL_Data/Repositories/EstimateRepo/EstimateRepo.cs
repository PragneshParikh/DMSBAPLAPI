using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.EstimateRepo
{
    public class EstimateRepo : IEstimateRepo
    {
        private readonly BapldmsvadContext _context;

        public EstimateRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        // Materializes JobType.Id -> JobTypeName once, then used as an
        // in-memory lookup — same pattern as GetFinancierNameLookupAsync /
        // GetLocationLookupAsync elsewhere in this codebase, to avoid
        // relying on EF Core translating a conditional join correctly.
        private async Task<Dictionary<int, string>> GetJobTypeNameLookupAsync()
        {
            var types = await _context.JobTypes.AsNoTracking().ToListAsync();
            return types
                .Where(t => t.JobTypeName != null)
                .GroupBy(t => t.Id)
                .ToDictionary(g => g.Key, g => g.First().JobTypeName!);
        }

        public async Task<int> CreateAsync(EstimateHeader header)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var details = header.EstimateDetails;
                header.EstimateDetails = new List<EstimateDetail>();

                _context.EstimateHeaders.Add(header);
                await _context.SaveChangesAsync();

                foreach (var d in details)
                {
                    d.EstimateHeaderId = header.Id;
                    _context.EstimateDetails.Add(d);
                }
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return header.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<EstimateHeader?> GetByIdAsync(int id)
        {
            try
            {
                return await _context.EstimateHeaders
                    .Include(x => x.EstimateDetails)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            }
            catch
            {
                throw;
            }
        }

        public async Task<string?> GetJobTypeNameAsync(int? jobTypeId)
        {
            if (!jobTypeId.HasValue) return null;

            try
            {
                return await _context.JobTypes
                    .AsNoTracking()
                    .Where(x => x.Id == jobTypeId.Value)
                    .Select(x => x.JobTypeName)
                    .FirstOrDefaultAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task<EstimatePagedResponse> GetAllAsync(EstimateFilterModel filter)
        {
            try
            {
                var query = _context.EstimateHeaders
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted);

                if (!string.IsNullOrWhiteSpace(filter.DealerCode))
                    query = query.Where(x => x.DealerCode == filter.DealerCode);

                if (!string.IsNullOrWhiteSpace(filter.ChassisNo))
                    query = query.Where(x => x.ChassisNo != null && x.ChassisNo.Contains(filter.ChassisNo));

                if (!string.IsNullOrWhiteSpace(filter.EstimationNo))
                    query = query.Where(x => x.EstimationNo != null && x.EstimationNo.Contains(filter.EstimationNo));

                if (filter.FromDate.HasValue)
                    query = query.Where(x => x.EstimateDate.Date >= filter.FromDate.Value.Date);

                if (filter.ToDate.HasValue)
                    query = query.Where(x => x.EstimateDate.Date <= filter.ToDate.Value.Date);

                var totalRecords = await query.CountAsync();

                var rawRows = await query
                    .OrderBy(x => x.EstimateDate)
                    .ThenBy(x => x.Id)
                    .Skip((filter.PageIndex - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                var jobTypeLookup = await GetJobTypeNameLookupAsync();

                var data = rawRows.Select(x => new EstimateResponseViewModel
                {
                    Id = x.Id,
                    EstimationNo = x.EstimationNo,
                    EstimateDate = x.EstimateDate,
                    ChassisNo = x.ChassisNo,
                    CustomerName = x.CustomerName,
                    CustomerMobile = x.CustomerMobile,
                    CustomerAddress = x.CustomerAddress,
                    CustomerPin = x.CustomerPin,
                    CustomerEmail = x.CustomerEmail,
                    CustomerCity = x.CustomerCity,
                    CustomerState = x.CustomerState,
                    Kms = x.Kms,
                    JobTypeId = x.JobTypeId,
                    JobTypeName = x.JobTypeId.HasValue && jobTypeLookup.TryGetValue(x.JobTypeId.Value, out var jtName)
                        ? jtName : null,
                    DealerCode = x.DealerCode,
                    Status = x.Status,
                    CreatedDate = x.CreatedDate
                }).ToList();

                return new EstimatePagedResponse
                {
                    Data = data,
                    TotalRecords = totalRecords,
                    PageIndex = filter.PageIndex,
                    PageSize = filter.PageSize
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task UpdateAsync(EstimateHeader entity)
        {
            try { 
                await _context.SaveChangesAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var entity = await _context.EstimateHeaders.FindAsync(id);
                if (entity == null) return false;

                entity.IsDeleted = true;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string?> GetLastEstimationNoAsync()
        {
            try
            {
                return await _context.EstimateHeaders
                    .OrderByDescending(x => x.Id)
                    .Select(x => x.EstimationNo)
                    .FirstOrDefaultAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<JobTypeDropdownItem>> GetJobTypesAsync()
        {
            try
            {
                return await _context.JobTypes
                    .AsNoTracking()
                    .Select(x => new JobTypeDropdownItem
                    {
                        Id = x.Id,
                        JobTypeName = x.JobTypeName
                    })
                    .OrderBy(x => x.JobTypeName)
                    .ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<PartSearchResultViewModel>> SearchPartsAsync(string query, int maxResults = 20)
        {
            try
            {
                var take = Math.Clamp(maxResults, 1, 200);
                var q = (query ?? "").Trim();
                var partsQuery = _context.PartWiseLabourMasters.AsNoTracking()
                    .Where(x => x.IsActive != false);

                if (!string.IsNullOrWhiteSpace(q))
                {
                    partsQuery = partsQuery.Where(x =>
                        (x.PartCode != null && x.PartCode.Contains(q)) ||
                        (x.PartDescription != null && x.PartDescription.Contains(q)));
                }

                return await partsQuery
                    .OrderBy(x => x.PartCode)
                    .Take(take)
                    .Select(x => new PartSearchResultViewModel
                    {
                        ItemCode = x.PartCode,
                        ItemDescription = x.PartDescription,
                        Rate = (decimal)x.LabourRate,
                        CgstPercent = (decimal)x.Cgst,
                        SgstPercent = (decimal)x.Sgst,
                        IgstPercent = (decimal)x.Igst
                    })
                    .ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<LabourSearchResultViewModel>> SearchLabourAsync(string query, int maxResults = 20)
        {
            try
            {
                var take = Math.Clamp(maxResults, 1, 200);
                var q = (query ?? "").Trim();

                var labourQuery = _context.LabourMasters.AsNoTracking()
                    .Where(x => x.IsLabourActive != false);

                if (!string.IsNullOrWhiteSpace(q))
                {
                    labourQuery = labourQuery.Where(x =>
                        (x.LabourCode != null && x.LabourCode.Contains(q)) ||
                        (x.LabourDescription != null && x.LabourDescription.Contains(q)));
                }

                return await labourQuery
                    .OrderBy(x => x.LabourCode)
                    .Take(take)
                    .Select(x => new LabourSearchResultViewModel
                    {
                        LabourCode = x.LabourCode,
                        LabourDescription = x.LabourDescription,
                        Rate = (decimal)x.LabourRate,
                        CgstPercent = (decimal)x.Cgst,
                        SgstPercent = (decimal)x.Sgst,
                        IgstPercent = (decimal)x.Igst
                    })
                    .ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<string>> GetEstimationNumbersAsync(string? dealerCode)
        {
            try
            {
                var query = _context.EstimateHeaders
                    .AsNoTracking()
                    .Where(x => !x.IsDeleted && x.EstimationNo != null);

                if (!string.IsNullOrWhiteSpace(dealerCode))
                    query = query.Where(x => x.DealerCode == dealerCode);

                return await query
                    .OrderByDescending(x => x.Id)
                    .Select(x => x.EstimationNo!)
                    .ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task<EstimatePrintViewModel?> GetEstimatePrintDataAsync(int id)
        {
            try
            {
                var header = await _context.EstimateHeaders
                    .Include(x => x.EstimateDetails)
                    .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

                if (header == null) return null;

                var dealer = await _context.DealerMasters
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Dealercode == header.DealerCode);

                var jobTypeName = await GetJobTypeNameAsync(header.JobTypeId);

                var lines = header.EstimateDetails.Select(d => new EstimatePrintLineViewModel
                {
                    ItemType = d.ItemType,
                    ItemCode = d.ItemCode,
                    ItemDescription = d.ItemDescription,
                    Qty = d.Qty,
                    Rate = d.Rate,
                    DiscountPercent = d.DiscountPercent,
                    CgstPercent = d.CgstPercent,
                    CgstAmount = d.CgstAmount,
                    SgstPercent = d.SgstPercent,
                    SgstAmount = d.SgstAmount,
                    IgstPercent = d.IgstPercent,
                    IgstAmount = d.IgstAmount,
                    Amount = d.Amount
                }).ToList();

                var parts = lines.Where(x => x.ItemType == "Part").ToList();
                var labour = lines.Where(x => x.ItemType == "Labour").ToList();

                return new EstimatePrintViewModel
                {
                    DealerCode = header.DealerCode,
                    DealerName = dealer?.Compname,
                    DealerAddress = dealer != null
                        ? $"{dealer.Adress1} {dealer.Adress2}, {dealer.City}, {dealer.State} - {dealer.Pin}".Trim()
                        : null,
                    DealerPhone = string.IsNullOrWhiteSpace(dealer?.PhoneOff) ? dealer?.Mobile : dealer?.PhoneOff,
                    DealerEmail = dealer?.Email,
                    DealerGstin = dealer?.CompgstinNo,
                    DealerPan = dealer?.Pan,
                    DealerTradeCertNo = dealer?.TradCert,

                    EstimationNo = header.EstimationNo,
                    EstimateDate = header.EstimateDate,
                    ChassisNo = header.ChassisNo,
                    Kms = header.Kms,
                    JobTypeName = jobTypeName,

                    CustomerName = header.CustomerName,
                    CustomerMobile = header.CustomerMobile,
                    CustomerAddress = header.CustomerAddress,
                    CustomerCity = header.CustomerCity,
                    CustomerState = header.CustomerState,
                    CustomerPin = header.CustomerPin,
                    CustomerEmail = header.CustomerEmail,

                    Parts = parts,
                    Labour = labour,
                    TotalPartsAmount = parts.Sum(x => x.Amount),
                    TotalLabourAmount = labour.Sum(x => x.Amount),
                    GrandTotal = lines.Sum(x => x.Amount)
                };
            }
            catch
            {
                throw;
            }
        }
    }
}