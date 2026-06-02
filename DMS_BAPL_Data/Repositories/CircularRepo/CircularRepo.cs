using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DMS_BAPL_Data.Repositories.CircularRepo
{
    public partial class CircularRepo : ICircularRepo
    {
        private readonly BapldmsvadContext _context;

        public CircularRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        async Task<int> ICircularRepo.Create(CircularMaster circularMaster)
        {
            try
            {
                await _context.CircularMasters.AddAsync(circularMaster);
                await _context.SaveChangesAsync();

                return circularMaster.Id;
            }
            catch
            {
                throw;
            }
        }

        async Task<bool> ICircularRepo.Delete(int Id)
        {
            try
            {
                var circular =
                    await _context.CircularMasters
                    .FirstOrDefaultAsync(x => x.Id == Id);

                if (circular == null)
                    return false;

                _context.CircularMasters
                    .Remove(circular);
                return await _context.SaveChangesAsync() > 0;
            }
            catch { throw; }
        }

        async Task<IEnumerable<CircularMasterViewModel>> ICircularRepo.Get()
        {
            var result = await (
                from CM in _context.CircularMasters

                join CMA in _context.CircularMasterAttachments
                on CM.Id equals CMA.CircularId into attachmentGroup

                from CMA in attachmentGroup.DefaultIfEmpty()

                select new
                {
                    CM.Id,
                    CM.Title,
                    CM.Description,
                    CM.PublishDate,
                    CM.ExpiryDate,
                    CM.IsActive,
                    CM.CreatedBy,
                    CM.CreatedDate,
                    CM.UpdatedBy,
                    CM.UpdatedDate,

                    FileName = CMA != null ? CMA.FileName : null,
                    ContentType = CMA != null ? CMA.ContentType : null,
                    FileData = CMA != null ? CMA.FileData : null
                }

            ).ToListAsync();

            return result
                .GroupBy(x => x.Id)
                .Select(g =>
                {
                    var first = g.First();

                    return new CircularMasterViewModel
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
                            .Select(f => new CircularFileViewModel
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

        async Task<CircularMasterViewModel?> ICircularRepo.GetById(int Id)
        {
            var result = await (
                from CM in _context.CircularMasters

                where CM.Id == Id

                join CMA in _context.CircularMasterAttachments
                on CM.Id equals CMA.CircularId into attachmentGroup

                from CMA in attachmentGroup.DefaultIfEmpty()

                select new
                {
                    CM.Id,
                    CM.Title,
                    CM.Description,
                    CM.PublishDate,
                    CM.ExpiryDate,
                    CM.IsActive,
                    CM.CreatedBy,
                    CM.CreatedDate,
                    CM.UpdatedBy,
                    CM.UpdatedDate,

                    AttachmentId = CMA != null ? CMA.Id : 0,
                    FileName = CMA != null ? CMA.FileName : null,
                    ContentType = CMA != null ? CMA.ContentType : null,
                    FileData = CMA != null ? CMA.FileData : null
                }

            ).ToListAsync();

            return result
                .GroupBy(x => x.Id)
                .Select(g =>
                {
                    var first = g.First();

                    return new CircularMasterViewModel
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
                            .Select(f => new CircularFileViewModel
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

        async Task<int> ICircularRepo.Update(CircularMaster circularMaster)
        {
            try
            {
                _context.CircularMasters.Update(circularMaster);
                await _context.SaveChangesAsync();

                return circularMaster.Id;
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