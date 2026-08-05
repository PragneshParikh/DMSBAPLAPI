using ClosedXML.Excel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.PartDispWarrantyRepo;

namespace DMS_BAPL_Data.Services.PartDispWarrantyService
{
    public class PartDispWarrantyService : IPartDispWarrantyService
    {
        private readonly IPartDispWarrantyRepo _repo;

        public PartDispWarrantyService(IPartDispWarrantyRepo repo)
        {
            _repo = repo;
        }

        public Task<List<ZDmsPartDispWarranty>> GetAllAsync() => _repo.GetAllAsync();

        public Task<ZDmsPartDispWarranty?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);

        public Task<ZDmsPartDispWarranty> CreateAsync(ZDmsPartDispWarranty item, string userId)
            => _repo.CreateAsync(item, userId);

        public Task<ZDmsPartDispWarranty?> UpdateAsync(ZDmsPartDispWarranty item, string userId)
            => _repo.UpdateAsync(item, userId);

        public Task<bool> DeleteAsync(int id) => _repo.DeleteAsync(id);

        public async Task<int> ImportFromExcelAsync(Stream fileStream, string userId)
        {
            var items = new List<ZDmsPartDispWarranty>();

            using var workbook = new XLWorkbook(fileStream);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(1); // skip header row

            // Expected column order:
            // InvoiceDate | InvoiceNo | InvoiceType | ChassisNumber | ItemCode |
            // SerialNo | VendorId | DealerCode | DeviceType | ItemQty | LotNo |
            // MfgDate | InvoiceItemCode | LineNo

            foreach (var row in rows)
            {
                var item = new ZDmsPartDispWarranty
                {
                    Invoicedate = ParseDate(row.Cell(1).GetString()),
                    Invoiceno = row.Cell(2).GetString(),
                    Invoicetype = row.Cell(3).GetString(),
                    Chassisnumber = row.Cell(4).GetString(),
                    Itemcode = row.Cell(5).GetString(),
                    Serialno = row.Cell(6).GetString(),
                    Vendorid = int.TryParse(row.Cell(7).GetString(), out var vid) ? vid : (int?)null,
                    Dealercode = row.Cell(8).GetString(),
                    Devicetype = row.Cell(9).GetString(),
                    Itemqty = int.TryParse(row.Cell(10).GetString(), out var qty) ? qty : (int?)null,
                    Lotno = row.Cell(11).GetString(),
                    Mfgdate = ParseDate(row.Cell(12).GetString()),
                    Invoiceitemcode = row.Cell(13).GetString(),
                    Lineno = int.TryParse(row.Cell(14).GetString(), out var ln) ? ln : (int?)null
                };

                items.Add(item);
            }

            return await _repo.ImportAsync(items, userId);
        }

        private static DateTime? ParseDate(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;

            if (DateTime.TryParseExact(value, "dd/MM/yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var dt))
                return dt;

            return DateTime.TryParse(value, out var fallback) ? fallback : null;
        }
    }
}