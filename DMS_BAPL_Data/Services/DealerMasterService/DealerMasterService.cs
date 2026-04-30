using Azure.Core;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.DealerMasterRepository;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.DealerMasterService
{
    public class DealerMasterService : IDealerMasterService
    {
        private readonly IDealerMasterRepo _dealerMasterRepo;
        private readonly IExcelService _excelService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DealerMasterService(IDealerMasterRepo dealerMasterRepo, IExcelService excelService, UserManager<ApplicationUser> userManager)
        {
            _dealerMasterRepo = dealerMasterRepo;
            _excelService = excelService;
            _userManager = userManager;
        }

        // Create dealer and corresponding identity user
        public async Task<DealerMaster?> AddDealerAsync(DealerMasterViewModel dealer, string userId)
        {
            await _dealerMasterRepo.BeginTransactionAsync();

            try
            {
                var existingDealer = await _dealerMasterRepo.GetDealerByCode(dealer.Dealercode);

                DealerMaster result;

                if (existingDealer != null)
                {
                    var _existingDealer = new DealerMasterViewModel
                    {
                        Compname = dealer.Compname,
                        Compcode = dealer.Compcode,
                        Adress1 = dealer.Adress1,
                        Adress2 = dealer.Adress2,
                        City = dealer.City,
                        State = dealer.State,
                        Pin = dealer.Pin,
                        Pan = dealer.Pan,
                        PhoneOff = dealer.PhoneOff,
                        Mobile = dealer.Mobile,
                        Email = dealer.Email,
                        Contactperson = dealer.Contactperson,
                        RegDate = dealer.RegDate,
                        TradCert = dealer.TradCert,
                        CompgstinNo = dealer.CompgstinNo,
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
                        CreatedBy = dealer.CreatedBy,
                        CreatedDate = dealer.CreatedDate,
                        UpdatedBy = dealer.UpdatedBy,
                        UpdatedDate = dealer.UpdatedDate
                    };

                    result = await _dealerMasterRepo.UpdateDealerAsync(existingDealer.Id, _existingDealer, userId);
                }
                else
                {
                    result = await _dealerMasterRepo.AddDealerAsync(dealer, userId);

                    await _dealerMasterRepo.AddDealerToLedgerAsync(dealer, userId);
                }

                await _dealerMasterRepo.SaveAsync();

                var existingUser = await _userManager.FindByNameAsync(dealer.Dealercode);

                if (existingUser == null)
                {
                    var newUser = new ApplicationUser
                    {
                        UserName = result.Dealercode,
                        Email = result.Email,
                        EmailConfirmed = true
                    };

                    var userResult = await _userManager.CreateAsync(
                        newUser,
                        StringConstants.DealerDefaultPassword
                    );

                    if (!userResult.Succeeded)
                        throw new Exception(string.Join(", ", userResult.Errors.Select(e => e.Description)));

                    var roleResult = await _userManager.AddToRoleAsync(newUser, StringConstants.DealerText);

                    if (!roleResult.Succeeded)
                        throw new Exception("Role assignment failed");
                }

                await _dealerMasterRepo.CommitTransactionAsync();

                return result;
            }
            catch
            {
                await _dealerMasterRepo.RollbackTransactionAsync();
                throw;
            }
        }

        // Get all dealers with optional search
        public async Task<List<DealerMaster>> GetAllDealersAsync(string? search)
        {
            try
            {
                return await _dealerMasterRepo.GetAllDealersAsync(search);
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
                return await _dealerMasterRepo.GetDealerById(id);
            }
            catch
            {
                throw;
            }
        }

        // Update dealer details
        public async Task<DealerMaster?> UpdateDealerAsync(int id, DealerMasterViewModel dealer, string userId)
        {
            try
            {
                return await _dealerMasterRepo.UpdateDealerAsync(id, dealer, userId);
            }
            catch
            {
                throw;
            }
        }

        // Export dealer list to Excel
        public async Task<byte[]> DownloadDealerExcel()
        {
            try
            {
                var data = await _dealerMasterRepo.GetAllDealersAsync(null);

                var properties = typeof(DealerMasterViewModel)
                    .GetProperties()
                    .ToList();

                var columns = properties.Select(p => p.Name).ToList();

                var rows = data.Select(d =>
                {
                    var dict = new Dictionary<string, object>();

                    foreach (var prop in properties)
                    {
                        var entityProp = d.GetType().GetProperty(prop.Name);
                        dict[prop.Name] = entityProp != null
                            ? entityProp.GetValue(d)
                            : null;
                    }

                    return dict;
                }).ToList();

                var model = new ExcelExportViewModel
                {
                    SheetName = StringConstants.DealerExcelSheetName,
                    Columns = columns,
                    Rows = rows
                };

                return await _excelService.GenerateExcel(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString()); // optional logging
                throw;
            }
        }

        // Get dealer dropdown list
        public async Task<List<DealerDropdownViewModel>> GetDealerDropdown()
        {
            try
            {
                return await _dealerMasterRepo.GetDealerDropdown();
            }
            catch
            {
                throw;
            }
        }

        // Get dealer by dealer code
        public async Task<DealerMaster> GetDealerByCode(string dealerCode)
        {
            try
            {
                return await _dealerMasterRepo.GetDealerByCode(dealerCode);
            }
            catch
            {
                throw;
            }
        }

        //Update Trade Certificate
        public async Task<DealerMaster> EditTradeCertificate(int dealerId, string tradeCertificate)
        {
            try
            {
                return await _dealerMasterRepo.EditTradeCertificate(dealerId, tradeCertificate);
            }
            catch
            {
                throw;
            }


        }

        public Task<object> UpdateByDealerCode(string dealerCode, string userId, DealerMasterViewModel dealerMasterViewModel) => _dealerMasterRepo.UpdateByDealerCode(dealerCode, userId, dealerMasterViewModel);
    }
}