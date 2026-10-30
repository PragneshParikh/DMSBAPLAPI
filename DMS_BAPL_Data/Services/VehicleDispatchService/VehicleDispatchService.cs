using DMS_BAPL_Data.Helpers;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.VehicleDispatchService
{
    /// <summary>
    /// Vehicle Dispatch (OEM invoice to dealer) = VehicleInward, one row per chassis.
    ///
    /// Import: Cogxim "VehicleDispatchReport2021-cogxi ... 2027" Excel (one sheet per year).
    ///  * 1 Excel row = 1 chassis. Imported rows get CreatedBy "Cogxim Import" so they can always be identified.
    ///  * Only VehicleInward is written. ChassisDetails, ChassisBatteryDetail, lot inspection, stock
    ///    adjustment and ERP are not touched - this is historical data.
    ///  * A chassis that already has a VehicleInward row created by DMS / ERP is skipped (never changed).
    ///    A chassis from an earlier Cogxim import is refreshed when UpdateExisting is on.
    ///  * Same chassis twice in a sheet: the row with the latest Invoice Date is kept.
    ///  * Re-running the same file is safe. A real import runs in one transaction: all or nothing.
    /// </summary>
    public class VehicleDispatchService : IVehicleDispatchService
    {
        public const string ImportTag = "Cogxim Import";
        private const int MaxIssues = 1000;
        private const int SaveChunk = 500;
        private const string Info = "Info";

        private readonly BapldmsvadContext _context;
        private readonly ILogger<VehicleDispatchService> _logger;

        public VehicleDispatchService(BapldmsvadContext context, ILogger<VehicleDispatchService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ═════════════════════════════════════════════════════════════════════
        // COLUMNS of the Cogxim dispatch report (header text compared after
        // lower-casing and removing everything that is not a letter / digit).
        // First alias found in the sheet wins.
        // ═════════════════════════════════════════════════════════════════════
        private static readonly Dictionary<string, string[]> Columns = new()
        {
            ["invoiceDate"] = new[] { "invoicedate", "invdate", "dispatchdate" },
            ["invoiceNo"] = new[] { "invoicno", "invoiceno", "invno", "invoicenumber" },
            ["receiveDate"] = new[] { "dealerinvoicereceivedate", "invoicereceivedate", "receivedate" },
            ["dealerCode"] = new[] { "dealercode", "dlrcode" },
            ["dealerName"] = new[] { "dealername" },
            ["locCode"] = new[] { "loccode", "locationcode" },
            ["location"] = new[] { "location", "locationname" },
            ["mfgMonth"] = new[] { "mfgmonth" },
            ["mfgYear"] = new[] { "mfgyear" },
            ["modelCode"] = new[] { "modelcode", "itemcode" },
            ["itemName"] = new[] { "itemname", "modelname" },
            ["colorCode"] = new[] { "colorcode", "colourcode", "colrcode" },
            ["chassisNo"] = new[] { "chasisno", "chassisno", "vinno", "vin" },
            ["motorNo"] = new[] { "motorno" },
            ["batteryId"] = new[] { "batteryid" },
            ["batteryNo"] = new[] { "batteryno", "batteryno1" },
            ["batteryNo2"] = new[] { "batteryno2" },
            ["batteryCapacity"] = new[] { "batterycapacity" },
            ["batteryChemistry"] = new[] { "batterychemistry", "batterychemical" },
            ["batteryMake"] = new[] { "batterymake" },
            ["ecuSerno"] = new[] { "ecuserialno", "ecuserno" },
            ["ecuImei"] = new[] { "ecuimei" },
            ["ecuBalMac"] = new[] { "ecubalmac" },
            ["immobilizerNo"] = new[] { "immoblizerno", "immobilizerno" },
            ["bikeSimId"] = new[] { "bikesimid" },
            ["bikeMobileNo"] = new[] { "bikemobileno" },
            ["chargerNo"] = new[] { "chargerno1", "chargerno" },
            ["controllerNo"] = new[] { "controllerno" },
            ["converterNo"] = new[] { "converterno", "converter", "convertorno" },
            ["soundbarSerno"] = new[] { "soundbarserialno", "soundbarserno" },
            ["soundbarBalMac"] = new[] { "soundbarbalmac" },
            ["voltage"] = new[] { "voltage" },
            ["vcu"] = new[] { "vcu" },
            ["regNumber"] = new[] { "regnumber", "regno" },
            ["startDate"] = new[] { "startdate" },
            ["tyre1"] = new[] { "tyre1", "tyreno1" },
            ["tyre2"] = new[] { "tyre2", "tyreno2" },
            ["poNumber"] = new[] { "ponumber", "pono" },
        };

        private static readonly (string Key, string Label)[] Required =
        {
            ("invoiceDate", "Invoice Date"), ("invoiceNo", "Invoice No"), ("dealerCode", "Dealer Code"), ("chassisNo", "Chasis No")
        };

        // ═════════════════════════════════════════════════════════════════════
        // SHEETS
        // ═════════════════════════════════════════════════════════════════════
        public DispatchSheetsResponse ReadSheets(Stream excel, string fileName)
        {
            using var wb = OpenWorkbook(excel);
            var response = new DispatchSheetsResponse { FileName = fileName };
            foreach (var ws in wb.Worksheets)
            {
                var info = new DispatchSheetInfo { Name = ws.Name, Rows = Math.Max(0, ws.CountRowsFast() - 1) };
                var cols = ResolveColumns(ws);
                info.MissingColumns = Required.Where(r => !cols.ContainsKey(r.Key)).Select(r => r.Label).ToList();
                response.Sheets.Add(info);
            }
            return response;
        }

        // ═════════════════════════════════════════════════════════════════════
        // IMPORT (dry run or real)
        // ═════════════════════════════════════════════════════════════════════
        public async Task<VehicleDispatchImportResult> ImportAsync(Stream excel, string fileName, VehicleDispatchImportOptions options, string userId)
        {
            options ??= new VehicleDispatchImportOptions();
            using var wb = OpenWorkbook(excel);
            var ws = PickSheet(wb, options.SheetName);
            var col = ResolveColumns(ws);
            var missing = Required.Where(r => !col.ContainsKey(r.Key)).Select(r => r.Label).ToList();
            if (missing.Count > 0)
                throw new ArgumentException($"Sheet \"{ws.Name}\" has no column for: {string.Join(", ", missing)}.");

            var result = new VehicleDispatchImportResult { DryRun = options.DryRun, FileName = fileName, SheetName = ws.Name };
            var issues = new List<DispatchImportIssue>();
            var warningCount = 0;
            void Issue(Row? r, string level, string msg, int rowNo = 0)
            {
                if (level == "Warning") warningCount++;
                issues.Add(new DispatchImportIssue { RowNo = r?.RowNo ?? rowNo, InvoiceNo = r?.InvoiceNo, ChassisNo = r?.ChassisNo, Level = level, Message = msg });
            }

            // ---------- 1. read every row ----------
            var rows = new List<Row>();
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            for (var n = 2; n <= lastRow; n++)
            {
                var xr = ws.Row(n);
                if (xr.IsEmpty()) continue;
                rows.Add(ParseRow(xr, n, col));
            }
            result.TotalRows = rows.Count;

            // ---------- 2. reference data (only what the sheet needs) ----------
            var dealerCodes = rows.Select(r => r.DealerCode).Where(c => c != null).Select(c => c!.ToUpperInvariant()).Distinct().ToList();
            var dealers = (await _context.DealerMasters.AsNoTracking()
                    .Where(d => dealerCodes.Contains(d.Dealercode))
                    .Select(d => new { d.Dealercode, d.Compname })
                    .ToListAsync())
                .GroupBy(d => d.Dealercode.Trim().ToUpperInvariant())
                .ToDictionary(g => g.Key, g => g.First().Compname);

            var locations = (await _context.LocationMasters.AsNoTracking()
                    .Where(l => dealerCodes.Contains(l.Dealercode))
                    .Select(l => new { l.Loccode, l.Dealercode, l.Locname, l.Locareaidno })
                    .ToListAsync())
                .Select(l => (Code: l.Loccode.Trim(), Dealer: l.Dealercode.Trim().ToUpperInvariant(), Name: Norm(l.Locname), Area: l.Locareaidno))
                .ToList();

            var modelCodes = rows.Select(r => r.ModelCode).Where(c => c != null).Select(c => c!).Distinct().ToList();
            var itemNames = rows.Select(r => r.ItemName).Where(c => c != null).Select(c => c!).Distinct().ToList();
            var items = await _context.ItemMasters.AsNoTracking()
                .Where(i => modelCodes.Contains(i.Itemcode) || itemNames.Contains(i.Itemname))
                .Select(i => new { i.Itemcode, i.Itemname })
                .ToListAsync();
            var itemByCode = items.GroupBy(i => i.Itemcode.Trim().ToUpperInvariant()).ToDictionary(g => g.Key, g => g.First().Itemcode.Trim());
            var itemByName = items.GroupBy(i => Norm(i.Itemname)).ToDictionary(g => g.Key, g => g.First().Itemcode.Trim());

            // ---------- 3. validate rows ----------
            foreach (var r in rows)
            {
                if (r.ChassisNo == null) r.Errors.Add("Chasis No is blank.");
                if (r.InvoiceNo == null) r.Errors.Add("Invoice No is blank.");
                if (r.InvoiceDate == null) r.Errors.Add(r.InvoiceDateRaw == null ? "Invoice Date is blank." : $"Invoice Date \"{r.InvoiceDateRaw}\" is not a valid date.");
                if (r.DealerCode == null) r.Errors.Add("Dealer Code is blank.");
                else if (!dealers.ContainsKey(r.DealerCode.ToUpperInvariant()))
                    r.Errors.Add($"Dealer {r.DealerCode} is not in Dealer Master - import the dealer first.");

                if (r.Errors.Count > 0) continue;
                r.DealerCode = r.DealerCode!.ToUpperInvariant();

                // model: Item Master code first, then the item name
                if (r.ModelCode != null && itemByCode.TryGetValue(r.ModelCode.ToUpperInvariant(), out var code)) r.ItemCode = code;
                else if (r.ItemName != null && itemByName.TryGetValue(Norm(r.ItemName), out var byName))
                {
                    r.ItemCode = byName;
                    if (r.ModelCode != null) Issue(r, Info, $"Model Code \"{r.ModelCode}\" not in Item Master - matched by Item Name \"{r.ItemName}\" ({byName}).");
                }
                else
                {
                    r.ItemCode = r.ModelCode;
                    Issue(r, "Warning", $"Model \"{r.ModelCode ?? r.ItemName ?? "(blank)"}\" is not in Item Master - saved as it is; the report shows no model name for it.");
                }

                // location: exact Loc Code of this dealer, then by name, then the dealer's showroom
                var mine = locations.Where(l => l.Dealer == r.DealerCode).ToList();
                var loc = r.LocCode != null ? mine.FirstOrDefault(l => l.Code.Equals(r.LocCode, StringComparison.OrdinalIgnoreCase)).Code : null;
                if (loc == null && r.Location != null) loc = mine.FirstOrDefault(l => l.Name == Norm(r.Location)).Code;
                if (loc == null)
                {
                    loc = mine.OrderBy(l => l.Area == 1 ? 0 : 1).ThenBy(l => l.Code).Select(l => l.Code).FirstOrDefault();
                    if (loc != null) { if (r.LocCode != null && !r.LocCode.Equals(r.DealerCode, StringComparison.OrdinalIgnoreCase)) Issue(r, Info, $"Loc Code \"{r.LocCode}\" not in Location Master - saved under {loc}."); }
                    else
                    {
                        loc = r.LocCode ?? r.DealerCode;
                        Issue(r, "Warning", $"Dealer {r.DealerCode} has no location in Location Master - Loc Code saved as \"{loc}\".");
                    }
                }
                r.FinalLocCode = loc;
            }

            // ---------- 4. same chassis twice in the sheet: keep the latest invoice ----------
            var valid = rows.Where(r => r.Errors.Count == 0).ToList();
            foreach (var g in valid.GroupBy(r => r.ChassisNo!.ToUpperInvariant()).Where(g => g.Count() > 1))
            {
                var keep = g.OrderByDescending(r => r.InvoiceDate).ThenByDescending(r => r.RowNo).First();
                foreach (var r in g.Where(r => r != keep))
                {
                    r.Skip = true;
                    Issue(r, "Skipped", $"Chassis is in this sheet again at row {keep.RowNo} (invoice {keep.InvoiceNo} dated {keep.InvoiceDate:dd.MM.yyyy}) - that later dispatch is kept.");
                }
            }
            valid = valid.Where(r => !r.Skip).ToList();

            // ---------- 5. already in VehicleInward ----------
            var chassisList = valid.Select(r => r.ChassisNo!).Distinct().ToList();
            var existing = new List<ExistingInward>();
            foreach (var chunk in chassisList.Chunk(1000))
            {
                var part = chunk.ToList();
                existing.AddRange(await _context.VehicleInwards.AsNoTracking()
                    .Where(v => v.ChasisNo != null && part.Contains(v.ChasisNo))
                    .Select(v => new ExistingInward(v.Id, v.ChasisNo!, v.CreatedBy, v.InvoiceNo, v.InvoiceDate, v.DealerCode))
                    .ToListAsync());
            }
            var existingByChassis = existing.GroupBy(e => e.ChassisNo.Trim().ToUpperInvariant()).ToDictionary(g => g.Key, g => g.ToList());

            var toInsert = new List<Row>();
            var toUpdate = new List<Row>();
            foreach (var r in valid)
            {
                if (!existingByChassis.TryGetValue(r.ChassisNo!.ToUpperInvariant(), out var found)) { toInsert.Add(r); continue; }
                var dms = found.FirstOrDefault(e => e.CreatedBy != ImportTag);
                if (dms != null)
                {
                    r.Skip = true;
                    Issue(r, "Skipped", $"Chassis already dispatched in DMS (invoice {dms.InvoiceNo ?? "-"} dated {dms.InvoiceDate:dd.MM.yyyy}, dealer {dms.DealerCode ?? "-"}) - not changed.");
                    continue;
                }
                if (!options.UpdateExisting)
                {
                    r.Skip = true;
                    Issue(r, "Skipped", "Chassis already imported from Cogxim (switch \"Update chassis already imported\" on to refresh it).");
                    continue;
                }
                r.ExistingId = found.OrderByDescending(e => e.Id).First().Id;
                toUpdate.Add(r);
            }

            // ---------- 6. counts ----------
            var work = toInsert.Concat(toUpdate).ToList();
            result.InvoicesInFile = rows.Where(r => r.InvoiceNo != null).Select(r => (r.DealerCode ?? "") + "|" + r.InvoiceNo).Distinct().Count();
            result.RowsWithErrors = rows.Count(r => r.Errors.Count > 0);
            result.RowsSkipped = rows.Count(r => r.Skip);
            result.FromDate = work.Min(r => r.InvoiceDate);
            result.ToDate = work.Max(r => r.InvoiceDate);
            result.Dealers = work.GroupBy(r => r.DealerCode!)
                .Select(g => new DispatchDealerSummary
                {
                    DealerCode = g.Key,
                    DealerName = dealers.TryGetValue(g.Key, out var nm) ? nm : null,
                    Invoices = g.Select(r => r.InvoiceNo).Distinct().Count(),
                    Vehicles = g.Count()
                })
                .OrderByDescending(d => d.Vehicles).ToList();
            foreach (var r in rows.Where(r => r.Errors.Count > 0))
                foreach (var e in r.Errors) Issue(r, "Error", e);

            if (options.DryRun)
            {
                result.VehiclesReady = toInsert.Count;
                result.VehiclesToUpdate = toUpdate.Count;
            }
            else if (work.Count > 0)
            {
                // ---------- 7. save (one transaction: all or nothing) ----------
                var lengths = await LoadColumnLengthsAsync();
                var now = DateTime.Now;
                await using var tx = await _context.Database.BeginTransactionAsync();
                try
                {
                    foreach (var chunk in toInsert.Chunk(SaveChunk))
                    {
                        foreach (var r in chunk)
                        {
                            var v = new VehicleInward { CreatedBy = ImportTag, CreatedDate = now, UpdatedBy = Cut(userId, 50), UpdatedDate = now, IsD2d = false };
                            Apply(v, r, options);
                            Fit(v, lengths, m => Issue(r, "Warning", m));
                            _context.VehicleInwards.Add(v);
                        }
                        await _context.SaveChangesAsync();
                        _context.ChangeTracker.Clear();
                    }
                    foreach (var chunk in toUpdate.Chunk(SaveChunk))
                    {
                        var ids = chunk.Select(r => r.ExistingId!.Value).ToList();
                        var tracked = await _context.VehicleInwards.Where(v => ids.Contains(v.Id)).ToDictionaryAsync(v => v.Id);
                        foreach (var r in chunk)
                        {
                            if (!tracked.TryGetValue(r.ExistingId!.Value, out var v)) continue;
                            Apply(v, r, options);
                            v.UpdatedBy = Cut(userId, 50);
                            v.UpdatedDate = now;
                            Fit(v, lengths, m => Issue(r, "Warning", m));
                        }
                        await _context.SaveChangesAsync();
                        _context.ChangeTracker.Clear();
                    }
                    await tx.CommitAsync();
                }
                catch
                {
                    await tx.RollbackAsync();
                    _context.ChangeTracker.Clear();
                    throw;
                }
                result.VehiclesImported = toInsert.Count;
                result.VehiclesUpdated = toUpdate.Count;
                _logger.LogInformation("Vehicle dispatch import {File} / {Sheet}: {Ins} inserted, {Upd} updated by {User}",
                    fileName, ws.Name, toInsert.Count, toUpdate.Count, userId);
            }

            result.Warnings = warningCount;
            var order = new Dictionary<string, int> { ["Error"] = 0, ["Skipped"] = 1, ["Warning"] = 2, [Info] = 3 };
            var sorted = issues.OrderBy(i => order.TryGetValue(i.Level, out var o) ? o : 9).ThenBy(i => i.RowNo).ToList();
            result.IssuesTruncated = sorted.Count > MaxIssues;
            result.Issues = sorted.Take(MaxIssues).ToList();
            return result;
        }

        private static void Apply(VehicleInward v, Row r, VehicleDispatchImportOptions options)
        {
            v.InvoiceDate = DateOnly.FromDateTime(r.InvoiceDate!.Value);
            v.InvoiceNo = r.InvoiceNo;
            v.DealerCode = r.DealerCode;
            v.LocCode = r.FinalLocCode;
            v.ChasisNo = r.ChassisNo;
            v.ItemCode = r.ItemCode;
            v.ColrCode = r.ColorCode;
            v.MfgYear = r.MfgYear;
            v.MfgMonth = r.MfgMonth;
            v.MotorNo = r.MotorNo;
            v.BatteryId = r.BatteryId;
            v.BatteryNo = r.BatteryNo;
            v.BatteryNo2 = r.BatteryNo2;
            v.BatteryCapacity = r.BatteryCapacity;
            v.BatteryChemistry = r.BatteryChemistry;
            v.BatteryMake = r.BatteryMake;
            v.EcuSerno = r.EcuSerno;
            v.EcuImEi = r.EcuImei;
            v.EcuBalMac = r.EcuBalMac;
            v.ImmoblizerNo = r.ImmobilizerNo;
            v.BikeSimid = r.BikeSimId;
            v.BikeMobileno = r.BikeMobileNo;
            v.ChargerNo = r.ChargerNo;
            v.ControllerNo = r.ControllerNo;
            v.Converter = r.ConverterNo;
            v.SoundbarSerno = r.SoundbarSerno;
            v.SoundbarBalMac = r.SoundbarBalMac;
            v.Voltage = r.Voltage;
            v.Vcu = r.Vcu;
            v.Regnumber = r.RegNumber;
            v.Startdate = r.StartDate;
            v.TyreNo1 = r.Tyre1;
            v.TyreNo2 = r.Tyre2;
            v.Ponumber = r.PoNumber;
            // historic dispatch = received by the dealer; otherwise only when Cogxim has a receive date
            v.IsAccepted = options.MarkAsAccepted || r.ReceiveDate != null;
        }

        // ═════════════════════════════════════════════════════════════════════
        // REPORT
        // ═════════════════════════════════════════════════════════════════════
        public async Task<VehicleDispatchReportResponse> GetReportAsync(VehicleDispatchReportFilter filter)
        {
            filter ??= new VehicleDispatchReportFilter();
            var pageIndex = filter.PageIndex < 1 ? 1 : filter.PageIndex;
            var pageSize = filter.PageSize < 1 ? 100 : Math.Min(filter.PageSize, 100000);

            var q = _context.VehicleInwards.AsNoTracking().AsQueryable();

            if (filter.FromDate.HasValue)
            {
                var f = DateOnly.FromDateTime(filter.FromDate.Value.Date);
                q = q.Where(v => v.InvoiceDate != null && v.InvoiceDate >= f);
            }
            if (filter.ToDate.HasValue)
            {
                var t = DateOnly.FromDateTime(filter.ToDate.Value.Date);
                q = q.Where(v => v.InvoiceDate != null && v.InvoiceDate <= t);
            }
            if (!string.IsNullOrWhiteSpace(filter.DealerCode))
            {
                var d = filter.DealerCode.Trim();
                q = q.Where(v => v.DealerCode == d);
            }
            if (!string.IsNullOrWhiteSpace(filter.InvoiceNo))
            {
                var inv = filter.InvoiceNo.Trim();
                q = q.Where(v => v.InvoiceNo != null && v.InvoiceNo.Contains(inv));
            }
            if (!string.IsNullOrWhiteSpace(filter.ChassisNo))
            {
                var ch = filter.ChassisNo.Trim();
                q = q.Where(v => v.ChasisNo != null && v.ChasisNo.Contains(ch));
            }
            switch ((filter.Source ?? "").Trim().ToLowerInvariant())
            {
                case "cogxim": q = q.Where(v => v.CreatedBy == ImportTag); break;
                case "dms": q = q.Where(v => v.CreatedBy == null || v.CreatedBy != ImportTag); break;
            }
            switch ((filter.Status ?? "").Trim().ToLowerInvariant())
            {
                case "accepted": q = q.Where(v => v.IsAccepted == true); break;
                case "pending": q = q.Where(v => v.IsAccepted != true); break;
            }

            // chassis on an Invoiced, not deleted sale bill
            var soldChassis = from d in _context.VehicleSaleBillDetails
                              join h in _context.VehicleSaleBillHeaders on d.VehicleSaleBillId equals h.Id
                              where h.Status == "Invoiced" && !h.IsDeleted
                              select d.ChassisNo;
            switch ((filter.SaleStatus ?? "").Trim().ToLowerInvariant())
            {
                case "sold": q = q.Where(v => soldChassis.Contains(v.ChasisNo!)); break;
                case "unsold": q = q.Where(v => !soldChassis.Contains(v.ChasisNo!)); break;
            }
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.Trim();
                var dealerHits = _context.DealerMasters.Where(d => d.Compname.Contains(s)).Select(d => d.Dealercode);
                var itemHits = _context.ItemMasters.Where(i => i.Itemname.Contains(s)).Select(i => i.Itemcode);
                q = q.Where(v => (v.ChasisNo != null && v.ChasisNo.Contains(s))
                              || (v.MotorNo != null && v.MotorNo.Contains(s))
                              || (v.InvoiceNo != null && v.InvoiceNo.Contains(s))
                              || (v.DealerCode != null && v.DealerCode.Contains(s))
                              || (v.ItemCode != null && v.ItemCode.Contains(s))
                              || (v.BatteryNo != null && v.BatteryNo.Contains(s))
                              || dealerHits.Contains(v.DealerCode!)
                              || itemHits.Contains(v.ItemCode!));
            }

            var response = new VehicleDispatchReportResponse { PageIndex = pageIndex, PageSize = pageSize };
            response.TotalRecords = await q.CountAsync();
            if (response.TotalRecords == 0) return response;

            response.TotalAccepted = await q.CountAsync(v => v.IsAccepted == true);
            response.TotalPending = response.TotalRecords - response.TotalAccepted;
            response.TotalSold = await q.CountAsync(v => soldChassis.Contains(v.ChasisNo!));
            response.TotalUnsold = response.TotalRecords - response.TotalSold;
            response.TotalDealers = await q.Select(v => v.DealerCode).Distinct().CountAsync();
            response.TotalInvoices = await q.Select(v => v.InvoiceNo).Distinct().CountAsync();
            response.TotalCogxim = await q.CountAsync(v => v.CreatedBy == ImportTag);

            var page = await q.OrderByDescending(v => v.InvoiceDate).ThenBy(v => v.InvoiceNo).ThenBy(v => v.ChasisNo)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            // lookups for this page only (avoids row multiplication from non-unique master codes)
            var dCodes = page.Select(v => v.DealerCode).Where(x => x != null).Distinct().ToList();
            var lCodes = page.Select(v => v.LocCode).Where(x => x != null).Distinct().ToList();
            var iCodes = page.Select(v => v.ItemCode).Where(x => x != null).Distinct().ToList();
            var cCodes = page.Select(v => v.ColrCode).Where(x => x != null).Distinct().ToList();
            var chassis = page.Select(v => v.ChasisNo).Where(x => x != null).Distinct().ToList();

            var dealerMap = (await _context.DealerMasters.AsNoTracking().Where(d => dCodes.Contains(d.Dealercode))
                    .Select(d => new { d.Dealercode, d.Compname, d.City, d.State }).ToListAsync())
                .GroupBy(d => d.Dealercode, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
            var locMap = (await _context.LocationMasters.AsNoTracking().Where(l => lCodes.Contains(l.Loccode))
                    .Select(l => new { l.Loccode, l.Locname, l.City }).ToListAsync())
                .GroupBy(l => l.Loccode, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
            var itemMap = (await _context.ItemMasters.AsNoTracking().Where(i => iCodes.Contains(i.Itemcode))
                    .Select(i => new { i.Itemcode, i.Itemname }).ToListAsync())
                .GroupBy(i => i.Itemcode, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.First().Itemname, StringComparer.OrdinalIgnoreCase);
            var colourMap = (await _context.ColorMasters.AsNoTracking().Where(c => cCodes.Contains(c.Colorcode))
                    .Select(c => new { c.Colorcode, c.Colorname }).ToListAsync())
                .GroupBy(c => c.Colorcode, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.First().Colorname, StringComparer.OrdinalIgnoreCase);
            var saleMap = (await (from d in _context.VehicleSaleBillDetails.AsNoTracking()
                                  join h in _context.VehicleSaleBillHeaders.AsNoTracking() on d.VehicleSaleBillId equals h.Id
                                  where h.Status == "Invoiced" && !h.IsDeleted && chassis.Contains(d.ChassisNo)
                                  select new { d.ChassisNo, h.SaleBillNo, h.SaleDate, h.CustomerName }).ToListAsync())
                .GroupBy(x => x.ChassisNo, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.SaleDate).First(), StringComparer.OrdinalIgnoreCase);

            var sr = (pageIndex - 1) * pageSize;
            foreach (var v in page)
            {
                var dm = v.DealerCode != null && dealerMap.TryGetValue(v.DealerCode, out var dd) ? dd : null;
                var lm = v.LocCode != null && locMap.TryGetValue(v.LocCode, out var ll) ? ll : null;
                var sale = v.ChasisNo != null && saleMap.TryGetValue(v.ChasisNo, out var ss) ? ss : null;
                response.Data.Add(new VehicleDispatchReportRow
                {
                    SrNo = ++sr,
                    Id = v.Id,
                    InvoiceDate = v.InvoiceDate,
                    InvoiceNo = v.InvoiceNo,
                    DealerCode = v.DealerCode,
                    DealerName = dm?.Compname,
                    DealerCity = dm?.City,
                    DealerState = dm?.State,
                    LocCode = v.LocCode,
                    LocationName = lm?.Locname,
                    LocationCity = lm?.City,
                    ItemCode = v.ItemCode,
                    ModelName = v.ItemCode != null && itemMap.TryGetValue(v.ItemCode, out var mn) ? mn : null,
                    ColrCode = v.ColrCode,
                    ColourName = v.ColrCode != null && colourMap.TryGetValue(v.ColrCode, out var cn) ? cn : null,
                    MfgMonth = v.MfgMonth,
                    MfgYear = v.MfgYear,
                    ChassisNo = v.ChasisNo,
                    MotorNo = v.MotorNo,
                    BatteryId = v.BatteryId,
                    BatteryNo = v.BatteryNo,
                    BatteryNo2 = v.BatteryNo2,
                    BatteryCapacity = v.BatteryCapacity,
                    BatteryChemistry = v.BatteryChemistry,
                    BatteryMake = v.BatteryMake,
                    ChargerNo = v.ChargerNo,
                    ControllerNo = v.ControllerNo,
                    Converter = v.Converter,
                    Vcu = v.Vcu,
                    EcuSerno = v.EcuSerno,
                    EcuImEi = v.EcuImEi,
                    ImmoblizerNo = v.ImmoblizerNo,
                    BikeSimid = v.BikeSimid,
                    Voltage = v.Voltage,
                    RegNumber = v.Regnumber,
                    StartDate = v.Startdate,
                    TyreNo1 = v.TyreNo1,
                    TyreNo2 = v.TyreNo2,
                    PoNumber = v.Ponumber,
                    IsAccepted = v.IsAccepted,
                    IsD2d = v.IsD2d,
                    Source = v.CreatedBy == ImportTag ? ImportTag : "DMS",
                    SaleBillNo = sale?.SaleBillNo,
                    SaleDate = sale?.SaleDate,
                    CustomerName = sale?.CustomerName,
                    CreatedDate = v.CreatedDate
                });
            }
            return response;
        }

        // ═════════════════════════════════════════════════════════════════════
        // helpers
        // ═════════════════════════════════════════════════════════════════════
        private sealed record ExistingInward(int Id, string ChassisNo, string? CreatedBy, string? InvoiceNo, DateOnly? InvoiceDate, string? DealerCode);

        private sealed class Row
        {
            public int RowNo;
            public List<string> Errors = new();
            public bool Skip;
            public int? ExistingId;
            public DateTime? InvoiceDate, ReceiveDate;
            public string? InvoiceDateRaw, InvoiceNo, DealerCode, LocCode, Location, FinalLocCode, ModelCode, ItemName, ItemCode, ColorCode, ChassisNo,
                MotorNo, BatteryId, BatteryNo, BatteryNo2, BatteryCapacity, BatteryChemistry, BatteryMake, EcuSerno, EcuImei, EcuBalMac,
                ImmobilizerNo, BikeSimId, BikeMobileNo, ChargerNo, ControllerNo, ConverterNo, SoundbarSerno, SoundbarBalMac, Voltage, Vcu,
                RegNumber, StartDate, Tyre1, Tyre2, PoNumber;
            public int? MfgYear, MfgMonth;
        }

        private static Row ParseRow(LargeExcelRow xr, int rowNo, Dictionary<string, int> col)
        {
            string? T(string key) => col.TryGetValue(key, out var c) ? Text(xr.Cell(c)) : null;
            DateTime? D(string key, out string? raw)
            {
                raw = null;
                if (!col.TryGetValue(key, out var c)) return null;
                raw = Text(xr.Cell(c));
                return Date(xr.Cell(c));
            }

            var r = new Row { RowNo = rowNo };
            r.InvoiceDate = D("invoiceDate", out var invRaw); r.InvoiceDateRaw = invRaw;
            r.ReceiveDate = D("receiveDate", out _);
            r.InvoiceNo = T("invoiceNo");
            r.DealerCode = T("dealerCode");
            r.LocCode = T("locCode");
            r.Location = T("location");
            r.ModelCode = T("modelCode");
            r.ItemName = T("itemName");
            r.ColorCode = T("colorCode");
            r.ChassisNo = T("chassisNo")?.ToUpperInvariant();
            r.MotorNo = T("motorNo");
            r.BatteryId = T("batteryId");
            r.BatteryNo = T("batteryNo");
            r.BatteryNo2 = T("batteryNo2");
            r.BatteryCapacity = T("batteryCapacity");
            r.BatteryChemistry = T("batteryChemistry");
            r.BatteryMake = T("batteryMake");
            r.EcuSerno = T("ecuSerno");
            r.EcuImei = T("ecuImei");
            r.EcuBalMac = T("ecuBalMac");
            r.ImmobilizerNo = T("immobilizerNo");
            r.BikeSimId = T("bikeSimId");
            r.BikeMobileNo = T("bikeMobileNo");
            r.ChargerNo = T("chargerNo");
            r.ControllerNo = T("controllerNo");
            r.ConverterNo = T("converterNo");
            r.SoundbarSerno = T("soundbarSerno");
            r.SoundbarBalMac = T("soundbarBalMac");
            r.Voltage = T("voltage");
            r.Vcu = T("vcu");
            r.RegNumber = T("regNumber");
            r.Tyre1 = T("tyre1");
            r.Tyre2 = T("tyre2");
            r.PoNumber = T("poNumber");
            // Start Date is a text column in VehicleInward: keep dd/MM/yyyy
            var sd = D("startDate", out var sdRaw);
            r.StartDate = sd?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? sdRaw;

            if (int.TryParse(T("mfgYear"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var y) && y is >= 2000 and <= 2100) r.MfgYear = y;
            r.MfgMonth = Month(T("mfgMonth"));
            return r;
        }

        private static int? Month(string? s)
        {
            if (s == null) return null;
            if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var m)) return m is >= 1 and <= 12 ? m : null;
            for (var i = 1; i <= 12; i++)
            {
                var full = CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(i);
                if (s.Equals(full, StringComparison.OrdinalIgnoreCase) || s.Equals(full[..3], StringComparison.OrdinalIgnoreCase)) return i;
            }
            return null;
        }

        /// <summary>Streaming reader: no 32,767-character cell limit, comments / pictures ignored, low memory.</summary>
        private static LargeExcelWorkbook OpenWorkbook(Stream excel)
        {
            try { return new LargeExcelWorkbook(excel); }
            catch (Exception ex) when (ex is not ArgumentException)
            {
                throw new ArgumentException("This file is not a valid .xlsx workbook (" + ex.GetBaseException().Message +
                    "). Open it in Excel and Save As \"Excel Workbook (.xlsx)\".");
            }
        }

        private static LargeExcelSheet PickSheet(LargeExcelWorkbook wb, string? sheetName)
        {
            if (!string.IsNullOrWhiteSpace(sheetName))
                return wb.Worksheets.FirstOrDefault(s => string.Equals(s.Name, sheetName, StringComparison.OrdinalIgnoreCase))
                       ?? throw new ArgumentException($"Sheet \"{sheetName}\" not found.");
            return wb.Worksheets.First();
        }

        /// <summary>field key -> column number, from row 1.</summary>
        private static Dictionary<string, int> ResolveColumns(LargeExcelSheet ws)
        {
            var row = ws.PeekRow(1);   // header only: does not read the whole sheet
            var last = row.LastCellUsed()?.Address.ColumnNumber ?? 0;
            var byNorm = new Dictionary<string, int>();
            for (var c = 1; c <= last; c++)
            {
                var n = Norm(Text(row.Cell(c)));
                if (n.Length > 0 && !byNorm.ContainsKey(n)) byNorm[n] = c;
            }
            var result = new Dictionary<string, int>();
            foreach (var (key, aliases) in Columns)
                foreach (var a in aliases)
                    if (byNorm.TryGetValue(a, out var c)) { result[key] = c; break; }
            return result;
        }

        /// <summary>Live size of every text column of VehicleInward (property name -> characters).</summary>
        private async Task<Dictionary<string, int>> LoadColumnLengthsAsync()
        {
            var result = new Dictionary<string, int>();
            var et = _context.Model.FindEntityType(typeof(VehicleInward));
            if (et == null) return result;
            var props = et.GetProperties().Where(p => p.ClrType == typeof(string)).ToList();
            foreach (var p in props)
                if (p.GetMaxLength() is int ml) result[p.Name] = ml;   // EF model as fallback
            try
            {
                var table = et.GetTableName() ?? "VehicleInward";
                var schema = et.GetSchema() ?? "dbo";
                var live = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                var conn = _context.Database.GetDbConnection();
                var opened = conn.State != ConnectionState.Open;
                if (opened) await conn.OpenAsync();
                try
                {
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT COLUMN_NAME, CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS " +
                                      "WHERE TABLE_SCHEMA = @s AND TABLE_NAME = @t AND CHARACTER_MAXIMUM_LENGTH IS NOT NULL";
                    var ps = cmd.CreateParameter(); ps.ParameterName = "@s"; ps.Value = schema; cmd.Parameters.Add(ps);
                    var pt = cmd.CreateParameter(); pt.ParameterName = "@t"; pt.Value = table; cmd.Parameters.Add(pt);
                    using var rd = await cmd.ExecuteReaderAsync();
                    while (await rd.ReadAsync())
                    {
                        var len = Convert.ToInt32(rd.GetValue(1), CultureInfo.InvariantCulture);
                        live[rd.GetString(0)] = len < 0 ? int.MaxValue : len;
                    }
                }
                finally
                {
                    if (opened) await conn.CloseAsync();
                }
                foreach (var p in props)
                {
                    var column = p.GetColumnName() ?? p.Name;
                    if (live.TryGetValue(column, out var len)) result[p.Name] = len;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not read VehicleInward column sizes - EF model sizes used.");
            }
            return result;
        }

        /// <summary>Cuts every text value to its column size (so one long value cannot fail the whole import).</summary>
        private static void Fit(VehicleInward v, Dictionary<string, int> lengths, Action<string> warn)
        {
            foreach (var (name, max) in lengths)
            {
                var prop = typeof(VehicleInward).GetProperty(name);
                if (prop == null || prop.PropertyType != typeof(string) || !prop.CanWrite) continue;
                if (prop.GetValue(v) is string s && s.Length > max)
                {
                    prop.SetValue(v, s[..max]);
                    if (name != nameof(VehicleInward.UpdatedBy))
                        warn($"{name} \"{s}\" is longer than the column ({max} characters) - saved as \"{s[..max]}\".");
                }
            }
        }

        // ---------- cell conversion (same rules as the Cogxim sale import) ----------
        private static string? Text(LargeExcelCell cell)
        {
            var v = cell.Value;
            string? s;
            if (v.IsBlank) return null;
            if (v.IsNumber)
            {
                var n = v.GetNumber();
                s = Math.Abs(n % 1) < 1e-9 && Math.Abs(n) < 1e15
                    ? ((long)Math.Round(n)).ToString(CultureInfo.InvariantCulture)
                    : n.ToString(CultureInfo.InvariantCulture);
            }
            else if (v.IsDateTime) s = v.GetDateTime().ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
            else if (v.IsBoolean) s = v.GetBoolean() ? "Y" : "N";
            else if (v.IsError) return null;
            else s = v.ToString(CultureInfo.InvariantCulture);
            s = s?.Trim();
            return string.IsNullOrEmpty(s) || s.Equals("NULL", StringComparison.OrdinalIgnoreCase) ? null : s;
        }

        // Cogxim writes dates day first (31/08/2020, 3/2/2021 = 03.02.2021)
        private static readonly string[] DateFormats =
        {
            "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy", "dd.MM.yyyy", "d.M.yyyy",
            "dd-MMM-yyyy", "dd MMM yyyy", "d-MMM-yy", "dd/MM/yy", "yyyy-MM-dd", "yyyy/MM/dd",
            "dd/MM/yyyy HH:mm:ss", "d/M/yyyy HH:mm:ss", "dd/MM/yyyy hh:mm:ss tt", "d/M/yyyy h:mm:ss tt",
            "dd-MM-yyyy HH:mm:ss", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-dd HH:mm:ss.fff", "dd/MM/yyyy HH:mm"
        };

        private static DateTime? Date(LargeExcelCell cell)
        {
            var v = cell.Value;
            if (v.IsBlank) return null;
            if (v.IsDateTime) return v.GetDateTime();
            if (v.IsNumber)
            {
                var n = v.GetNumber();
                return n is > 20000 and < 80000 ? DateTime.FromOADate(n) : null;
            }
            var s = Text(cell);
            if (s == null) return null;
            if (DateTime.TryParseExact(s, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var d)) return d;
            if (DateTime.TryParse(s, new CultureInfo("en-IN"), DateTimeStyles.AllowWhiteSpaces, out d)) return d;
            return null;
        }

        private static string Norm(string? s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var sb = new StringBuilder(s.Length);
            foreach (var ch in s) if (char.IsLetterOrDigit(ch)) sb.Append(char.ToLowerInvariant(ch));
            return sb.ToString();
        }

        private static string? Cut(string? s, int max)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            s = s.Trim();
            return s.Length <= max ? s : s[..max];
        }
    }
}
