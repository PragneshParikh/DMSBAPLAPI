using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.LedgerMasterRepo
{
    public partial class LedgerMasterRepo : ILedgerMasterRepo
    {
        private readonly BapldmsvadContext _context;

        public LedgerMasterRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        async Task<IEnumerable<LedgerMaster>> ILedgerMasterRepo.GetAll()
        {
            try
            {

                return await _context.LedgerMasters
                                     .AsNoTracking()
                                     .OrderBy(c => c.LedgerName)
                                     .ToListAsync();

            }
            catch { throw; }
        }

        async Task<PagedResponse<LedgerMaster>> ILedgerMasterRepo.GetLedgerByPagedAsync(string? searchTerms, int pageIndex, int pageSize)
        {
            try
            {
                var query = _context.LedgerMasters.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(searchTerms))
                {
                    query = query.Where(c => c.LedgerType.Contains(searchTerms)
                                        || c.LedgerName.Contains(searchTerms)
                                        || c.MobileNumber.Contains(searchTerms)
                                        || c.EMail.Contains(searchTerms)
                                        || c.City.Contains(searchTerms)
                                        || c.State.Contains(searchTerms));
                }

                int totalRecords = await query.CountAsync();

                var items = await query
                    .OrderBy(c => c.LedgerName)
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                int startSrNo = (pageIndex * pageSize) + 1;

                return new PagedResponse<LedgerMaster>
                {
                    Data = items,
                    TotalRecords = totalRecords
                };
            }
            catch { throw; }

        }

        async Task<LedgerMaster?> ILedgerMasterRepo.GetLedgerById(int id)
        {
            try
            {
                return await _context.LedgerMasters
                               .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch { throw; }
        }

        async Task<int> ILedgerMasterRepo.InsertLedgerDetail(LedgerMaster ledgerMaster)
        {
            try
            {
                await _context.LedgerMasters.AddAsync(ledgerMaster);
                var result = await _context.SaveChangesAsync();
                return ledgerMaster.Id;
            }
            catch { throw; }
        }

        async Task<bool> ILedgerMasterRepo.UpdateLedgerDetail(LedgerMaster ledgerMaster)
        {
            try
            {
                _context.LedgerMasters.Update(ledgerMaster);
                return await _context.SaveChangesAsync() > 0;
            }
            catch { throw; }
        }

        public async Task<bool> CheckLedgerExist(string? email, string? mobile)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(email))
                {
                    var result = await _context.LedgerMasters.Where(i => i.EMail == email).FirstOrDefaultAsync();
                    if (result == null)
                        return false;
                }
                if (!string.IsNullOrWhiteSpace(mobile))
                {
                    var result = await _context.LedgerMasters.Where(i => i.MobileNumber == mobile).FirstOrDefaultAsync();
                    if (result == null)
                        return false;
                }
                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<LedgerMaster> CreateLedgerFromLead(LmsleadMaster lead, string userId)
        {
            var lastParty = await _context.LedgerMasters
                .Where(x => x.LedgerType == "Party" && x.LedgerCode != null)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (lastParty != null)
            {
                var numberPart = new string(lastParty.LedgerCode
                    .Where(char.IsDigit)
                    .ToArray());

                if (int.TryParse(numberPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            var firstName = lead.Name.Split(' ')[0];
            var ledgerCode = $"{firstName.ToUpper()}{nextNumber:D3}";

            var newLedger = new LedgerMaster
            {
                LedgerCode = ledgerCode,
                LedgerName = lead.Name,
                LedgerType = "Party",
                MobileNumber = lead.Mobile,
                EMail = lead.Email,
                City = lead.City,
                CreatedBy = userId,
                CreatedDate = DateTime.Now
            };

            _context.LedgerMasters.Add(newLedger);
            await _context.SaveChangesAsync();
            return newLedger;
        }

        public async Task<LedgerMaster> GetLedgerByEmailOrMobile(string? email, string? mobile)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(email))
                {
                    var result = await _context.LedgerMasters.Where(i => i.EMail == email).FirstOrDefaultAsync();
                    if (result != null)
                        return result;
                }
                if (!string.IsNullOrWhiteSpace(mobile))
                {
                    var result = await _context.LedgerMasters.Where(i => i.MobileNumber == mobile).FirstOrDefaultAsync();
                    if (result != null)
                        return result;
                }
                else
                {
                    throw new Exception("Email or Mobile number must be provided.");
                }
                return null;
            }
            catch
            {
                throw;
            }
        }
    }
}
