using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.MaterialTransferRepo
{
    public partial class MaterialTransferRepo : IMaterialTransferRepo
    {
        private readonly BapldmsvadContext _context;

        public MaterialTransferRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        async Task<object> IMaterialTransferRepo.Get()
        {
            return await _context.MaterialTransfers
                .ToListAsync();
        }

        async Task<string> IMaterialTransferRepo.GetIssueIdAsync()
        {
            try
            {
                var issueId = (await _context.MaterialTransfers
                .MaxAsync(x => (int?)x.Id) ?? 0) + 1;

                return issueId.ToString();
            }
            catch { throw; }
        }

        async Task<int> IMaterialTransferRepo.InsertMaterials(List<MaterialTransferViewModel> materialTransferViewModels)
        {
            try
            {
                var entities = materialTransferViewModels.Select(k => new MaterialTransfer
                {
                    JobId = k.JobId,
                    ItemId = k.ItemId,
                    RackNo = k.RackNo,
                    Bin = k.Bin,
                    Technician = k.Technician,
                    Ffi = k.Ffi,
                    Quantity = k.Quantity,
                    ItemRate = k.ItemRate,
                    SerialNo = k.SerialNo,
                    Remarks = k.Remarks,
                    ItemReceived = k.ItemReceived,
                    ValidDays = k.ValidDays,
                    IssueType = k.IssueType,
                    MaterialPrefix = k.MaterialPrefix,
                    MaterialIssueNumber = k.MaterialIssueNumber,
                    CreatedBy = k.CreatedBy,
                    CreatedDate = k.CreatedDate,
                    UpdatedBy = k.UpdatedBy,
                    UpdatedDate = k.UpdatedDate
                }).ToList();

                await _context.MaterialTransfers.AddRangeAsync(entities);
                var result = await _context.SaveChangesAsync();

                return result;

            }
            catch { throw; }
        }

        async Task<int> IMaterialTransferRepo.DeleteMaterials(List<int> ids)
        {
            try
            {
                var result = await _context.MaterialTransfers
                    .Where(x => ids.Contains(x.Id))
                    .ExecuteDeleteAsync();

                return result;
            }
            catch { throw; }
        }

        async Task<int> IMaterialTransferRepo.UpdateMaterialDetails(List<MaterialTransferViewModel> materialTransferViewModels)
        {
            try
            {
                var entities = materialTransferViewModels.Select(k => new MaterialTransfer
                {
                    Id = k.Id,
                    ItemId = k.ItemId,
                    JobId = k.JobId,
                    RackNo = k.RackNo,
                    Bin = k.Bin,
                    Technician = k.Technician,
                    Ffi = k.Ffi,
                    Quantity = k.Quantity,
                    ItemRate = k.ItemRate,
                    SerialNo = k.SerialNo,
                    Remarks = k.Remarks,
                    ItemReceived = k.ItemReceived,
                    ValidDays = k.ValidDays,
                    //IssueType = k.IssueType,
                    CreatedBy = k.CreatedBy,
                    CreatedDate = k.CreatedDate,
                    UpdatedBy = k.UpdatedBy,
                    UpdatedDate = k.UpdatedDate
                }).ToList();

                _context.MaterialTransfers.UpdateRange(entities);
                return await _context.SaveChangesAsync();
            }
            catch { throw; }
        }

        async Task<IEnumerable<object>> IMaterialTransferRepo.GetMeterialByJobId(int jobId)
        {
            try
            {
                var result = from MT in _context.MaterialTransfers
                             join IM in _context.ItemMasters
                             on MT.ItemId equals IM.Id
                             where MT.JobId == jobId
                             select new
                             {
                                 MT.Id,
                                 MT.ItemId,
                                 MT.JobId,
                                 MT.RackNo,
                                 MT.Bin,
                                 MT.Technician,
                                 MT.Ffi,
                                 MT.Quantity,
                                 MT.ItemRate,
                                 MT.SerialNo,
                                 MT.Remarks,
                                 MT.ItemReceived,
                                 MT.ValidDays,
                                 MT.IssueType,
                                 MT.MaterialPrefix,
                                 MT.MaterialIssueNumber,
                                 MT.CreatedBy,
                                 MT.CreatedDate,
                                 MT.UpdatedBy,
                                 MT.UpdatedDate,
                                 IM.Itemname,
                                 IM.Itemcode,
                                 IM.Itemdesc,
                                 IM.Dlrprice,
                                 IM.Custprice,

                                 Amount = MT.ItemRate * MT.Quantity,
                             };

                return await result.ToListAsync();
            }
            catch { throw; }
        }

        async Task<PagedResponse<object>> IMaterialTransferRepo.GetMaterialTransferDetailsByDealer(
            string? searchTerm,
            string dealerCode,
            int pageIndex,
            int pageSize)
        {
            try
            {
                var materialTransferGroup = _context.MaterialTransfers
                    .GroupBy(x => x.JobId)
                    .Select(x => new
                    {
                        JobId = x.Key,
                        CreatedDate = x.Max(y => y.CreatedDate)
                    });

                var query =
                    from MT in materialTransferGroup

                    join JH in _context.JobCardHeaders
                        on MT.JobId equals JH.Id

                    join JC in _context.JobCardCustomers
                        on JH.Id equals JC.JobCardHeaderId

                    join U in _context.AspNetUsers
                        on JH.CreatedBy equals U.Id into userGroup
                    from U in userGroup.DefaultIfEmpty()

                    join UM in _context.AspNetUsers
                        on JH.UpdateBy equals UM.Id into userModGroup
                    from UM in userModGroup.DefaultIfEmpty()

                    where JH.DealerCode == dealerCode

                    select new
                    {
                        JH.Id,
                        JH.InvoiceNo,
                        JH.Chassisno,
                        JH.JobinDate,
                        JH.JobNo,
                        JH.Serviceloc,

                        PreparedBy = U != null ? U.UserName : null,
                        ModifiedBy = UM != null ? UM.UserName : null,

                        JC.CustomerName,
                        JC.RegisterNo,

                        MT.CreatedDate
                    };

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.Trim();

                    query = query.Where(x =>
                        (x.InvoiceNo != null && x.InvoiceNo.Contains(searchTerm)) ||
                        (x.Chassisno != null && x.Chassisno.Contains(searchTerm)) ||
                        (x.JobNo != null && x.JobNo.ToString().Contains(searchTerm)) ||
                        (x.CustomerName != null && x.CustomerName.Contains(searchTerm)) ||
                        (x.RegisterNo != null && x.RegisterNo.Contains(searchTerm))
                    );
                }

                int totalRecords = await query.CountAsync();

                var result = await query
                    .OrderByDescending(x => x.CreatedDate)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var data = result.Select((x, index) => new
                {
                    SrNo = ((pageIndex - 1) * pageSize) + index + 1,
                    x.Id,
                    x.InvoiceNo,
                    x.Chassisno,
                    x.JobinDate,
                    x.JobNo,
                    x.Serviceloc,
                    x.PreparedBy,
                    x.ModifiedBy,
                    x.CustomerName,
                    x.RegisterNo
                }).Cast<object>().ToList();

                return new PagedResponse<object>
                {
                    Data = data,
                    TotalRecords = totalRecords
                };
            }
            catch
            {
                throw;
            }
        }

        async Task<List<MaterialTransferExcelViewModel>> IMaterialTransferRepo.GetMaterialTransferExcelByDealer(string? dealerCode)
        {
            try
            {
                var materialTransferGroup = _context.MaterialTransfers
                    .GroupBy(x => x.JobId)
                    .Select(x => new
                    {
                        JobId = x.Key,
                        CreatedDate = x.Max(y => y.CreatedDate)
                    });

                var query =
                    from MT in materialTransferGroup

                    join JH in _context.JobCardHeaders
                        on MT.JobId equals JH.Id

                    join JC in _context.JobCardCustomers
                        on JH.Id equals JC.JobCardHeaderId

                    join U in _context.AspNetUsers
                        on JH.CreatedBy equals U.Id into userGroup
                    from U in userGroup.DefaultIfEmpty()

                    join UM in _context.AspNetUsers
                        on JH.UpdateBy equals UM.Id into userModGroup
                    from UM in userModGroup.DefaultIfEmpty()

                    where string.IsNullOrEmpty(dealerCode)
                        || JH.DealerCode == dealerCode

                    select new
                    {
                        JH.InvoiceNo,
                        JH.Chassisno,
                        JH.JobinDate,
                        JH.JobNo,
                        JH.Serviceloc,
                        JH.DealerCode,

                        JC.CustomerName,
                        JC.RegisterNo,

                        MT.CreatedDate
                    };

                var result = await query
                    .OrderByDescending(x => x.CreatedDate)
                    .ToListAsync();

                var data = result.Select(x => new MaterialTransferExcelViewModel
                {
                    InvoiceNo = x.InvoiceNo,
                    Chassisno = x.Chassisno,
                    JobinDate = x.JobinDate,
                    JobNo = x.JobNo,
                    DealerCode = x.DealerCode,
                    Serviceloc = x.Serviceloc,
                    CustomerName = x.CustomerName,
                    RegisterNo = x.RegisterNo
                }).ToList();

                return data;
            }
            catch
            {
                throw;
            }
        }

    }
}
