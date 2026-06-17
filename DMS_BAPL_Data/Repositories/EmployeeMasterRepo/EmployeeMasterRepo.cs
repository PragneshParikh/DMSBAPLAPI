using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.EmployeeMasterRepo
{
    public partial class EmployeeMasterRepo : IEmployeeMasterRepo
    {
        private readonly BapldmsvadContext _context;

        public EmployeeMasterRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        async Task<IEnumerable<EmployeeMaster>> IEmployeeMasterRepo.Get()
        {
            try
            {
                return await Task.FromResult(_context.EmployeeMasters.ToList());
            }
            catch { throw; }
        }

        async Task<EmployeeMaster?> IEmployeeMasterRepo.GetEmployeeById(int id)
        {
            try
            {
                return await _context.EmployeeMasters
                    .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch { throw; }
        }

        async Task<int> IEmployeeMasterRepo.CreateNewUser(EmployeeMaster employeeMaster)
        {
            try
            {
                _context.EmployeeMasters.Add(employeeMaster);
                return await _context.SaveChangesAsync();
            }
            catch { throw; }
        }
        async Task<int> IEmployeeMasterRepo.UpdateEmployee(EmployeeMaster employeeMaster)
        {
            try
            {
                var existingEmployee = await _context.EmployeeMasters
                    .FirstOrDefaultAsync(x => x.Id == employeeMaster.Id);

                if (existingEmployee == null)
                    return 0;

                existingEmployee.EmployeeCode = employeeMaster.EmployeeCode;
                existingEmployee.FirstName = employeeMaster.FirstName;
                existingEmployee.LastName = employeeMaster.LastName;
                existingEmployee.Gender = employeeMaster.Gender;
                existingEmployee.Mobile = employeeMaster.Mobile;
                existingEmployee.EmailId = employeeMaster.EmailId;
                existingEmployee.Password = employeeMaster.Password;
                existingEmployee.Address = employeeMaster.Address;
                existingEmployee.State = employeeMaster.State;
                existingEmployee.City = employeeMaster.City;
                existingEmployee.Pincode = employeeMaster.Pincode;
                existingEmployee.DateOfJoin = employeeMaster.DateOfJoin;
                existingEmployee.Designation = employeeMaster.Designation;
                existingEmployee.Department = employeeMaster.Department;
                existingEmployee.DealerCode = employeeMaster.DealerCode;
                existingEmployee.Supervisor = employeeMaster.Supervisor;
                existingEmployee.IsActive = employeeMaster.IsActive;
                existingEmployee.ProfileImage = employeeMaster.ProfileImage;
                existingEmployee.Notes = employeeMaster.Notes;
                existingEmployee.UpdatedBy = "admin";
                existingEmployee.UpdatedDate = DateTime.Now;

                return await _context.SaveChangesAsync();
            }
            catch
            {
                throw;
            }
        }

        //async Task<object?> IEmployeeMasterRepo.GetDealerByCode(string dealerCode)
        //{
        //    try
        //    {
        //        return await _context.DealerMasters
        //            .AsNoTracking()
        //            .Where(x => x.Dealercode == dealerCode)
        //            .Select(x => new
        //            {
        //                dealerCode = x.Dealercode,
        //                dealerName = x.Compname,
        //                isActive = x.IsActive
        //            })
        //            .FirstOrDefaultAsync();
        //    }
        //    catch { throw; }
        //}

        //async Task<List<object>> IEmployeeMasterRepo.GetLocationsByDealer(string dealerCode)
        //{
        //    try
        //    {
        //        return await _context.LocationMasters
        //            .AsNoTracking()
        //            .Where(x => x.Dealercode == dealerCode)  
        //            .Select(x => new
        //            {
        //                locCode = x.Loccode,
        //                locName = x.Locname
        //            })
        //            .ToListAsync<object>();
        async Task<List<EmployeeDesignationWiseViewModel>> IEmployeeMasterRepo.GetEmployeesByDesignation(string? dealerCode, string designation)
        {
            try
            {
                var result = await _context.EmployeeMasters
                    .Where(e => e.DealerCode == dealerCode && e.Designation.ToLower() == designation.ToLower() && e.IsActive)
                    .Select(e=> new EmployeeDesignationWiseViewModel
                    {
                        EmployeeCode = e.EmployeeCode,
                        Designation = e.Designation,
                        Department = e.Department,
                        FirstName = e.FirstName,
                        LastName = e.LastName,
                        DealerCode = e.DealerCode,
                        LocationCode = e.LocationCode,
                    })
                    .ToListAsync();
                return result;
            }
            catch { throw; }
        }
    }
}
