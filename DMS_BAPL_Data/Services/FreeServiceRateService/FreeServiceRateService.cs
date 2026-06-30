using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.FreeServiceRateRepo;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.FreeServiceRateService
{
    public class FreeServiceRateService : IFreeServiceRateService
    {
        private readonly IFreeServiceRateRepo _freeServiceRateRepo;

        public FreeServiceRateService(IFreeServiceRateRepo freeServiceRateRepo)
        {
            _freeServiceRateRepo = freeServiceRateRepo;
        }

        Task<IEnumerable<FreeServiceRate>> IFreeServiceRateService.Get() => _freeServiceRateRepo.Get();
        Task<bool> IFreeServiceRateService.Insert(List<FreeServiceRateViewModel> freeServiceRateViewModel)
        {
            List<FreeServiceRate> freeServices = new List<FreeServiceRate>();

            foreach (var freeService in freeServiceRateViewModel)
            {
                var freeServiceRate = new FreeServiceRate
                {
                    OemmodelId = freeService.OemmodelId,
                    ServiceId = freeService.ServiceId,
                    EffectiveDate = freeService.EffectiveDate,
                    MetroRate = freeService.MetroRate,
                    MetroGst = freeService.MetroGst,
                    NonMetroRate = freeService.NonMetroRate,
                    NonMetroGst = freeService.NonMetroGst,
                    CreatedBy = freeService.CreatedBy,
                    CreatedDate = freeService.CreatedDate
                };

                freeServices.Add(freeServiceRate);
            }

            return _freeServiceRateRepo.Insert(freeServices);
        }
        Task<int> IFreeServiceRateService.Update(FreeServiceRateViewModel freeServiceRateViewModel)
        {
            var freeServiceRate = new FreeServiceRate
            {
                Id = freeServiceRateViewModel.Id,
                OemmodelId = freeServiceRateViewModel.OemmodelId,
                EffectiveDate = freeServiceRateViewModel.EffectiveDate,
                ServiceId = freeServiceRateViewModel.ServiceId,
                MetroRate = freeServiceRateViewModel.MetroRate,
                MetroGst = freeServiceRateViewModel.MetroGst,
                NonMetroRate = freeServiceRateViewModel.NonMetroRate,
                NonMetroGst = freeServiceRateViewModel.NonMetroGst,
                CreatedBy = freeServiceRateViewModel.CreatedBy,
                CreatedDate = freeServiceRateViewModel.CreatedDate,
                UpdatedBy = freeServiceRateViewModel.UpdatedBy,
                UpdatedDate = freeServiceRateViewModel.UpdatedDate
            };
            return _freeServiceRateRepo.Update(freeServiceRate);
        }
        public async Task<IEnumerable<FreeServiceRateGroupViewModel>> GetByOEMModelId(int? id)
        {
            if (id == null)
            {
                var rates = await _freeServiceRateRepo.GetByOEMModelId(null);

                return rates
                    .GroupBy(x => new
                    {
                        x.OemmodelId,
                        x.CreatedDate,
                        x.EffectiveDate
                    })
                    .Select(g => new FreeServiceRateGroupViewModel
                    {
                        OEMModelId = g.Key.OemmodelId,
                        OEMModelName = g.First().OEMModelName,
                        EffectiveDate = g.Key.EffectiveDate,
                        CreatedDate = g.Key.CreatedDate,
                        Services = g.ToList()
                    })
                    .OrderByDescending(x => x.CreatedDate)
                    .Select((g, index) =>
                    {
                        g.SrNo = index + 1;
                        return g;
                    })
                    .ToList();

            }

            var result = await _freeServiceRateRepo.GetByOEMModelId(id);

            return new List<FreeServiceRateGroupViewModel>
            {
                new FreeServiceRateGroupViewModel
                {
                    SrNo = 1,
                    OEMModelId = id,
                    EffectiveDate = result.FirstOrDefault()?.EffectiveDate,
                    OEMModelName = result.FirstOrDefault()?.OEMModelName,
                    Services = result.ToList()
                }
            };
        }



    }
}
