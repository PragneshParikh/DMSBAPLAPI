using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.LedgerMasterRepo;
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

    }
}
