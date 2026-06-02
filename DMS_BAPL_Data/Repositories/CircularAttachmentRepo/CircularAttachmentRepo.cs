using DMS_BAPL_Data.DBModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.CircularAttachmentRepo
{
    public partial class CircularAttachmentRepo : ICircularAttachmentRepo
    {
        private readonly BapldmsvadContext _context;
        public CircularAttachmentRepo(BapldmsvadContext context)
        {
            _context = context;
        }
        async Task<bool> ICircularAttachmentRepo.Delete(List<int> attachmentIds)
        {
            var attachments = await _context.CircularMasterAttachments
                .Where(x => attachmentIds.Contains(x.Id))
                .ToListAsync();

            _context.CircularMasterAttachments.RemoveRange(attachments);

            return await _context.SaveChangesAsync() > 0;
        }

        async Task<IEnumerable<CircularMasterAttachment>> ICircularAttachmentRepo.Get()
        {
            throw new NotImplementedException();
        }

        async Task<CircularMasterAttachment> ICircularAttachmentRepo.GetById(int Id)
        {
            throw new NotImplementedException();
        }

        async Task<IEnumerable<CircularMasterAttachment>> ICircularAttachmentRepo.GetByCircularId(int Id)
        {
            throw new NotImplementedException();
        }

        async Task<bool> ICircularAttachmentRepo.Insert(IEnumerable<CircularMasterAttachment> circularMasterAttachment)
        {
            try
            {
                await _context.CircularMasterAttachments
                     .AddRangeAsync(circularMasterAttachment);

                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                throw;
            }
        }
    }
}
