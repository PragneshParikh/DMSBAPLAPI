using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.DealerMasterRepository
{
    public class DealerMasterRepo : IDealerMasterRepo
    {
        private readonly BapldmsvadContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IDbContextTransaction _transaction;


        public DealerMasterRepo(BapldmsvadContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // Add new dealer
        public async Task<DealerMaster> AddDealerAsync(DealerMasterViewModel dealer, string userId)
        {
            try
            {
                var newDealer = new DealerMaster
                {
                    Compname = dealer.Compname,
                    Compcode = dealer.Compcode,
                    Adress1 = dealer.Adress1,
                    Adress2 = dealer.Adress2,
                    City = dealer.City,
                    State = dealer.State,
                    Pin = dealer.Pin,
                    Pan = dealer.Pan,
                    PhoneOff = dealer.PhoneOff ?? "",
                    Mobile = dealer.Mobile,
                    Email = dealer.Email,
                    Contactperson = dealer.Contactperson,
                    RegDate = ParseRegDate(dealer.RegDate),
                    TradCert = dealer.TradCert ?? "",
                    CompgstinNo = dealer.CompgstinNo ?? "",
                    BrandName = dealer.BrandName,
                    CompImage = dealer.CompImage,
                    Dealercode = dealer.Dealercode,
                    Areaofficeid = dealer.Areaofficeid,
                    CinNo = dealer.CinNo,
                    VatNo = dealer.VatNo,
                    IsTcs = dealer.IsTcs,
                    TcsPercent = dealer.TcsPercent,
                    FameiiCode = dealer.FameiiCode,
                    CeditLimit = dealer.CeditLimit,
                    RegAddress = dealer.RegAddress,
                    B2b = dealer.B2b,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now
                };

                await _context.DealerMasters.AddAsync(newDealer);
               // await _context.SaveChangesAsync();

                return newDealer;
            }
            catch
            {
                throw;
            }
        }


        public async Task AddDealerToLedgerAsync(DealerMasterViewModel dealer, string userId)
        {
            var state = await _context.States
    .FirstOrDefaultAsync(s => s.StateName.ToLower() == dealer.State.ToLower());

          
            var city = await _context.Cities
                .FirstOrDefaultAsync(c => c.CityName.ToLower() == dealer.City.ToLower()
                                      && c.StateId == state.StateId);

            var ledger = new LedgerMaster
            {
                LedgerCode = dealer.Dealercode,
                LedgerName = dealer.Compname,
                LedgerType = "Dealer",
                Gstno = dealer.CompgstinNo,
                Pan = dealer.Pan,
                MobileNumber = dealer.Mobile,
                Address = string.Join(" ", new[] { dealer.Adress1, dealer.Adress2 }
                                .Where(x => !string.IsNullOrEmpty(x))),
                City = city.CityId,
                State = state.StateId,
                Pin = dealer.Pin,
                EMail = dealer.Email,
                CreatedBy = userId,
                CreatedDate = DateTime.Now
            };

            await _context.LedgerMasters.AddAsync(ledger);
          //  await _context.SaveChangesAsync();

        }
        
        // Get all dealers with optional search
        public async Task<List<DealerMaster>> GetAllDealersAsync(string? search)
        {
            try
            {
                IQueryable<DealerMaster> query = _context.DealerMasters;

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(d =>
                        EF.Functions.Like(d.Compname, $"%{search}%") ||
                        EF.Functions.Like(d.Compcode, $"%{search}%") ||
                        EF.Functions.Like(d.Adress1, $"%{search}%") ||
                        EF.Functions.Like(d.Adress2, $"%{search}%") ||
                        EF.Functions.Like(d.City, $"%{search}%") ||
                        EF.Functions.Like(d.State, $"%{search}%") ||
                        EF.Functions.Like(d.Pin, $"%{search}%") ||
                        EF.Functions.Like(d.Pan, $"%{search}%") ||
                        EF.Functions.Like(d.PhoneOff, $"%{search}%") ||
                        EF.Functions.Like(d.Mobile, $"%{search}%") ||
                        EF.Functions.Like(d.Email, $"%{search}%") ||
                        EF.Functions.Like(d.Contactperson, $"%{search}%") ||
                        EF.Functions.Like(d.CompgstinNo, $"%{search}%") ||
                        EF.Functions.Like(d.Dealercode, $"%{search}%") ||
                        EF.Functions.Like(d.TradCert, $"%{search}%") ||

                        // Boolean search
                        (search == "yes" && d.B2b) ||
                        (search == "no" && !d.B2b) ||

                        // Date search
                        d.RegDate.ToString().Contains(search)
                    );
                }

                return await query.OrderByDescending(i=>i.CreatedDate).ToListAsync();
            }
            catch
            {
                throw;
            }
        }

        // Get dealer by ID
        public async Task<DealerMaster> GetDealerById(int id)
        {
            try
            {
                return await _context.DealerMasters
                    .Where(i => i.Id == id)
                    .FirstOrDefaultAsync();
            }
            catch
            {
                throw;
            }
        }

        // Get dealer by code
        public async Task<DealerMaster> GetDealerByCode(string dealerCode)
        {
            try
            {
                return await _context.DealerMasters
                    .Where(i => i.Dealercode == dealerCode)
                    .FirstOrDefaultAsync();
            }
            catch
            {
                throw;
            }
        }

        // Update dealer
        public async Task<DealerMaster?> UpdateDealerAsync(int id, DealerMasterViewModel dealerDto, string userId)
        {
            try
            {
                var existingDealer = await _context.DealerMasters.FindAsync(id);

                if (existingDealer == null)
                    return null;

                existingDealer.Compname = dealerDto.Compname;
                existingDealer.Compcode = dealerDto.Compcode;
                existingDealer.Adress1 = dealerDto.Adress1;
                existingDealer.Adress2 = dealerDto.Adress2;
                existingDealer.City = dealerDto.City;
                existingDealer.State = dealerDto.State;
                existingDealer.Pin = dealerDto.Pin;
                existingDealer.Pan = dealerDto.Pan;
                existingDealer.PhoneOff = dealerDto.PhoneOff ?? "";
                existingDealer.Mobile = dealerDto.Mobile;
                existingDealer.Email = dealerDto.Email;
                existingDealer.Contactperson = dealerDto.Contactperson;
                existingDealer.RegDate = ParseRegDate(dealerDto.RegDate);
                existingDealer.TradCert = dealerDto.TradCert ?? "";
                existingDealer.CompgstinNo = dealerDto.CompgstinNo ?? "";
                existingDealer.BrandName = dealerDto.BrandName;
                existingDealer.CompImage = dealerDto.CompImage;
                existingDealer.Dealercode = dealerDto.Dealercode;
                existingDealer.Areaofficeid = dealerDto.Areaofficeid;
                existingDealer.CinNo = dealerDto.CinNo;
                existingDealer.VatNo = dealerDto.VatNo;
                existingDealer.IsTcs = dealerDto.IsTcs;
                existingDealer.TcsPercent = dealerDto.TcsPercent;
                existingDealer.FameiiCode = dealerDto.FameiiCode;
                existingDealer.CeditLimit = dealerDto.CeditLimit;
                existingDealer.RegAddress = dealerDto.RegAddress;
                existingDealer.B2b = dealerDto.B2b;

                existingDealer.UpdatedBy = userId;
                existingDealer.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();

                return existingDealer;
            }
            catch
            {
                throw;
            }
        }

        // Get dealer dropdown list
        public async Task<List<DealerDropdownViewModel>> GetDealerDropdown()
        {
            try
            {
                var dealerCodes = await _context.LocationMasters
                    .Select(x => x.Dealercode)
                    .Distinct()
                    .ToListAsync();

                var result = await _context.DealerMasters
                    .Where(x => dealerCodes.Contains(x.Dealercode))
                    .Select(x => new DealerDropdownViewModel
                    {
                        DealerCode = x.Dealercode,
                        DealerName = x.Compname
                    })
                    .OrderBy(x => x.DealerName)
                    .ToListAsync();

                return result;
            }
            catch
            {
                throw;
            }
        }

        // Parse registration date safely
        private DateTime ParseRegDate(string regDate)
        {
            try
            {
                var culture = new CultureInfo("en-IN");

                if (DateTime.TryParse(regDate, culture, DateTimeStyles.None, out DateTime parsedDate))
                {
                    return parsedDate;
                }

                throw new Exception($"Invalid registration date format: {regDate}");
            }
            catch
            {
                throw;
            }
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
                await _transaction.CommitAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
                await _transaction.RollbackAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<DealerMaster> EditTradeCertificate(int dealerId, string tradeCertificate)
        {
            try
            {
                var dealer = await _context.DealerMasters
                    .FirstOrDefaultAsync(d => d.Id == dealerId);

                if (dealer == null)
                    throw new KeyNotFoundException(StringConstants.DealerNotFound);

                dealer.TradCert = tradeCertificate;
                dealer.UpdatedDate = DateTime.Now;
                dealer.UpdatedBy = GetUserInfoFromToken.GetUserIdFromToken(_httpContextAccessor.HttpContext);

                await _context.SaveChangesAsync();

                return dealer;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}