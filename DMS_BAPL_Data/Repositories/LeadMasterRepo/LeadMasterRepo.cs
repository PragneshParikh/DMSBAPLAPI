using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.LeadMasterRepo;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMS_BAPL_Data.ViewModels;
using DMS_BAPL_Utils.ViewModels;

namespace DMS_BAPL_Data.Repositories.LeadMasterRep
{
    public class LeadMasterRepo : ILeadMasterRepo
    {
        private readonly BapldmsvadContext _context;

        public LeadMasterRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        async Task<LeadViewModel> ILeadMasterRepo.InsertLmsleadAsync(LeadViewModel leadViewModel)
        {
            try
            {
                // Get Color from ColorMaster
                var color = await _context.ColorMasters
                    .FirstOrDefaultAsync(c => c.Id == leadViewModel.ColorId);

                if (color == null)
                {
                    throw new Exception("Invalid Color Id");
                }

                // Get Dealer from DealerMaster
                var dealer = await _context.DealerMasters
                    .FirstOrDefaultAsync(d => d.Id == leadViewModel.DealerId);

                if (dealer == null)
                {
                    throw new Exception("Invalid Dealer Id");
                }

                var lmsleadMaster = new LmsleadMaster
                {
                    Leadid = leadViewModel.leadid,
                    Name = leadViewModel.CustomerName,
                    Mobile = leadViewModel.CustomerMobile,
                    Email = leadViewModel.CustomerEmail,
                    Date = leadViewModel.LMSDate,
                    Area = leadViewModel.LMSArea,
                    City = leadViewModel.LMSCity,
                    Brancharea = leadViewModel.LMSBrancharea,
                    Company = leadViewModel.CustomerCompany,
                    Pincode = leadViewModel.LMSPincode,
                    Time = leadViewModel.CustomerTime,
                    Branchpin = leadViewModel.LMSBranchpin,
                    Model = leadViewModel.LMSModel,
                    Variant = leadViewModel.LMSVariant,
                    Productcode = leadViewModel.Productcode,
                    Sourceapp = leadViewModel.Sourceapp,

                    // Foreign Keys
                    ColorId = leadViewModel.ColorId,
                    DealerId = leadViewModel.DealerId,

                    // Master table values
                    Color = color.Colorname,
                    Dealercode = dealer.Dealercode,

                    CreatedBy = leadViewModel.createdby.ToString(),
                    CreatedDate = leadViewModel.createddatetime,
                    UpdatedBy = leadViewModel.updatedby?.ToString(),
                    UpdatedDate = leadViewModel.updateddatetime ?? DateTime.Now
                };

                _context.LmsleadMasters.Add(lmsleadMaster);
                await _context.SaveChangesAsync();

                // inserted data return
                leadViewModel.Color = color.Colorname;
                leadViewModel.Dealercode = dealer.Dealercode;

                return leadViewModel;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<List<LmsleadMaster>> GetAlllmsleadMasters()
        {
            var query = _context.LmsleadMasters.AsQueryable();
            
             
            return await query.ToListAsync();
        }




    }
}
