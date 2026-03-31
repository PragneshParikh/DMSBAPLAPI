using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.LeadMasterRepo;
using DMS_BAPL_Data.Repositories.LedgerMasterRepo;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.LeadMasterService
{
    public class LeadMasterService : ILeadMasterService
    {
        private readonly ILeadMasterRepo _leadMasterRepo;
        private readonly ILedgerMasterRepo _ledgerMasterRepo;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public LeadMasterService(ILeadMasterRepo leadMasterRepo,ILedgerMasterRepo ledgerMasterRepo, IHttpContextAccessor httpContextAccessor)
        {
            _leadMasterRepo = leadMasterRepo;
            _ledgerMasterRepo = ledgerMasterRepo;
            _httpContextAccessor = httpContextAccessor;
        }
        async Task<LeadViewModel> ILeadMasterService.InsertLmsleadMasterAsync(LeadViewModel leadViewModel)
        {
            try
            {
                return await _leadMasterRepo.InsertLmsleadAsync(leadViewModel);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<List<LmsleadMaster>> GetAlllmsleadMasters()
        {
            return await _leadMasterRepo.GetAlllmsleadMasters();


        }

        public async Task<LmsleadMaster> GetLMSLeadMasterByMobileNo(string? mobileNo, int? bookingId)
        {
            try
            {
                var result= await _leadMasterRepo.GetLMSLeadMasterByMobileNo(mobileNo, bookingId);
                bool checkIfExistInLedger= await _ledgerMasterRepo.CheckLedgerExist(result.Email,mobileNo);
                if (checkIfExistInLedger == false)
                {
                    var httpContext = _httpContextAccessor.HttpContext;

                    if (httpContext == null)
                    {
                        throw new Exception("HttpContext is null");
                    }

                    string userId = GetUserInfoFromToken.GetUserIdFromToken(httpContext);
                    await _ledgerMasterRepo.CreateLedgerFromLead(result,userId);

                }
                return result;

            }
            catch (Exception)
            {
                throw;
            }
        }


    }
}
