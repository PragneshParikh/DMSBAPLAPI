using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.LineOrderReportRepo;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Utils.ViewModels;

namespace DMS_BAPL_Data.Services.LineOrderReportService
{
    public partial class LineOrderReportService : ILineOrderReportService
    {
        private readonly ILineOrderReportRepo _repo;
        private readonly IExcelService _excelService;

        public LineOrderReportService(ILineOrderReportRepo repo, IExcelService excelService)
        {
            _repo = repo;
            _excelService = excelService;
        }

        private static readonly (string Header, Func<DmsLineOrderReport, object?> Get)[] ExportColumns =
        {
            ("Dealer Name",      x => x.DealerName),
            ("Dealer Code",      x => x.DealerCode),
            ("Unique Id",        x => x.UniqueId),
            ("Loc Code",         x => x.LocCode),
            ("Doc Date",         x => Fmt(x.DocDate)),
            ("Doc No",           x => x.DocNo),
            ("Doc Type",         x => x.DocType),
            ("Job Date",         x => Fmt(x.JobDate)),
            ("Job No",           x => x.JobNo),
            ("Brand",            x => x.BrandName),
            ("Model",            x => x.Model),
            ("Job Card Type",    x => x.JobCardType),
            ("Payment Mode",     x => x.PaymentMode),
            ("Party Name",       x => x.PartyName),
            ("Party Mobile",     x => x.PartyMobile),
            ("Reg No",           x => x.RegNo),
            ("Vehicle Type",     x => x.VehicleType),
            ("Chassis No",       x => x.ChassisNo),
            ("Location",         x => x.Location),
            ("Item Name",        x => x.ItemName),
            ("Item Description", x => x.ItemDescription),
            ("Item Type",        x => x.ItemType),
            ("Qty",              x => x.Qty),
            ("Rate",             x => x.Rate),
            ("Total",            x => x.Total),
            ("SGST %",           x => x.SgstPer),
            ("SGST Amount",      x => x.SgstAmount),
            ("CGST %",           x => x.CgstPer),
            ("CGST Amount",      x => x.CgstAmount),
            ("IGST %",           x => x.IgstPer),
            ("IGST Amount",      x => x.IgstAmount),
            ("Discount",         x => x.Discount),
            ("Total Amount",     x => x.TotalAmount),
            ("MRP",              x => x.Mrp),
            ("Dealer Type",      x => x.DealerType),
        };

        private static string Fmt(DateOnly? d) => d.HasValue ? d.Value.ToString("dd-MM-yyyy") : "";

        private static void Normalize(LineOrderReportFilterModel f)
        {
            if (f.PageIndex < 1) f.PageIndex = 1;
            if (f.PageSize < 1) f.PageSize = 10;
            if (f.PageSize > 100) f.PageSize = 100;
        }

        public async Task<PagedResponse<object>> GetLineOrderReport(LineOrderReportFilterModel filter)
        {
            Normalize(filter);
            return await _repo.GetPagedAsync(filter);
        }

        public async Task<byte[]> DownloadLineOrderExcel(LineOrderReportFilterModel filter)
        {
            var data = await _repo.GetAllAsync(filter);

            var columns = new List<string> { "Sr No" };
            columns.AddRange(ExportColumns.Select(c => c.Header));

            var rows = new List<Dictionary<string, object>>();
            int sr = 1;
            foreach (var item in data)
            {
                var dict = new Dictionary<string, object> { ["Sr No"] = sr++ };
                foreach (var col in ExportColumns)
                    dict[col.Header] = col.Get(item) ?? "";
                rows.Add(dict);
            }

            var model = new ExcelExportViewModel
            {
                //SheetName = filter.LineType == "Part" ? "Part" : "Labour",
                SheetName = filter.LineType?.ToLower() switch
                {
                    "part" => "Part",
                    "labour" => "Labour",
                    _ => "All"
                },
                Columns = columns,
                Rows = rows
            };

            return await _excelService.GenerateExcel(model);
        }
    }
}