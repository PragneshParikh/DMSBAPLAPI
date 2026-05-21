using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.NewsBulletinRepo;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office.CoverPageProps;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.NewsBulletinService
{
    public partial class NewsBulletinService : INewsBulletinService
    {
        private readonly INewsBulletinRepo _newsBulletinRepo;
        private readonly BapldmsvadContext _context;

        public NewsBulletinService(INewsBulletinRepo newsBulletinRepo, BapldmsvadContext context)
        {
            _newsBulletinRepo = newsBulletinRepo;
            _context = context;
        }

        async Task<int> INewsBulletinService.Create(NewsBulletinMasterViewModel newsBulletinMasterViewModel)
        {
            var newsBulletin = new NewsBulletinMaster
            {
                Title = newsBulletinMasterViewModel.Title,
                Description = newsBulletinMasterViewModel.Description,
                PublishDate = newsBulletinMasterViewModel.PublishDate,
                ExpiryDate = newsBulletinMasterViewModel.ExpiryDate,
                IsActive = newsBulletinMasterViewModel.IsActive,
                CreatedBy = newsBulletinMasterViewModel.CreatedBy,
                CreatedDate = newsBulletinMasterViewModel.CreatedDate,
            };

            return await _newsBulletinRepo.Create(newsBulletin);
        }

        Task<bool> INewsBulletinService.Delete(int Id) => _newsBulletinRepo.Delete(Id);

        async Task<object> INewsBulletinService.Get()
        {
            IEnumerable<NewsBulletinMasterViewModel> newsBulletinList =
                await _newsBulletinRepo.Get();

            var folderStructure = newsBulletinList
                .OrderBy(x => x.PublishDate)

                // YEAR
                .GroupBy(x => x.PublishDate.Year)

                .ToDictionary(

                    yearGroup => yearGroup.Key.ToString(),

                    // MONTH
                    yearGroup => yearGroup
                    .GroupBy(x => x.PublishDate.ToString("MMMM"))

                    .ToDictionary(

                        monthGroup => monthGroup.Key,

                        // DATE
                        monthGroup => monthGroup
                        .GroupBy(x => x.PublishDate.ToString("dd-MM-yyyy"))

                        .ToDictionary(

                            dateGroup => dateGroup.Key,

                            // NEWS
                            dateGroup => dateGroup
                            .Select(x => new
                            {
                                x.Id,
                                x.Title,
                                x.Description,
                                x.PublishDate,
                                x.Files
                            })
                            .ToList()
                        )
                    )
                );

            return folderStructure;
        }

        Task<NewsBulletinMasterViewModel> INewsBulletinService.GetById(int Id) => _newsBulletinRepo.GetById(Id);
        async Task<int> INewsBulletinService.Update(NewsBulletinMasterViewModel newsBulletinMasterViewModel)
        {
            var newsBulletin = new NewsBulletinMaster
            {
                Id = newsBulletinMasterViewModel.Id,
                Title = newsBulletinMasterViewModel.Title,
                Description = newsBulletinMasterViewModel.Description,
                PublishDate = newsBulletinMasterViewModel.PublishDate,
                ExpiryDate = newsBulletinMasterViewModel.ExpiryDate,
                IsActive = newsBulletinMasterViewModel.IsActive,
                CreatedBy = newsBulletinMasterViewModel.CreatedBy,
                CreatedDate = newsBulletinMasterViewModel.CreatedDate,
            };

            return await _newsBulletinRepo.Update(newsBulletin);
        }
        //Task<NewsBulletinMasterViewModel> INewsBulletinService.GetByDate(DateTime date) => _newsBulletinRepo.GetByDate(date);

    }
}
