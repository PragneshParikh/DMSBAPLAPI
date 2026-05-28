using DMS_BAPL_Data.DBModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.NewsBulletinAttachmentRepo
{
    public partial class NewsBulletinAttachmentRepo : INewsBulletinAttachmentRepo
    {
        private readonly BapldmsvadContext _context;
        public NewsBulletinAttachmentRepo(BapldmsvadContext context)
        {
            _context = context;
        }
        async Task<bool> INewsBulletinAttachmentRepo.Delete(List<int> attachmentIds)
        {
            var attachments = await _context.NewsBulletinMasterAttachments
                .Where(x => attachmentIds.Contains(x.Id))
                .ToListAsync();

            _context.NewsBulletinMasterAttachments.RemoveRange(attachments);

            return await _context.SaveChangesAsync() > 0;
        }

        async Task<IEnumerable<NewsBulletinMasterAttachment>> INewsBulletinAttachmentRepo.Get()
        {
            throw new NotImplementedException();
        }

        async Task<NewsBulletinMasterAttachment> INewsBulletinAttachmentRepo.GetById(int Id)
        {
            throw new NotImplementedException();
        }

        async Task<IEnumerable<NewsBulletinMasterAttachment>> INewsBulletinAttachmentRepo.GetByNewsBulletinId(int Id)
        {
            throw new NotImplementedException();
        }

        async Task<bool> INewsBulletinAttachmentRepo.Insert(IEnumerable<NewsBulletinMasterAttachment> newsBulletinMasterAttachment)
        {
            try
            {
                await _context.NewsBulletinMasterAttachments
                     .AddRangeAsync(newsBulletinMasterAttachment);

                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                throw;
            }
        }
    }
}
