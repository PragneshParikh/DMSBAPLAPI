using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.AgreeTaxcodeRepo;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.JobCardRepo
{
    public class JobCardRepo : IJobCardRepo
    {
        private readonly BapldmsvadContext _context;

        public JobCardRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        // bind jobtype dropdown
        public async Task<List<JobCardViewModel>> GetJobtype()
        {
            return await _context.JobTypes
                .Select(j => new JobCardViewModel
                {
                    JobTypeId = j.Id,
                    JobtypeName = j.JobTypeName
                })
                .ToListAsync();
        }

        // bind service head dropdown based on job type
        public async Task<List<ServiceHeadViewModel>> GetServiceHead(int jobTypeId)
        {
            return await _context.ServiceHeads
                .Where(x => x.JobTypeId == jobTypeId)
                .Select(x => new ServiceHeadViewModel
                {
                    ServiceHeadId = x.Id,
                    ServiceHeadName = x.ServiceHeadName
                }).ToListAsync();
        }

        // bind service type dropdown based on service head
        public async Task<List<ServiceTypeViewModel>> GetServiceType(int serviceHeadId)
        {
            return await _context.ServiceTypes
                .Where(x => x.ServiceHeadId == serviceHeadId)
                .Select(x => new ServiceTypeViewModel
                {
                    ServiceTypeId = x.Id,
                    ServiceTypeName = x.ServiceTypeName
                }).ToListAsync();
        }

        // get service data based on job type
        public async Task<List<ServiceDataViewModel>> GetServiceDataByJobType(string jobTypeName)
        {
            var result = await (from jt in _context.JobTypes
                                join sh in _context.ServiceHeads on jt.JobTypeName equals sh.ServiceHeadName
                                join st in _context.ServiceTypes on sh.Id equals st.ServiceHeadId
                                where jt.JobTypeName == jobTypeName
                                select new ServiceDataViewModel
                                {
                                    JobTypeName = jt.JobTypeName,
                                    ServiceHead = sh.ServiceHeadName,
                                    ServiceType = st.ServiceTypeName
                                }).ToListAsync();

            return result;
        }

        // get inspected lot chassis based on invoice number
        public async Task<List<LotInspectionChassisVM>> GetAllInspectedLotChassisAsync(string dealerCode)
        {
            var result = await (from h in _context.LotinspectionHeaders
                                join d in _context.LotinspectionDetails
                                on h.Id equals d.LotHeaderId
                                where h.IsLotInspected == true && h.DealerCode == dealerCode
                                select new LotInspectionChassisVM
                                {
                                    InvoiceNo = h.InvoiceNo,
                                    ChassisNumber = d.ChassisNo
                                }).ToListAsync();

            return result;
        }

    }
}
