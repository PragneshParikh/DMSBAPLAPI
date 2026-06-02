using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.CircularRepo;
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
    public partial class CircularService : ICircularService
    {
        private readonly ICircularRepo _circularRepo;
        private readonly BapldmsvadContext _context;

        public CircularService(ICircularRepo circularRepo, BapldmsvadContext context)
        {
            _circularRepo = circularRepo;
            _context = context;
        }

        async Task<int> ICircularService.Create(CircularMasterViewModel circularMasterViewModel)
        {
            var circular = new CircularMaster
            {
                Title = circularMasterViewModel.Title,
                Description = circularMasterViewModel.Description,
                PublishDate = circularMasterViewModel.PublishDate,
                ExpiryDate = circularMasterViewModel.ExpiryDate,
                IsActive = circularMasterViewModel.IsActive,
                CreatedBy = circularMasterViewModel.CreatedBy,
                CreatedDate = circularMasterViewModel.CreatedDate,
            };

            return await _circularRepo.Create(circular);
        }

        Task<bool> ICircularService.Delete(int Id) => _circularRepo.Delete(Id);

        async Task<object> ICircularService.Get()
        {
            IEnumerable<CircularMasterViewModel> circularList =
                await _circularRepo.Get();

            var folderStructure = circularList
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

        Task<CircularMasterViewModel> ICircularService.GetById(int Id) => _circularRepo.GetById(Id);
        async Task<int> ICircularService.Update(CircularMasterViewModel circularMasterViewModel)
        {
            var circular = new CircularMaster
            {
                Id = circularMasterViewModel.Id,
                Title = circularMasterViewModel.Title,
                Description = circularMasterViewModel.Description,
                PublishDate = circularMasterViewModel.PublishDate,
                ExpiryDate = circularMasterViewModel.ExpiryDate,
                IsActive = circularMasterViewModel.IsActive,
                CreatedBy = circularMasterViewModel.CreatedBy,
                CreatedDate = circularMasterViewModel.CreatedDate,
            };

            return await _circularRepo.Update(circular);
        }
        //Task<NewsBulletinMasterViewModel> INewsBulletinService.GetByDate(DateTime date) => _newsBulletinRepo.GetByDate(date);

    }
}
