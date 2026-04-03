using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.VehicleSaleBillRepo;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.VehicleSaleBillService
{
    public class VehicleSaleBillService:IVehicleSaleBillService
    {
        private readonly IVehicleSaleBillRepo _repo;
        private readonly IHttpContextAccessor _contextAccessor;
        public VehicleSaleBillService(IVehicleSaleBillRepo repo,IHttpContextAccessor contextAccessor)
        {
           _repo = repo; 
            _contextAccessor = contextAccessor;
        }

        public async Task<int> CreateAsync(VehicleSaleBillEditCreateViewModel model)
        {
            var header = MapToEntity(model);
            header.CreatedBy=GetUserInfoFromToken.GetUserIdFromToken(_contextAccessor.HttpContext);
            return await _repo.CreateAsync(header);
        }

        // ✅ GET BY ID
        public async Task<VehicleSaleBillResponseViewModel?> GetByIdAsync(int id)
        {
            var data = await _repo.GetByIdAsync(id);
            if (data == null) return null;

            return MapToResponse(data);
        }

        // ✅ GET ALL
        public async Task<List<VehicleSaleBillResponseViewModel>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(MapToResponse).ToList();
        }

        // ✅ UPDATE
        public async Task UpdateAsync(int id, VehicleSaleBillEditCreateViewModel model)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                throw new Exception("Record not found");

            // Update Header
            existing.SaleDate = model.SaleDate;
            existing.SaleBillNo = model.SaleBillNo;
            existing.CustomerName = model.CustomerName;
            existing.TotalAmount = model.TotalAmount;
            existing.UpdatedDate = DateTime.Now;

            // Remove old details
            existing.VehicleSaleBillDetails.Clear();

            // Add new details
            foreach (var d in model.Details)
            {
                existing.VehicleSaleBillDetails.Add(new VehicleSaleBillDetail
                {
                    ChassisNo = d.ChassisNo,
                    ItemRate = d.ItemRate,
                    PreGstDiscount = d.PreGstDiscount,
                    RegAmount = d.RegAmount,
                    InsuranceAmount = d.InsuranceAmount,
                    HasDevice = d.HasDevice,
                    HasKit = d.HasKit,
                    IsDelivered = d.IsDelivered,
                    Segment = d.Segment,
                    InstitutionalType = d.InstitutionalType,
                    SchemeName = d.SchemeName,
                    Narration = d.Narration,
                    FinalAmount = d.FinalAmount,
                    IsAgainstExchange = d.IsAgainstExchange,
                    CreatedDate = DateTime.Now
                });
            }

            await _repo.UpdateAsync(existing);
        }

        // ✅ DELETE
        public async Task DeleteAsync(int id)
        {
            await _repo.DeleteAsync(id);
        }

        // 🔁 Mapping Methods

        private VehicleSaleBillHeader MapToEntity(VehicleSaleBillEditCreateViewModel model)
        {
            return new VehicleSaleBillHeader
            {
                SaleDate = model.SaleDate,
                SaleBillNo = model.SaleBillNo,
                IsD2d = model.IsD2d,
                CustomerType = model.CustomerType,
                Location = model.Location,
                SaleType = model.SaleType,
                CashAccount = model.CashAccount,
                Financier = model.Financier,
                BillType = model.BillType,
                BillFrom = model.BillFrom,
                CustomerName = model.CustomerName,
                BillingName = model.BillingName,
                SalesExecutive = model.SalesExecutive,
                TempRegNo = model.TempRegNo,
                BookingId = model.BookingId,
                PrintType = model.PrintType,
                RefName = model.RefName,
                RefAddress = model.RefAddress,
                RefEmail = model.RefEmail,
                RefPoint = model.RefPoint,
                RefRemarks = model.RefRemarks,
                TotalAmount = model.TotalAmount,
                CreatedDate = DateTime.Now,

                VehicleSaleBillDetails = model.Details.Select(d => new VehicleSaleBillDetail
                {
                    ChassisNo = d.ChassisNo,
                    ItemRate = d.ItemRate,
                    PreGstDiscount = d.PreGstDiscount,
                    RegAmount = d.RegAmount,
                    InsuranceAmount = d.InsuranceAmount,
                    HasDevice = d.HasDevice,
                    HasKit = d.HasKit,
                    IsDelivered = d.IsDelivered,
                    Segment = d.Segment,
                    InstitutionalType = d.InstitutionalType,
                    SchemeName = d.SchemeName,
                    Narration = d.Narration,
                    FinalAmount = d.FinalAmount,
                    IsAgainstExchange = d.IsAgainstExchange,
                    CreatedDate = DateTime.Now,
                    CreatedBy = GetUserInfoFromToken.GetUserIdFromToken(_contextAccessor.HttpContext)
                }).ToList()
            };
        }

        private VehicleSaleBillResponseViewModel MapToResponse(VehicleSaleBillHeader data)
        {
            return new VehicleSaleBillResponseViewModel
            {
                Id = data.Id,
                SaleBillNo = data.SaleBillNo,
                CustomerName = data.CustomerName ?? "",
                TotalAmount = data.TotalAmount ?? 0,

                Details = data.VehicleSaleBillDetails.Select(d => new VehicleSaleBillDetailVM
                {
                    ChassisNo = d.ChassisNo,
                    ItemRate = d.ItemRate,
                    PreGstDiscount = d.PreGstDiscount ?? 0,
                    RegAmount = d.RegAmount ?? 0,
                    InsuranceAmount = d.InsuranceAmount ?? 0,
                    HasDevice = d.HasDevice,
                    HasKit = d.HasKit,
                    IsDelivered = d.IsDelivered,
                    Segment = d.Segment ?? "",
                    InstitutionalType = d.InstitutionalType ?? "",
                    SchemeName = d.SchemeName ?? "",
                    Narration = d.Narration ?? "",
                    FinalAmount = d.FinalAmount,
                    IsAgainstExchange = d.IsAgainstExchange
                }).ToList()
            };
        }
    }
}
