using ClosedXML.Excel;
using DMS_BAPL_Data.DBModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.ChassisRepo
{
    public partial class ChassisRepo : IChassisRepo
    {
        private readonly BapldmsvadContext _context;
        public ChassisRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        //public async Task<object> GetChassisDataAsync(string chassisNumber)
        //{
        //    var result = await (from v in _context.VehicleInwards
        //                        join j in _context.JobCardHeaders on v.ChasisNo equals j.Chassisno into cv
        //                        from j in cv.DefaultIfEmpty()
        //                        join c in _context.JobCardCustomers on v.Id equals c.Id into cc
        //                        from c in cc.DefaultIfEmpty()
        //                        join i in _context.ItemMasters on v.ItemCode equals i.Itemcode

        //                        where v.ChasisNo == chassisNumber
        //                        select new
        //                        {
        //                            v.ChasisNo,
        //                            v.MotorNo,
        //                            v.InvoiceNo,
        //                            v.InvoiceDate,
        //                            v.DealerCode,
        //                            v.ColrCode,
        //                            j.IsPdiSuccess,
        //                            c.CustomerName,
        //                            c.SaleDate,
        //                            i.Itemname
        //                        }).FirstOrDefaultAsync();
        //    return result;
        //}

        public async Task<object> GetChassisDataAsync(string chassisNumber)
        {
            var result = await (from cd in _context.ChassisDetails
                                join j in _context.JobCardHeaders on cd.ChassisNo equals j.Chassisno into cv
                                join vi in _context.VehicleInwards on cd.ChassisNo equals vi.ChasisNo into viInfo
                                from vi in viInfo.DefaultIfEmpty()
                                join cb in _context.ChassisBatteryDetails on cd.ChassisNo equals cb.ChassisNo
                                from j in cv.DefaultIfEmpty()
                                join c in _context.JobCardCustomers on cd.Id equals c.Id into cc
                                from c in cc.DefaultIfEmpty()
                                join i in _context.ItemMasters on cd.ItemCode equals i.Itemcode
                                join cl in _context.ColorMasters on i.Colorcode equals cl.Colorcode into itemClr
                                from cl in itemClr.DefaultIfEmpty()
                                join sd in _context.VehicleSaleBillDetails on cd.ChassisNo equals sd.ChassisNo into saleInfo
                                from sd in saleInfo.DefaultIfEmpty()
                                join sh in _context.VehicleSaleBillHeaders on sd.VehicleSaleBillId equals sh.Id into SHInfo
                                from sh in SHInfo.DefaultIfEmpty()
                                join lc in _context.LocationMasters on vi.LocCode equals lc.Loccode into locInfo
                                from lc in locInfo.DefaultIfEmpty()
                                join cust in _context.LedgerMasters on sh.LedgerId equals cust.Id into custInfo
                                from cust in custInfo.DefaultIfEmpty()


                                where cd.ChassisNo == chassisNumber
                                select new
                                {
                                    cd.ChassisNo,
                                    cb.MotorNo,
                                    cd.DealerId,
                                    j.IsPdiSuccess,
                                    PDIStatus = j == null? "PDI Pending": j.IsPdiSuccess.GetValueOrDefault()? "YES": "NO",
                                    c.CustomerName,
                                    c.SaleDate,
                                    i.Itemname,
                                    i.Colorcode,
                                    cl.Colorname,
                                    lc.Locname,
                                    cust,
                                    sh,
                                    vi


                                }).FirstOrDefaultAsync();
            return result;
        }

        public async Task<string> ImportChassisExcelAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return "Invalid file";

                using var stream =
                    new MemoryStream();

                await file.CopyToAsync(stream);

                using var workbook =
                    new XLWorkbook(stream);

                var worksheet =
                    workbook.Worksheet(1);

                var rows =
                    worksheet.RowsUsed().Skip(1);

                foreach (var row in rows)
                {
                    var chassisNo =
                        row.Cell(1).GetString().Trim();

                    if (string.IsNullOrEmpty(chassisNo))
                        continue;

                    var vehicle =
                        await _context.VehicleInwards
                            .FirstOrDefaultAsync(x =>
                                x.ChasisNo == chassisNo);

                    if (vehicle == null)
                        continue;

                    vehicle.BatteryNo =
                        row.Cell(2).GetString();

                    vehicle.MotorNo =
                        row.Cell(3).GetString();

                    vehicle.ChargerNo =
                        row.Cell(4).GetString();

                    vehicle.ControllerNo =
                        row.Cell(5).GetString();

                    vehicle.Converter =
                        row.Cell(6).GetString();

                    vehicle.Vcu =
                        row.Cell(7).GetString();

                    vehicle.BatteryCapacity =
                        row.Cell(8).GetString();

                    vehicle.BatteryChemistry =
                        row.Cell(9).GetString();

                    vehicle.BatteryMake =
                        row.Cell(10).GetString();
                }

                await _context.SaveChangesAsync();

                return "Excel imported successfully";
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Error importing chassis excel",
                    ex);
            }
        }


    }
}
