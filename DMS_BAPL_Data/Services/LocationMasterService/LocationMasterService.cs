using DMS_BAPL_Data.Repositories.LocationMasterRepo;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.LocationMasterService
{
    public class LocationMasterService : ILocationMasterService
    {
        private readonly ILocationMasterRepo _repo;
        private readonly IExcelService _excelService;
        public LocationMasterService(ILocationMasterRepo repo , IExcelService excelService)
        {
            _repo = repo;
            _excelService = excelService;
        }
        public async Task<List<LocationMasterViewModel>> GetAllLocationMaster()
        {
            return await _repo.GetAllLocationMaster();
        }
        public async Task<LocationMasterViewModel> GetLocationMasterById(int id)
        {
            return await _repo.GetLocationMasterById(id);
        }

        public async Task<bool> AddLocationMaster(LocationMasterViewModel model)
        {
            return await _repo.AddLocationMaster(model);
        }

        public async Task<bool> UpdateLocationMaster(LocationMasterViewModel model)
        {
            return await _repo.UpdateLocationMaster(model);
        }
        public async Task<byte[]> DownloadLocationMasterExcel()
        {
            try
            {
                var data = await _repo.GetAllLocationMaster();

                var columns = new List<string>
        {
            "Loccode",
            "Locname",
            "LocationArea",
            "Add1",
            "Add2",
            "State",
            "City",
            "Pincode",
            "Gstinno",
            "Email",
            "Mobileno",
            "Contpername1",
            "Contpername2",
            "Contpermob1",
            "Contpermob2",
            "Contperemail1",
            "Contperemail2",
            "Formtype",
            "Dealercode",
            "Rrglocationidno",
            "Active"
        };

                var rows = data.Select(d =>
                {
                    var dict = new Dictionary<string, object>();

                    dict["Loccode"] = d.Loccode;
                    dict["Locname"] = d.Locname;
                    //dict["Locareaidno"] = d.Locareaidno;
                    dict["LocationArea"] = d.Locareaidno == 1 ? "Showroom"
                        : d.Locareaidno == 2 ? "Workshop"
                        : d.Locareaidno == 3 ? "Yard"
                        : "";
                    dict["Add1"] = d.Add1;
                    dict["Add2"] = d.Add2;
                    dict["State"] = d.State;
                    dict["City"] = d.City;
                    dict["Pincode"] = d.Pincode;
                    dict["Gstinno"] = d.Gstinno;
                    dict["Email"] = d.Email;
                    dict["Mobileno"] = d.Mobileno;
                    dict["Contpername1"] = d.Contpername1;
                    dict["Contpername2"] = d.Contpername2;
                    dict["Contpermob1"] = d.Contpermob1;
                    dict["Contpermob2"] = d.Contpermob2;
                    dict["Contperemail1"] = d.Contperemail1;
                    dict["Contperemail2"] = d.Contperemail2;
                    dict["Formtype"] = d.Formtype;
                    dict["Dealercode"] = d.Dealercode;
                    dict["Rrglocationidno"] = d.Rrglocationidno;
                    dict["Active"] = d.Active;

                    return dict;
                }).ToList();

                var model = new ExcelExportViewModel
                {
                    SheetName = "LocationMaster",
                    Columns = columns,
                    Rows = rows
                };

                return await _excelService.GenerateExcel(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<List<LocationNameViewModel>> GetLocationByDealerCode(string dealerCode)
        {
          return  await _repo.GetLocationByDealerCode(dealerCode);

        }
    }
}
