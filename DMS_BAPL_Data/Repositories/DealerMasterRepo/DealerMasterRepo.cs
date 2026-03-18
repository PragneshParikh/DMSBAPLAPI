using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.DealerMasterRepository
{
    public class DealerMasterRepo : IDealerMasterRepo
    {
        private readonly BapldmsvadContext _context;

        public DealerMasterRepo(BapldmsvadContext context)
        {
            _context = context;
        }


        public async Task<DealerMaster> AddDealerAsync(DealerMasterViewModel dealer)
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
                CreatedBy = 1,
                CreatedDate = DateTime.Now
            };

            await _context.DealerMasters.AddAsync(newDealer);
            await _context.SaveChangesAsync();

            return newDealer;
        }
        //public async Task<List<DealerMaster>> GetAllDealersAsync()
        //{
        //    return await _context.DealerMasters.ToListAsync();
        //}

        public async Task<List<DealerMaster>> GetAllDealersAsync(string? search)
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
                    EF.Functions.Like(d.Dealercode, $"%{search}%") ||
                    // Boolean Yes/No search
                    (search == "yes" && d.B2b) ||
                    (search == "no" && !d.B2b) ||

                    // Date search
                    d.RegDate.ToString().Contains(search)

                //For future implementation if needed.
                //EF.Functions.Like(d.TradCert, $"%{search}%") || 
                //EF.Functions.Like(d.BrandName!, $"%{search}%") ||
                //EF.Functions.Like(d.CinNo!, $"%{search}%") ||
                //EF.Functions.Like(d.VatNo!, $"%{search}%") ||
                //EF.Functions.Like(d.FameiiCode!, $"%{search}%") ||
                //EF.Functions.Like(d.RegAddress!, $"%{search}%")
                );
            }

            return await query.ToListAsync();
        }
        public async Task<DealerMaster> GetDealerById(int id)
        {
            return await _context.DealerMasters.Where(i => i.Id == id).FirstOrDefaultAsync();
        }



        public async Task<DealerMaster?> UpdateDealerAsync(int id, DealerMasterViewModel dealerDto)
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


            existingDealer.UpdatedBy = 0;
            existingDealer.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return existingDealer;
        }
        public async Task<List<DealerDropdownViewModel>> GetDealerDropdown()
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
        private DateTime ParseRegDate(string regDate)
        {
            var culture = new CultureInfo("en-IN");

            if (DateTime.TryParse(regDate, culture, DateTimeStyles.None, out DateTime parsedDate))
            {
                return parsedDate;
            }

            throw new Exception($"Invalid registration date format: {regDate}");
        }
    }
}

