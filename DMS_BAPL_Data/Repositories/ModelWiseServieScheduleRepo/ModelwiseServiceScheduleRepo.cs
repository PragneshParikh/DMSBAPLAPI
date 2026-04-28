using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.ModelWiseServieScheduleRepo
{
    public class ModelwiseServiceScheduleRepo : IModelwiseServiceSchedule
    {
        private readonly BapldmsvadContext _context;

        public ModelwiseServiceScheduleRepo(BapldmsvadContext context)
        {
            _context = context;

        }

        public async Task<List<ServiceHeadViewModel>> GetServiceHeadViews()
        {
            return await _context.ServiceHeads
                .Select(sh => new ServiceHeadViewModel
                {
                    ServiceHeadId = sh.Id,
                    ServiceHeadName = sh.ServiceHeadName
                })
                .ToListAsync();
        }

        public async Task<int> SavemodelwiseserviceScheduleAsync(List<ServiceScheduleVM> serviceScheduleVM)
        {
            foreach (var item in serviceScheduleVM)
            {
                if (item.Id == 0)
                {
                    var entity = new ModelwiseServiceSchedule
                    {
                        OemmodelId = item.OemmodelId,
                        Noofservice = item.Noofservice,
                        Seqno = item.Seqno,
                        SrNo = item.SrNo,
                        DaysFrom = item.DaysFrom,
                        DaysTo = item.DaysTo,
                        JourneyFrom = item.JourneyFrom,
                        JourneyTo = item.JourneyTo,
                        ServiceFrom = item.ServiceFrom,
                        ServiceHead = item.ServiceHead,
                        ServiceType = item.ServiceType,
                        EffectiveDate = item.EffectiveDate,
                        CreatedBy = "Admin", // You can replace this with the actual user
                        CreatedDate = DateTime.Now
                    };

                    _context.ModelwiseServiceSchedules.Add(entity);
                }
                else
                {
                    var existing = await _context.ModelwiseServiceSchedules
                        .FirstOrDefaultAsync(x => x.Id == item.Id);

                    if (existing != null)
                    {
                        existing.OemmodelId = item.OemmodelId;
                        existing.Noofservice = item.Noofservice;
                        existing.Seqno = item.Seqno;
                        existing.SrNo = item.SrNo;
                        existing.DaysFrom = item.DaysFrom;
                        existing.DaysTo = item.DaysTo;
                        existing.JourneyFrom = item.JourneyFrom;
                        existing.JourneyTo = item.JourneyTo;
                        existing.ServiceFrom = item.ServiceFrom;
                        existing.ServiceHead = item.ServiceHead;
                        existing.ServiceType = item.ServiceType;
                        existing.EffectiveDate = item.EffectiveDate;
                        existing.UpdatedBy = "Admin"; // You can replace this with the actual user
                        existing.UpdatedDate = DateTime.Now;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return 1; // You can return a more meaningful value if needed
        }
        public async Task<List<ServiceSchedulelistVM>> GetModelwiseservicescheduleListAsync(int? oemModelId, DateTime? effectiveDate)
        {
            var query = from s in _context.ModelwiseServiceSchedules
                        join m in _context.OemmodelMasters
                            on s.OemmodelId equals m.Id
                        join sh in _context.ServiceHeads
                            on s.ServiceHead equals sh.Id
                        join st in _context.ServiceTypes
                            on s.ServiceType equals st.Id
                        select new ServiceSchedulelistVM
                        {
                            Id = s.Id,
                            OemModelId = s.OemmodelId,   
                            ModelName = m.ModelName,
                            Noofservice = s.Noofservice,
                            Seqno = s.Seqno,
                            SrNo = s.SrNo,
                            DaysFrom = s.DaysFrom,
                            DaysTo = s.DaysTo,
                            JourneyFrom = s.JourneyFrom,
                            JourneyTo = s.JourneyTo,
                            ServiceHeadId = s.ServiceHead,
                            ServiceTypeId = s.ServiceType,
                            ServiceFrom = s.ServiceFrom,
                            ServiceHead = sh.ServiceHeadName,
                            ServiceType = st.ServiceTypeName,
                            EffectiveDate = s.EffectiveDate
                        };

            //  FILTER 1: OEM MODEL
            if (oemModelId.HasValue)
            {
                query = query.Where(x => x.OemModelId == oemModelId.Value);
            }

            //  FILTER 2: EFFECTIVE DATE
            if (effectiveDate.HasValue)
            {
                query = query.Where(x => x.EffectiveDate.Value.Date == effectiveDate.Value.Date);
            }

            return await query.ToListAsync();
        }

        //GET BY MODEL (FOR EDIT)
        public async Task<List<ServiceScheduleVM>> GetByModelwiseservicescheduleAsync(int oemModelId)
        {
            var data = await _context.ModelwiseServiceSchedules
                .Where(x => x.OemmodelId == oemModelId)
                .Select(s => new ServiceScheduleVM
                {
                    Id = s.Id,
                    OemmodelId = s.OemmodelId,
                    Noofservice = s.Noofservice,
                    Seqno = s.Seqno,
                    SrNo = s.SrNo,
                    DaysFrom = s.DaysFrom,
                    DaysTo = s.DaysTo,
                    JourneyFrom = s.JourneyFrom,
                    JourneyTo = s.JourneyTo,
                    ServiceFrom = s.ServiceFrom,
                    ServiceHead = s.ServiceHead,
                    ServiceType = s.ServiceType,
                    EffectiveDate = s.EffectiveDate
                })
                .ToListAsync();

            return data;
        }

        // Get OemModelID Based Modelvariant List api
        public async Task<List<ModellistVMbasedOnOemMode>> GetOemModelbasedModelVarientListAsync(int oemModelId)
        {
            var data = await (from m in _context.OemmodelMasters
                              join i in _context.ItemMasters
                              on m.ModelName equals i.Oemmodelname
                              where m.Id == oemModelId
                              select new ModellistVMbasedOnOemMode
                              {
                                  ModelName = i.Itemname
                              })
                .ToListAsync();
            return data;

        }
    }

}
