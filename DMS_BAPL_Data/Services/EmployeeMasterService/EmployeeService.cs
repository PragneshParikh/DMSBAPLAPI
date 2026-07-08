using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.EmployeeMasterRepo;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.EmployeeMasterService
{
    public partial class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeMasterRepo _employeeMasterRepo;
        public EmployeeService(IEmployeeMasterRepo employeeMasterRepo)
        {
            _employeeMasterRepo = employeeMasterRepo;
        }
        Task<IEnumerable<EmployeeMaster>> IEmployeeService.Get() => _employeeMasterRepo.Get();
        Task<EmployeeMaster?> IEmployeeService.GetEmployeeById(int id) => _employeeMasterRepo.GetEmployeeById(id);
        Task<int> IEmployeeService.CreateNewUser(EmployeeMaster employeeMaster) => _employeeMasterRepo.CreateNewUser(employeeMaster);
        Task<int> IEmployeeService.UpdateEmployee(EmployeeMaster employeeMaster) => _employeeMasterRepo.UpdateEmployee(employeeMaster);
        Task<List<EmployeeDesignationWiseViewModel>> IEmployeeService.GetEmployeesByDesignation(string? dealerCode, string designation) => _employeeMasterRepo.GetEmployeesByDesignation(dealerCode, designation);
        Task<EmployeeMaster?> IEmployeeService.GetEmployeeByEmail(string email) => _employeeMasterRepo.GetEmployeeByEmail(email);

        Task<IEnumerable<EmployeeRoleMapping>> IEmployeeService.GetRoleMappings(int employeeId)
            => _employeeMasterRepo.GetRoleMappings(employeeId);

        async Task<byte[]> IEmployeeService.DownloadEmployeeExcel(string? dealerCode)
        {
            var employees = (await _employeeMasterRepo.Get()).ToList();

            if (!string.IsNullOrWhiteSpace(dealerCode))
            {
                employees = employees
                    .Where(e => string.Equals(e.DealerCode?.Trim(), dealerCode.Trim(), StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            using var stream = new MemoryStream();

            using (var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                var workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                var sheetData = new SheetData();
                worksheetPart.Worksheet = new Worksheet(sheetData);

                var sheets = workbookPart.Workbook.AppendChild(new Sheets());
                sheets.Append(new Sheet
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "Employees"
                });

                // Header row
                var headerRow = new Row();
                string[] headers =
                {
                    "Employee Code", "First Name", "Last Name", "Gender", "Mobile",
                    "Email", "Department Id", "Designation Id", "Dealer Code",
                    "Location Code", "Date Of Join", "Status"
                };
                foreach (var h in headers)
                    headerRow.Append(CreateTextCell(h));
                sheetData.Append(headerRow);

                // Data rows
                foreach (var e in employees)
                {
                    var row = new Row();
                    row.Append(CreateTextCell(e.EmployeeCode));
                    row.Append(CreateTextCell(e.FirstName));
                    row.Append(CreateTextCell(e.LastName));
                    row.Append(CreateTextCell(e.Gender));
                    row.Append(CreateTextCell(e.Mobile));
                    row.Append(CreateTextCell(e.EmailId));
                    row.Append(CreateTextCell(e.Department));
                    row.Append(CreateTextCell(e.Designation));
                    row.Append(CreateTextCell(e.DealerCode));
                    row.Append(CreateTextCell(e.LocationCode));
                    row.Append(CreateTextCell(e.DateOfJoin.ToString("yyyy-MM-dd")));
                    row.Append(CreateTextCell(e.IsActive ? "Active" : "Inactive"));
                    sheetData.Append(row);
                }

                workbookPart.Workbook.Save();
            }

            return stream.ToArray();
        }

        private static Cell CreateTextCell(string? value)
        {
            return new Cell
            {
                DataType = CellValues.String,
                CellValue = new CellValue(value ?? string.Empty)
            };
        }
    }
}