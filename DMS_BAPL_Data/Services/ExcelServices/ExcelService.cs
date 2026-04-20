using ClosedXML.Excel;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;

public class ExcelService : IExcelService
{
    public async Task<byte[]> GenerateExcel(ExcelExportViewModel model)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(model.SheetName);

        // SLNo column
        model.Columns.Insert(0, StringConstants.SlNo);

        // Add headers
        for (int i = 0; i < model.Columns.Count; i++)
        {
            var headerCell = worksheet.Cell(1, i + 1);
            headerCell.Value = model.Columns[i];
            headerCell.Style.Font.Bold = true;
        }

        // Add rows
        for (int r = 0; r < model.Rows.Count; r++)
        {
            worksheet.Cell(r + 2, 1).Value = r + 1;

            for (int c = 1; c < model.Columns.Count; c++)
            {
                var columnName = model.Columns[c];
                model.Rows[r].TryGetValue(columnName, out var value);

                worksheet.Cell(r + 2, c + 1).Value = value?.ToString();
            }
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return await Task.FromResult(stream.ToArray());
    }
}