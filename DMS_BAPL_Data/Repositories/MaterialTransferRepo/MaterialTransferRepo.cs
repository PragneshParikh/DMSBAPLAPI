using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
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
                    IssueType = k.IssueType,
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
                                 MT.CreatedBy,
                                 MT.CreatedDate,
                                 MT.UpdatedBy,
                                 MT.UpdatedDate,
                                 IM.Itemname,
                                 IM.Itemcode,
                                 IM.Itemdesc,
                                 IM.Dlrprice,

                                 Amount = MT.ItemRate * MT.Quantity,
                             };

                return await result.ToListAsync();
            }
            catch { throw; }
        }

    }
}
