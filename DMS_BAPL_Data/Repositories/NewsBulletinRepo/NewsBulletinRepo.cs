using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DMS_BAPL_Data.Repositories.NewsBulletinRepo
{
    public partial class NewsBulletinRepo : INewsBulletinRepo
    {
        private readonly BapldmsvadContext _context;

        public NewsBulletinRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        async Task<int> INewsBulletinRepo.Create(NewsBulletinMaster newsBulletinMasters)
        {
            try
            {
                await _context.NewsBulletinMasters.AddAsync(newsBulletinMasters);

                await _context.SaveChangesAsync();

                return newsBulletinMasters.Id;
            }
            catch
            {
                throw;
            }
        }

        async Task<bool> INewsBulletinRepo.Delete(int Id)
        {
            try
            {
                var newsBulletin =
                    await _context.NewsBulletinMasters
                    .FirstOrDefaultAsync(x => x.Id == Id);

                if (newsBulletin == null)
                    return false;

                _context.NewsBulletinMasters
                    .Remove(newsBulletin);

                return await _context.SaveChangesAsync() > 0;
            }
            catch { throw; }
        }

        async Task<IEnumerable<NewsBulletinMasterViewModel>> INewsBulletinRepo.Get()
        {
            var result = await (
                from NB in _context.NewsBulletinMasters

                join NBA in _context.NewsBulletinMasterAttachments
                on NB.Id equals NBA.NewsBulletinId into attachmentGroup

                from NBA in attachmentGroup.DefaultIfEmpty()

                select new
                {
                    NB.Id,
                    NB.Title,
                    NB.Description,
                    NB.PublishDate,
                    NB.ExpiryDate,
                    NB.IsActive,
                    NB.CreatedBy,
                    NB.CreatedDate,
                    NB.UpdatedBy,
                    NB.UpdatedDate,

                    FileName = NBA != null ? NBA.FileName : null,
                    ContentType = NBA != null ? NBA.ContentType : null,
                    FileData = NBA != null ? NBA.FileData : null
                }

            ).ToListAsync();

            return result
                .GroupBy(x => x.Id)
                .Select(g =>
                {
                    var first = g.First();

                    return new NewsBulletinMasterViewModel
                    {
                        Id = first.Id,
                        Title = first.Title,
                        Description = first.Description,
                        PublishDate = first.PublishDate,
                        ExpiryDate = first.ExpiryDate,
                        IsActive = first.IsActive,
                        CreatedBy = first.CreatedBy,
                        CreatedDate = first.CreatedDate,
                        UpdatedBy = first.UpdatedBy,
                        UpdatedDate = first.UpdatedDate,

                        Files = g
                            .Where(f => f.FileName != null)
                            .Select(f => new NewsBulletinFileViewModel
                            {
                                FileName = f.FileName!,
                                ContentType = f.ContentType!,
                                FileData = f.FileData
                            })
                            .ToList()
                    };
                })
                .ToList();
        }

        async Task<NewsBulletinMasterViewModel?> INewsBulletinRepo.GetById(int Id)
        {
            var result = await (
                from NB in _context.NewsBulletinMasters

                where NB.Id == Id

                join NBA in _context.NewsBulletinMasterAttachments
                on NB.Id equals NBA.NewsBulletinId into attachmentGroup

                from NBA in attachmentGroup.DefaultIfEmpty()

                select new
                {
                    NB.Id,
                    NB.Title,
                    NB.Description,
                    NB.PublishDate,
                    NB.ExpiryDate,
                    NB.IsActive,
                    NB.CreatedBy,
                    NB.CreatedDate,
                    NB.UpdatedBy,
                    NB.UpdatedDate,

                    AttachmentId = NBA != null ? NBA.Id : 0,
                    FileName = NBA != null ? NBA.FileName : null,
                    ContentType = NBA != null ? NBA.ContentType : null,
                    FileData = NBA != null ? NBA.FileData : null
                }

            ).ToListAsync();

            return result
                .GroupBy(x => x.Id)
                .Select(g =>
                {
                    var first = g.First();

                    return new NewsBulletinMasterViewModel
                    {
                        Id = first.Id,
                        Title = first.Title,
                        Description = first.Description,
                        PublishDate = first.PublishDate,
                        ExpiryDate = first.ExpiryDate,
                        IsActive = first.IsActive,
                        CreatedBy = first.CreatedBy,
                        CreatedDate = first.CreatedDate,
                        UpdatedBy = first.UpdatedBy,
                        UpdatedDate = first.UpdatedDate,

                        Files = g
                            .Where(f => f.FileName != null)
                            .Select(f => new NewsBulletinFileViewModel
                            {
                                AttachmentId = f.AttachmentId,
                                FileName = f.FileName!,
                                ContentType = f.ContentType!,
                                FileData = f.FileData
                            })
                            .ToList()
                    };
                })
                .FirstOrDefault();
        }

        async Task<int> INewsBulletinRepo.Update(NewsBulletinMaster newsBulletinMasters)
        {
            try
            {
                _context.NewsBulletinMasters.Update(newsBulletinMasters);

                await _context.SaveChangesAsync();

                return newsBulletinMasters.Id;
            }
            catch
            {
                throw;
            }
        }

        //async Task<NewsBulletinMasterViewModel?> INewsBulletinRepo.GetByDate(DateTime date)
        //{
        //    return await _context.NewsBulletinMasters
        //        .Where(x => x.PublishDate.Date == date.Date)
        //        .GroupBy(x => new
        //        {
        //            x.Id,
        //            x.Title,
        //            x.Description,
        //            x.PublishDate,
        //            x.ExpiryDate,
        //            x.IsActive,
        //            x.CreatedBy,
        //            x.CreatedDate,
        //            x.UpdatedBy,
        //            x.UpdatedDate
        //        })

        //        .Select(g => new NewsBulletinMasterViewModel
        //        {
        //            Id = g.Key.Id,
        //            Title = g.Key.Title,
        //            Description = g.Key.Description,
        //            PublishDate = g.Key.PublishDate,
        //            ExpiryDate = g.Key.ExpiryDate,
        //            IsActive = g.Key.IsActive,
        //            CreatedBy = g.Key.CreatedBy,
        //            CreatedDate = g.Key.CreatedDate,
        //            UpdatedBy = g.Key.UpdatedBy,
        //            UpdatedDate = g.Key.UpdatedDate,
        //            //Files = g.Select(x => new NewsBulletinFileViewModel
        //            //{
        //            //    FileName = x.FileName,
        //            //    ContentType = x.ContentType,
        //            //    FileData = x.FileData
        //            //}).ToList()
        //        })

        //        .FirstOrDefaultAsync();
        //}
    }
}