using DMS_BAPL_Data.Repositories.LocationMasterRepo;
using DMS_BAPL_Data.Repositories.OEMModelMasterRepo;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Data.ViewModels;
using DMS_BAPL_Utils.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.OEMModelMasterService
{
    public class OEMModelMasterService : IOEMModelMasterService
    {
        private readonly IOEMModelMasterRepo _oemmasterrepo;
        private readonly IExcelService _excelService;
        public OEMModelMasterService(IOEMModelMasterRepo oemmasterrepo, IExcelService excelService)
        {
            _oemmasterrepo = oemmasterrepo;
            _excelService = excelService;
        }
        public async Task<List<OEMModelMasterViewModel>> GetAllOEMModels()
        {
            return await _oemmasterrepo.GetAllOEMModels();
        }

        public async Task<OEMModelMasterViewModel> GetOEMModelById(int id)
        {
            return await _oemmasterrepo.GetOEMModelById(id);
        }

        public async Task<bool> AddOEMModel(OEMModelMasterViewModel oemViewModel)
        {
            return await _oemmasterrepo.AddOEMModel(oemViewModel);
        }

        public async Task<bool> DeleteOEMModel(int id)
        {
            return await _oemmasterrepo.DeleteOEMModel(id);
        }
        public async Task<bool> UpdateOEMModel(OEMModelMasterViewModel oemViewModel)
        {
            return await _oemmasterrepo.UpdateOEMModel(oemViewModel);
        }
        public async Task<byte[]> DownloadOEMModelExcel()
        {
            try
            {
                var data = await _oemmasterrepo.GetAllOEMModels();

                var columns = new List<string>
        {
            "Id",
            "ModelName",
            "ModelShortName",
            "Active"
        };

                var rows = data.Select(d =>
                {
                    var dict = new Dictionary<string, object>();

                    dict["Id"] = d.Id;
                    dict["ModelName"] = d.ModelName;
                    dict["ModelShortName"] = d.ModelShortName;
                    dict["Active"] = d.IsActive ? "ACTIVE" : "INACTIVE";

                    return dict;

                }).ToList();

                var model = new ExcelExportViewModel
                {
                    SheetName = "OEMModelMaster",
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
    }
}
