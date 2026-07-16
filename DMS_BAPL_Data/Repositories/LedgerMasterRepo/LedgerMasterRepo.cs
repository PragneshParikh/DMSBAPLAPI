using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using MailKit.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Helpers;
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

        async Task<IEnumerable<LedgerExcelViewModel>> ILedgerMasterRepo.GetExcelData()
        {
            try
            {
                var query = _context.LedgerMasters.AsNoTracking();
                var result = await (
                   from LM in query
                   join C in _context.Cities
                       on LM.City equals C.CityId into cityGroup
                   from city in cityGroup.DefaultIfEmpty()

                   join S in _context.States
                       on LM.State equals S.StateId into stateGroup
                   from state in stateGroup.DefaultIfEmpty()

                   join O in _context.OccupationMasters
                        on LM.OccupationId equals O.Id into occupationGroup
                   from occupation in occupationGroup.DefaultIfEmpty()

                   join D in _context.DealerMasters
                        on LM.DealerCode equals D.Dealercode into DealerInfo
                   from dealer in DealerInfo.DefaultIfEmpty()

                   orderby LM.CreatedDate descending

                   select new LedgerExcelViewModel
                   {
                       LedgerCode = LM.LedgerCode,
                       LedgerName = LM.LedgerName,
                       DealerName = dealer.Compname,
                       LedgerType = LM.LedgerType,
                       Gstno = LM.Gstno,
                       Pan = LM.Pan,
                       AadharNumber = LM.AadharNumber,
                       MobileNumber = LM.MobileNumber,
                       Address = LM.Address,
                       Pin = LM.Pin,
                       DealerCode = LM.DealerCode,
                       EMail = LM.EMail,
                       Gender = LM.Gender,
                       DateOfBirth = LM.DateOfBirth,
                       Occupation = occupation != null ? occupation.OccupationName : string.Empty,
                       cityName = city != null ? city.CityName : string.Empty,
                       stateName = state != null ? state.StateName : string.Empty
                   }
               )

               .ToListAsync();
                return result;

            }
            catch
            {
                throw;
            }
        }

        async Task<PagedResponse<object>> ILedgerMasterRepo.GetLedgerByPagedAsync(string? searchTerms, int pageIndex, int pageSize, string dealerCode, string filter)
        {
            try
            {
                var query = _context.LedgerMasters.AsNoTracking();
                if (dealerCode != null)
                {
                    query = query.Where(i => i.LedgerVisibility == dealerCode || i.LedgerVisibility.ToLower() == "all");
                }
                if (filter != null)
                {
                    query = query.Where(i => i.DealerCode == filter);
                }
                if (!string.IsNullOrWhiteSpace(searchTerms))
                {
                    query = query.Where(c => c.LedgerType.Contains(searchTerms)
                                        || c.LedgerName.Contains(searchTerms)
                                        || c.MobileNumber.Contains(searchTerms)
                                        || c.EMail.Contains(searchTerms));
                    //|| c.City.Contains(searchTerms)
                    //|| c.State.Contains(searchTerms));
                }

                int totalRecords = await query.CountAsync();

                var result = await (
                    from LM in query
                    join C in _context.Cities
                        on LM.City equals C.CityId into cityGroup
                    from city in cityGroup.DefaultIfEmpty()

                    join S in _context.States
                        on LM.State equals S.StateId into stateGroup
                    from state in stateGroup.DefaultIfEmpty()

                    join D in _context.DealerMasters
                        on LM.DealerCode equals D.Dealercode into DealerInfo
                    from dealer in DealerInfo.DefaultIfEmpty()

                    orderby LM.CreatedDate descending

                    select new
                    {
                        Id = LM.Id,
                        LedgerCode = LM.LedgerCode,
                        LedgerName = LM.LedgerName,
                        LedgerType = LM.LedgerType,
                        Gstno = LM.Gstno,
                        Pan = LM.Pan,
                        AadharNumber = LM.AadharNumber,
                        MobileNumber = LM.MobileNumber,
                        Address = LM.Address,
                        City = LM.City,
                        State = LM.State,
                        Pin = LM.Pin,
                        EMail = LM.EMail,
                        Gender = LM.Gender,
                        DateOfBirth = LM.DateOfBirth,
                        CreatedBy = LM.CreatedBy,
                        CreatedDate = LM.CreatedDate,
                        UpdatedBy = LM.UpdatedBy,
                        UpdatedDate = LM.UpdatedDate,
                        LedgerVisibility = LM.LedgerVisibility,
                        cityName = city.CityName,
                        stateName = state.StateName,
                        DealerCode = dealer.Dealercode,
                        DealerName = dealer.Compname,
                    }
                )
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

                return new PagedResponse<object>
                {
                    Data = result.Cast<object>().ToList(),
                    TotalRecords = totalRecords
                };
            }
            catch { throw; }
        }

        public async Task<LedgerDetailViewModel?> GetLedgerById(int id)
        {
            //return await _context.LedgerMasters.Include(i => i.State)
            //.FirstOrDefaultAsync(x => x.Id == id);
            try
            {
                var query = _context.LedgerMasters.AsNoTracking();

                var result = await (
                    from LM in query
                    join C in _context.Cities
                        on LM.City equals C.CityId into cityGroup
                    from city in cityGroup.DefaultIfEmpty()

                    join S in _context.States
                        on LM.State equals S.StateId into stateGroup
                    from state in stateGroup.DefaultIfEmpty()

                    where LM.Id == id
                    select new LedgerDetailViewModel
                    {
                        Id = LM.Id,
                        LedgerCode = LM.LedgerCode,
                        LedgerName = LM.LedgerName,
                        LedgerType = LM.LedgerType,
                        Gstno = LM.Gstno,
                        DealerCode =LM.DealerCode,
                        Pan = LM.Pan,
                        AadharNumber = LM.AadharNumber,
                        MobileNumber = LM.MobileNumber,
                        AltMobileNumber = LM.AlternateMobileNo,
                        Address = LM.Address,
                        City = LM.City,
                        State = LM.State,
                        Pin = LM.Pin,
                        EMail = LM.EMail,
                        Gender = LM.Gender,
                        DateOfBirth = LM.DateOfBirth,
                        CreatedBy = LM.CreatedBy,
                        CreatedDate = LM.CreatedDate,
                        UpdatedBy = LM.UpdatedBy,
                        UpdatedDate = LM.UpdatedDate,
                        OccupationId = LM.OccupationId,
                        cityName = city.CityName,
                        stateName = state.StateName,
                        D2DProvision =LM.D2dprovision
                    }
                ).FirstOrDefaultAsync();

                return result;
            }
            catch { throw; }

        }

        async Task<int> ILedgerMasterRepo.InsertLedgerDetail(LedgerMaster ledgerMaster)
        {
            try
            {
                var existingLedger = await _context.LedgerMasters
                    .FirstOrDefaultAsync(i => i.MobileNumber == ledgerMaster.MobileNumber &&
                    i.LedgerType == "Receipt");

                if (existingLedger != null)
                {
                    ledgerMaster.Id = existingLedger.Id;
                    _context.Entry(existingLedger).CurrentValues.SetValues(ledgerMaster);
                    _context.Entry(existingLedger).Property(x => x.Id).CurrentValue = existingLedger.Id;
                    _context.Entry(existingLedger).Property(x => x.Id).IsModified = false;
                    await _context.SaveChangesAsync();
                    return existingLedger.Id;
                }
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
                // City = lead.City,
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
        public async Task<IEnumerable<LedgerDetailViewModel>> GetCompanyLedgers()
        {
            try
            {
                var result = await (from L in _context.LedgerMasters
                                    join S in _context.States
                                        on L.State equals S.StateId

                                    where L.LedgerType != null && L.LedgerType.ToLower() == "company"
                                    orderby L.LedgerName
                                    select new LedgerDetailViewModel
                                    {
                                        LedgerCode = L.LedgerCode,
                                        LedgerName = L.LedgerName,
                                        DateOfBirth = L.DateOfBirth,
                                        OccupationId = L.OccupationId,
                                        Gender = L.Gender,
                                        EMail = L.EMail,
                                        Pin = L.Pin,
                                        State = L.State,
                                        City = L.City,
                                        Address = L.Address,
                                        MobileNumber = L.MobileNumber,
                                        AadharNumber = L.AadharNumber,
                                        Pan = L.Pan,
                                        Gstno = L.Gstno,
                                        LedgerType = L.LedgerType,
                                        Id = L.Id,
                                        stateName = S.StateName
                                    }).ToListAsync();

                return result;
            }
            catch { throw; }
        }

        public async Task<IEnumerable<LedgerMaster>> GetLotRelatedLedgers(string? dealerCode, bool? IsD2D)
        {

            Console.WriteLine($"IsD2D: {IsD2D}");
            Console.WriteLine($"dealerCode: {dealerCode}");

            try
            {
                if (IsD2D == true && !string.IsNullOrWhiteSpace(dealerCode))
                {
                    return await _context.LedgerMasters
                    .AsNoTracking()
                    .Where(x => x.LedgerType != null && x.DealerCode == dealerCode && x.LedgerType.ToLower() == "dealer")
                    .OrderBy(c => c.LedgerName)
                    .ToListAsync();
                }
                else
                {
                    return await _context.LedgerMasters
                        .AsNoTracking()
                        .Where(x => x.LedgerType != null && x.LedgerType.ToLower() == "company")
                        .OrderBy(c => c.LedgerName)
                        .ToListAsync();
                }
            }
            catch { throw; }
        }

        public async Task<IEnumerable<LedgerMaster>> GetInsuranceLedgers()
        {
            try
            {
                return await _context.LedgerMasters
                    .AsNoTracking()
                    .Where(x => x.LedgerType != null && x.LedgerType.ToLower() == "insurance")
                    .OrderBy(c => c.LedgerName)
                    .ToListAsync();
            }
            catch { throw; }
        }

        public async Task<List<LedgerMaster>> GetLedgerByLedgerType(string ledgerType)
        {
            try
            {
                return await _context.LedgerMasters.
                    Where(i => i.LedgerType == ledgerType).ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<string>> GetAllMobileNumberByDealerCode(string dealerCode)
        {
            try
            {
                return await _context.LedgerMasters.Where(i => i.DealerCode == dealerCode).Select(i => i.MobileNumber).ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task<string> GetNextLedCode(string dealerCode)
        {
            try
            {
                string dealerSuffix = dealerCode.Length >= 4 ? dealerCode.Substring(dealerCode.Length - 4) : dealerCode;
                string? lastLedgerCode = await _context.LedgerMasters.Where(x => x.DealerCode == dealerCode).OrderByDescending(x => x.Id).Select(x => x.LedgerCode).FirstOrDefaultAsync();
                int nextNumber = 1;
                if (!string.IsNullOrWhiteSpace(lastLedgerCode))
                {
                    string[] parts = lastLedgerCode.Split('/');
                    if (parts.Length == 3 && int.TryParse(parts[2], out int currentNumber))
                    {
                        nextNumber = currentNumber + 1;
                    }
                }

                return $"LED/{dealerSuffix}/{nextNumber}";
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<LedgerMaster>> GetLedgerForSale(string? dealerCode, bool isSuperAdmin)
        {
            try
            {
                var result = await _context.LedgerMasters.Where(i => i.LedgerType.ToLower() == "institutional" || i.LedgerType.ToLower() == "party" || i.LedgerType.ToLower() == "dealer").ToListAsync();
                if (!isSuperAdmin)
                {
                    result = result.Where(i => i.DealerCode == dealerCode || i.LedgerType.ToLower() == "dealer").ToList();
                }
                return result;

            }
            catch
            {
                throw;
            }
        }

        public async Task<bool?> GetD2DProvision(string? dealerCode)
        {
            try
            {
                var result = await _context.LedgerMasters.Where(i => i.LedgerType.ToLower() == "dealer" && i.DealerCode == dealerCode).FirstOrDefaultAsync();
                if(result == null || result.D2dprovision == null)
                {
                    return false;   
                }
                return result.D2dprovision;
            }
            catch
            {
                throw;
            }
        }
        //public async Task<List<LedgerMaster>> GetLedgerByVisibility(string? dealerCode)
        //{
        //    try
        //    {
        //        var result = await _context.LedgerMasters.Where(i => i.LedgerType.ToLower() == "institutional" || i.LedgerType.ToLower() == "party" || i.LedgerType.ToLower() == "dealer").ToListAsync();
        //        if(dealerCode!=null)
        //        {
        //            result = result. Where(i=>i)
        //        }

        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}

    }
}
