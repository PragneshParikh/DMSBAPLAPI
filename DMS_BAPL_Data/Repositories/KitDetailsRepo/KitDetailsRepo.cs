using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.LedgerMasterRepo;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.KitDetailsRepo
{
    public partial class KitDetailsRepo : IKitDetailsRepo
    {
        private readonly BapldmsvadContext _context;
        public KitDetailsRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<object>> GetKitDetailsByHeaderId(int headerId)
        {
            try
            {
                var kitDetails = await _context.KitDetails
                    .Where(x => x.KitHeaderId == headerId)
                    .Include(x => x.Item) // Include Item navigation property
                    .Select(x => new
                    {
                        x.Id,
                        x.KitHeaderId,
                        x.ItemId,
                        x.Quantity,
                        x.CreatedBy,
                        x.CreatedDate,
                        x.UpdatedBy,
                        x.UpdatedDate,

                        ItemName = x.Item.Itemname,
                        ItemDescription = x.Item.Itemdesc,
                    })
                    .ToListAsync();

                return kitDetails;
            }
            catch { throw; }
        }
        public async Task<PagedResponse<object>> GetKitDetailsByPaged(int pageIndex, int pageSize, int headerId)
        {
            try
            {
                var query = _context.KitDetails
                    .Where(x => x.KitHeaderId == headerId)
                    .AsNoTracking();

                int totalRecords = await query.CountAsync();

                var kitDetails = await query
                    .Where(x => x.KitHeaderId == headerId)
                    .Include(x => x.Item)
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize)
                    .Select(x => new
                    {
                        x.Id,
                        x.KitHeaderId,
                        x.ItemId,
                        x.Quantity,
                        x.CreatedBy,
                        x.CreatedDate,
                        x.UpdatedBy,
                        x.UpdatedDate,

                        ItemName = x.Item.Itemname,
                        ItemDescription = x.Item.Itemdesc,
                    })
                    .Cast<object>()
                    .ToListAsync();

                return new PagedResponse<object>
                {
                    Data = kitDetails,
                    TotalRecords = totalRecords
                };
            }
            catch { throw; }
        }
        public async Task<int> InsertKitDetails(List<KitDetailsViewModel> kitDetailsViewModels)
        {
            try
            {
                var entities = kitDetailsViewModels.Select(k => new KitDetail
                {
                    KitHeaderId = k.KitHeaderId,
                    ItemId = k.ItemId,
                    Quantity = k.Quantity,
                    CreatedBy = k.CreatedBy,
                    CreatedDate = k.CreatedDate,
                    UpdatedBy = k.UpdatedBy,
                    UpdatedDate = k.UpdatedDate
                }).ToList();

                _context.KitDetails.AddRange(entities);
                return await _context.SaveChangesAsync();
            }
            catch { throw; }
        }
        async Task<bool> IKitDetailsRepo.UpdateKitDetails(KitDetail kitDetail)
        {
            try
            {
                _context.KitDetails.Update(kitDetail);
                return await _context.SaveChangesAsync() > 0;
            }
            catch { throw; }
        }

        //async Task<IEnumerable<object>> IKitDetailsRepo.GetKitDetailsWithItemByHeaderId(int headerId)
        //{
        //    try
        //    {
        //        var kitDetails = await _context.KitDetails
        //            .Where(x => x.KitHeaderId == headerId)
        //            .Include(x => x.Item)
        //            .Select(x => new
        //            {
        //                x.Id,
        //                x.KitHeaderId,
        //                x.ItemId,
        //                x.Quantity,
        //                x.CreatedBy,
        //                x.CreatedDate,
        //                x.UpdatedBy,
        //                x.UpdatedDate,

        //                ItemName = x.Item.Itemname,
        //                itemCode = x.Item.Itemcode,
        //                ItemDescription = x.Item.Itemdesc,
        //                ItemPrice = x.Item.Dlrprice,
        //            })
        //            .ToListAsync();

        //        return kitDetails;
        //    }
        //    catch { throw; }

        //}

        public async Task<IEnumerable<object>> GetKitDetailsWithItemByHeaderAndLocation(
            int headerId,
            string dealerLocation,
            string companyLocation
            )
        {
            try
            {
                // State Flag
                string stateFlag = string.Equals(
                    dealerLocation,
                    companyLocation,
                    StringComparison.OrdinalIgnoreCase)
                    ? "S"
                    : "O";

                // Get Kit Details
                var kitDetails = await _context.KitDetails
                    .Where(x => x.KitHeaderId == headerId)
                    .Include(x => x.Item)
                    .ToListAsync();

                var result = new List<object>();

                foreach (var kit in kitDetails)
                {
                    var hsnTax = await _context.HsnwiseTaxCodes
                        .Where(h =>
                            h.Hsncode == kit.Item.Hsncode &&
                            h.StateFlag == stateFlag)
                        .OrderByDescending(h => h.EffectiveDate)
                        .FirstOrDefaultAsync();

                    string ataxCode = hsnTax?.AtaxCode ?? "";

                    var taxDetails = new List<TaxDetailViewModel>();

                    if (!string.IsNullOrEmpty(ataxCode))
                    {
                        taxDetails = await _context.AggregateTaxCodes
                            .Where(a => a.AtaxCode == ataxCode)
                            .OrderBy(a => a.SrNo)
                            .Select(a => new TaxDetailViewModel
                            {
                                SrNo = a.SrNo,
                                TaxCode = a.TaxCode,
                                TaxRate = a.TaxRate
                            })
                            .ToListAsync();
                    }

                    result.Add(new
                    {
                        kit.Id,
                        kit.KitHeaderId,
                        kit.ItemId,
                        kit.Quantity,
                        kit.CreatedBy,
                        kit.CreatedDate,
                        kit.UpdatedBy,
                        kit.UpdatedDate,

                        ItemName = kit.Item.Itemname,
                        ItemCode = kit.Item.Itemcode,
                        ItemDescription = kit.Item.Itemdesc,
                        ItemPrice = kit.Item.Dlrprice,
                        HsnCode = kit.Item.Hsncode,

                        TaxDetails = taxDetails
                    });
                }

                return result;
            }
            catch
            {
                throw;
            }
        }

    }
}
