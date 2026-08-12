using ClosedXML.Excel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.PartDispatchRepo;
using System.Globalization;

namespace DMS_BAPL_Data.Services.PartDispatchService
{
    public class PartDispatchService : IPartDispatchService
    {
        private readonly IPartDispatchRepo _repo;

        public PartDispatchService(IPartDispatchRepo repo)
        {
            _repo = repo;
        }

        public Task<List<DmsPartDispatch>> GetAllAsync() => _repo.GetAllAsync();

        public Task<DmsPartDispatch?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);

        public Task<DmsPartDispatch> CreateAsync(DmsPartDispatch item, string userId)
            => _repo.CreateAsync(item, userId);

        public Task<DmsPartDispatch?> UpdateAsync(DmsPartDispatch item, string userId)
            => _repo.UpdateAsync(item, userId);

        public Task<bool> DeleteAsync(int id) => _repo.DeleteAsync(id);

        public async Task<int> ImportFromExcelAsync(Stream fileStream, string userId)
        {
            var items = new List<DmsPartDispatch>();

            using var workbook = new XLWorkbook(fileStream);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(1); // skip header row

            // Expected column order:
            // InvoiceDate | InvoiceNo | PartNo | ItemIdno | ItemHsncode | ItemRate |
            // ItemMrp | ItemQty | Sgst | Cgst | Igst | Ugst | ItemDisc |
            // DiscountType | LocCode | VendorIdno | DealerCode

            foreach (var row in rows)
            {
                var item = new DmsPartDispatch
                {
                    InvoiceDate = ParseDate(row.Cell(1).GetString()),
                    InvoiceNo = row.Cell(2).GetString(),
                    PartNo = row.Cell(3).GetString(),
                    ItemIdno = ParseInt(row.Cell(4).GetString()),
                    ItemHsncode = row.Cell(5).GetString(),
                    ItemRate = ParseDecimal(row.Cell(6).GetString()),
                    ItemMrp = ParseDecimal(row.Cell(7).GetString()),
                    ItemQty = ParseInt(row.Cell(8).GetString()),
                    Sgst = ParseDecimal(row.Cell(9).GetString()),
                    Cgst = ParseDecimal(row.Cell(10).GetString()),
                    Igst = ParseDecimal(row.Cell(11).GetString()),
                    Ugst = ParseDecimal(row.Cell(12).GetString()),
                    ItemDisc = ParseDecimal(row.Cell(13).GetString()),
                    DiscountType = row.Cell(14).GetString(),
                    LocCode = row.Cell(15).GetString(),
                    //VendorIdno = ParseInt(row.Cell(16).GetString()),
                    DealerCode = row.Cell(17).GetString()
                };

                items.Add(item);
            }

            return await _repo.ImportAsync(items, userId);
        }

        // implementation
        public async Task<List<DmsPartDispatch>> CreateBulkAsync(List<DmsPartDispatch> items, string userId)
        {
            var results = new List<DmsPartDispatch>();
            foreach (var item in items)
            {
                var created = await _repo.CreateAsync(item, userId);
                results.Add(created);
            }
            return results;
        }

        private static DateTime? ParseDate(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;

            if (DateTime.TryParseExact(value, "dd/MM/yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var dt))
                return dt;

            return DateTime.TryParse(value, out var fallback) ? fallback : null;
        }

        private static int? ParseInt(string value)
            => int.TryParse(value, out var result) ? result : null;

        private static decimal? ParseDecimal(string value)
            => decimal.TryParse(value, out var result) ? result : null;
    }
}