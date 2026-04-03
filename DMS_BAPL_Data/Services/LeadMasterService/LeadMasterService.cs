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

        public async Task<(LmsleadMaster lead, int? ledgerId, bool isNew)> GetLMSLeadMasterByMobileNo(string? mobileNo, int? bookingId)
        {
            try
            {
                var result = await _leadMasterRepo.GetLMSLeadMasterByMobileNo(mobileNo, bookingId);

                bool exists = await _ledgerMasterRepo.CheckLedgerExist(result.Email, mobileNo);

                int? ledgerId = null;
                bool isNew = false;

                if (!exists)
                {
                    var httpContext = _httpContextAccessor.HttpContext;
                    string userId = GetUserInfoFromToken.GetUserIdFromToken(httpContext);

                   var ledger = await _ledgerMasterRepo.CreateLedgerFromLead(result, userId);
                    ledgerId = ledger.Id;
                    isNew = true;
                }
                else
                {
                    var ledger = await _ledgerMasterRepo.GetLedgerByEmailOrMobile(result.Email, mobileNo);
                    ledgerId = ledger?.Id;
                }

                return (result, ledgerId, isNew);
            }
            catch
            {
                throw;
            }
        }
    }
}
