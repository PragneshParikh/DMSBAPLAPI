using ClosedXML.Excel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.VehicleSaleImportService
{
    /// <summary>
    /// Imports the Cogxim (old DMS) sale register - tblSaleRegister2021-Cogxim ... 2027 exported
    /// to Excel - into VehicleSaleBillHeader + VehicleSaleBillDetails.
    ///
    /// Rules
    ///  * 1 Excel row = 1 vehicle. Rows with the same Bill No (per dealer) = 1 bill.
    ///  * Imported bills get Status "Invoiced" (so every report / dashboard counts them) and
    ///    BillFrom "Cogxim Import" (so they can always be identified).
    ///  * Nothing else is touched: no stock (PartsInventory), ChassisDetails, InvoiceHeader,
    ///    JobCardCustomer or ERP push - this is historical data.
    ///  * Re-running the same file is safe: existing Sale Bill Nos and already-billed chassis
    ///    are skipped, not duplicated.
    ///  * A bill is imported only if ALL its rows are valid.
    /// </summary>
    public class VehicleSaleImportService : IVehicleSaleImportService
    {
        public const string BillFromTag = "Cogxim Import";
        private const string ImportStatus = "Invoiced";
        private const int MaxIssues = 1000;
        private const int SaveChunk = 200;

        private readonly BapldmsvadContext _context;
        private readonly ILogger<VehicleSaleImportService> _logger;

        public VehicleSaleImportService(BapldmsvadContext context, ILogger<VehicleSaleImportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ═════════════════════════════════════════════════════════════════════
        // FIELD CATALOGUE  (key, label, group, required, DB target, aliases)
        // Aliases are compared after lower-casing and removing everything that
        // is not a letter/digit, so "Chasis No." == "chasisno" == "CHASIS_NO".
        // ═════════════════════════════════════════════════════════════════════
        private static readonly List<ImportFieldViewModel> Fields = new()
        {
            // ---------- Bill (header) ----------
            F("dealerCode", "Dealer Code", "Header", false, "VehicleSaleBillHeader.DealerCode", "dealercode", "dlrcode", "dealer", "dealerid", "compcode"),
            F("saleBillNo", "Bill No", "Header", true, "VehicleSaleBillHeader.SaleBillNo", "billno", "salebillno", "invoiceno", "billnumber", "invno", "voucherno"),
            F("saleDate", "Bill Date", "Header", true, "VehicleSaleBillHeader.SaleDate", "billdate", "saledate", "invoicedate", "invdate", "date"),
            F("bookingId", "Booking Id", "Header", false, "VehicleSaleBillHeader.BookingId", "bookingid", "bookingno", "referenceno"),
            F("customerName", "Party / Customer Name", "Header", false, "VehicleSaleBillHeader.CustomerName + LedgerId", "partyname", "customername", "soldto", "customer", "party"),
            F("billingName", "Billing Name / Contact Person", "Header", false, "VehicleSaleBillHeader.BillingName", "contactperson", "billingname"),
            F("customerMobile", "Party Mobile", "Header", false, "LedgerMaster.MobileNumber (customer match)", "partymobile", "cusmob", "customermobile", "mobileno", "mobile", "phone"),
            F("customerEmail", "Party Email", "Header", false, "LedgerMaster.eMail (new customer)", "partyemail", "email", "emailid"),
            F("customerAddress", "Party Address", "Header", false, "LedgerMaster.Address (new customer)", "partyaddress", "address", "address1"),
            F("customerGstin", "GSTN No", "Header", false, "LedgerMaster.GSTNo (new customer)", "gstnno", "gstno", "gstin", "partygstin"),
            F("location", "Location", "Header", false, "VehicleSaleBillHeader.Location (Loccode)", "location", "loccode", "locationname", "locname", "branch"),
            F("salesExecutive", "Executive Name", "Header", false, "VehicleSaleBillHeader.SalesExecutive", "executivename", "salesexecutive", "executive"),
            F("saleType", "Sales Type (Cash/Credit)", "Header", false, "VehicleSaleBillHeader.SaleType", "salestype", "saletype", "paymenttype", "paymentmode"),
            F("financier", "Financer Name", "Header", false, "VehicleSaleBillHeader.Financier (LedgerMaster.Id)", "financername", "financiername", "financedby", "financer", "financier"),
            F("customerType", "Customer / Account Type", "Header", false, "VehicleSaleBillHeader.CustomerType + BillType", "customertype", "accounttype"),
            F("tempRegNo", "Temp Reg No", "Header", false, "VehicleSaleBillHeader.TempRegNo", "tempregno", "temporaryregno"),
            F("accessoryBillNo", "Accessory Bill No", "Header", false, "VehicleSaleBillHeader.AccessoryBillNo", "accessorybillno", "acsrybillno"),
            F("accessoryAmount", "Accessory Amount", "Header", false, "VehicleSaleBillHeader.AccessoryAmount", "acsryamnt", "acsryamount", "accessoryamount", "accessoryamnt"),
            F("hpAmount", "Hyp Amount", "Header", false, "VehicleSaleBillHeader.HPAmount", "hypamnt", "hypamount", "hpamount"),
            F("noPlateAmount", "Number Plate Amount", "Header", false, "VehicleSaleBillHeader.NoPlateAmount", "numplateamnt", "numplateamount", "numberplateamount", "noplateamount", "hsrpamount"),
            F("handlingCharges", "Handling Charges", "Header", false, "VehicleSaleBillHeader.HandlingCharges", "handlingcharges", "handlingcharge"),
            F("stateSubsidyAmount", "State Subsidy Amount", "Header", false, "VehicleSaleBillHeader.StateSubsidyAmount", "statesubsidyamnt", "statesubsidyamount", "statefameii"),
            F("totalAmount", "Bill Total (optional)", "Header", false, "VehicleSaleBillHeader.TotalAmount", "billamount", "billtotal", "grandtotal", "totalamount", "totalbillamount"),

            // ---------- Vehicle (detail) ----------
            F("chassisNo", "Chassis No", "Detail", true, "VehicleSaleBillDetails.ChassisNo", "chasisno", "chassisno", "chassis", "vinno", "vin", "frameno"),
            F("modelName", "Item Model", "Detail", false, "VehicleSaleBillDetails.ModelName", "itemmodel", "modelname", "model", "itemname", "vehiclemodel"),
            F("oemModel", "OEM Model Name", "Detail", false, "used to find ItemCode", "oemmodelname", "oemmodel"),
            F("itemCode", "Item Code", "Detail", false, "VehicleSaleBillDetails.ItemCode", "itemcode", "productcode"),
            F("colour", "Colour", "Detail", false, "VehicleSaleBillDetails.Colour", "color", "colour", "colorname", "colourname", "colorcode"),
            F("mfgYear", "Mfg Year", "Detail", false, "VehicleSaleBillDetails.MfgYear", "mfgyear", "manufacturingyear", "yearofmfg"),
            F("itemRate", "Item Rate", "Detail", false, "VehicleSaleBillDetails.ItemRate", "itemrate", "rate", "basicrate", "exshowroomrate"),
            F("preGstDiscount", "Pre-GST Discount", "Detail", false, "VehicleSaleBillDetails.PreGstDiscount", "pregstdiscamnt", "pregstdiscamount", "pregstdisc", "pregstdiscount"),
            F("postGstDiscount", "Post-GST Discount", "Detail", false, "VehicleSaleBillDetails.PostGstDisc", "postgstdiscamnt", "postgstdiscamount", "postgstdisc", "postgstdiscount"),
            F("fameII", "Subsidy (FAME-II / PM E-DRIVE)", "Detail", false, "VehicleSaleBillDetails.FameII", "subsidyamnt", "subsidyamount", "fameii", "fame2", "fame2amount", "pmedrive"),
            F("sgstPer", "SGST %", "Detail", false, "VehicleSaleBillDetails.SGSTPer", "sgstper", "sgstpercent", "sgstrate", "sgst"),
            F("sgstAmount", "SGST Amount", "Detail", false, "VehicleSaleBillDetails.SGSTAmnt", "sgstamnt", "sgstamount"),
            F("cgstPer", "CGST %", "Detail", false, "VehicleSaleBillDetails.CGSTPer", "cgstper", "cgstpercent", "cgstrate", "cgst"),
            F("cgstAmount", "CGST Amount", "Detail", false, "VehicleSaleBillDetails.CGSTAmnt", "cgstamnt", "cgstamount"),
            F("igstPer", "IGST %", "Detail", false, "VehicleSaleBillDetails.IGSTPer", "igstper", "igstpercent", "igstrate", "igst"),
            F("igstAmount", "IGST Amount", "Detail", false, "VehicleSaleBillDetails.IGSTAmnt", "igstamnt", "igstamount"),
            F("regAmount", "Registration Amount", "Detail", false, "VehicleSaleBillDetails.RegAmount", "regnamnt", "regnamount", "regamount", "registrationamount"),
            F("insuranceAmount", "Insurance Amount", "Detail", false, "VehicleSaleBillDetails.InsuranceAmount", "insuamnt", "insuamount", "insuranceamount", "insamount", "insamnt"),
            F("finalAmount", "Net Amount", "Detail", false, "VehicleSaleBillDetails.FinalAmount", "netamnt", "netamount", "finalamount", "nettotal"),
            F("regNo", "Reg No", "Detail", false, "VehicleSaleBillDetails.RegNo", "regno", "registrationno", "vehicleregno", "regnumber"),
            F("insNo", "Insurance Policy No", "Detail", false, "VehicleSaleBillDetails.InsNo", "insno", "policyno", "insurancepolicyno", "insurancono"),
            F("insStartDate", "Insurance Start Date", "Detail", false, "VehicleSaleBillDetails.InsStartDate", "insstartdate", "policystartdate", "insurancestartdate"),
            F("insExpDate", "Insurance Expiry Date", "Detail", false, "VehicleSaleBillDetails.InsExpDate", "insexpdate", "policyexpirydate", "insuranceexpdate", "insuranceexpirydate"),
            F("motorNo", "Motor No (kept in Narration)", "Detail", false, "VehicleSaleBillDetails.Narration", "motorno", "motornumber", "engineno"),
            F("battery", "Battery No", "Detail", false, "VehicleSaleBillDetails.Battery", "battery", "batteryno", "batteryserialno"),
            F("batteryMake", "Battery Make", "Detail", false, "VehicleSaleBillDetails.BatteryMake", "batterymake"),
            F("batteryChemical", "Battery Chemistry / Type", "Detail", false, "VehicleSaleBillDetails.BatteryChemical", "batterychemical", "batterychemistry", "batterytype"),
            F("batteryCapacity", "Battery Capacity", "Detail", false, "VehicleSaleBillDetails.BatteryCapacity", "batterycapacity"),
            F("chargerNo", "Charger No", "Detail", false, "VehicleSaleBillDetails.ChargerNo", "chargerno", "charger"),
            F("controllerNo", "Controller No", "Detail", false, "VehicleSaleBillDetails.ControllerNo", "controllerno", "controller"),
            F("convertorNo", "Converter No", "Detail", false, "VehicleSaleBillDetails.ConvertorNo", "converter", "convertor", "converterno", "convertorno"),
            F("vcu", "VCU", "Detail", false, "VehicleSaleBillDetails.VCU", "vcu", "vcuno"),
            F("keyNo", "Key No", "Detail", false, "VehicleSaleBillDetails.Key", "keyno", "key"),
            F("bookNo", "Service Book No", "Detail", false, "VehicleSaleBillDetails.BookNo", "bookno", "servicebookno", "servbkno"),
            F("segment", "Segment", "Detail", false, "VehicleSaleBillDetails.Segment", "segment", "segmentname"),
            F("institutionalType", "Institutional Name", "Detail", false, "VehicleSaleBillDetails.InstitutionalType", "institutionalname", "institutionaltype"),
            F("schemeName", "Scheme Name", "Detail", false, "VehicleSaleBillDetails.SchemeName", "schemename", "scheme"),
            F("narration", "Narration / Remarks", "Detail", false, "VehicleSaleBillDetails.Narration", "narration", "remarks", "remark"),
        };

        private static ImportFieldViewModel F(string key, string label, string group, bool required, string target, params string[] aliases)
            => new() { Key = key, Label = label, Group = group, Required = required, Target = target, Aliases = aliases.ToList() };

        public List<ImportFieldViewModel> GetFields() => Fields;

        // ═════════════════════════════════════════════════════════════════════
        // PREVIEW
        // ═════════════════════════════════════════════════════════════════════
        public VehicleSaleImportPreviewResponse Preview(Stream excel, string fileName, string? sheetName, int headerRow)
        {
            using var wb = new XLWorkbook(excel);
            var ws = PickSheet(wb, sheetName);
            headerRow = headerRow < 1 ? 1 : headerRow;
            var headers = ReadHeaders(ws, headerRow);
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? headerRow;

            var response = new VehicleSaleImportPreviewResponse
            {
                FileName = fileName,
                Sheets = wb.Worksheets.Select(s => new ImportSheetInfo { Name = s.Name, Rows = Math.Max(0, (s.LastRowUsed()?.RowNumber() ?? 1) - 1) }).ToList(),
                SheetName = ws.Name,
                HeaderRow = headerRow,
                Headers = headers.Select(h => h.Title).ToList(),
                SuggestedMapping = SuggestMapping(headers.Select(h => h.Title).ToList()),
                Fields = Fields
            };

            var rows = 0;
            for (var r = headerRow + 1; r <= lastRow; r++)
            {
                var row = ws.Row(r);
                if (row.IsEmpty()) continue;
                rows++;
                if (response.SampleRows.Count < 10)
                {
                    var dict = new Dictionary<string, string?>();
                    foreach (var h in headers) dict[h.Title] = Text(row.Cell(h.Column));
                    response.SampleRows.Add(dict);
                }
            }
            response.TotalRows = rows;
            return response;
        }

        // ═════════════════════════════════════════════════════════════════════
        // IMPORT (dry run or real)
        // ═════════════════════════════════════════════════════════════════════
        public async Task<VehicleSaleImportResult> ImportAsync(Stream excel, string fileName, VehicleSaleImportOptions options, string userId)
        {
            options ??= new VehicleSaleImportOptions();
            var headerRow = options.HeaderRow < 1 ? 1 : options.HeaderRow;

            using var wb = new XLWorkbook(excel);
            var ws = PickSheet(wb, options.SheetName);
            var headers = ReadHeaders(ws, headerRow);
            var colOf = ResolveMapping(options.Mapping, headers);

            foreach (var f in Fields.Where(f => f.Required))
                if (!colOf.ContainsKey(f.Key))
                    throw new ArgumentException($"Map a column to the required field \"{f.Label}\".");
            if (!colOf.ContainsKey("dealerCode") && string.IsNullOrWhiteSpace(options.DefaultDealerCode))
                throw new ArgumentException("Map the Dealer Code column or choose a default dealer.");

            var result = new VehicleSaleImportResult { DryRun = options.DryRun, FileName = fileName, SheetName = ws.Name };
            var issues = new List<ImportIssue>();
            void Issue(ImportRow? r, string level, string msg, int rowNo = 0) =>
                issues.Add(new ImportIssue { RowNo = r?.RowNo ?? rowNo, SaleBillNo = r?.SaleBillNo, ChassisNo = r?.ChassisNo, Level = level, Message = msg });

            // ---------- 1. read + type-convert every row ----------
            var rows = new List<ImportRow>();
            var lastRow = ws.LastRowUsed()?.RowNumber() ?? headerRow;
            for (var r = headerRow + 1; r <= lastRow; r++)
            {
                var xr = ws.Row(r);
                if (xr.IsEmpty()) continue;
                var row = ParseRow(xr, r, colOf, options.DefaultDealerCode);
                rows.Add(row);
            }
            result.TotalRows = rows.Count;

            // ---------- 2. reference data (only what the file needs) ----------
            var dealerCodesInFile = rows.Select(x => x.DealerCode).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!.ToUpperInvariant()).Distinct().ToList();
            var dealers = (await _context.DealerMasters.AsNoTracking().Select(d => d.Dealercode).ToListAsync())
                .Where(d => d != null).GroupBy(d => d.Trim().ToUpperInvariant()).ToDictionary(g => g.Key, g => g.First().Trim());

            var locations = await _context.LocationMasters.AsNoTracking()
                .Select(l => new { l.Dealercode, l.Loccode, l.Locname, l.Locareaidno }).ToListAsync();

            var items = await _context.ItemMasters.AsNoTracking()
                .Select(i => new { i.Itemcode, i.Itemname, i.Displayname, i.Oemmodelname }).ToListAsync();
            var itemByCode = items.Where(i => i.Itemcode != null).GroupBy(i => Norm(i.Itemcode)).ToDictionary(g => g.Key, g => g.First().Itemcode);
            var itemByName = new Dictionary<string, string>();
            foreach (var i in items)
                foreach (var n in new[] { i.Itemname, i.Displayname, i.Oemmodelname })
                    if (!string.IsNullOrWhiteSpace(n) && !itemByName.ContainsKey(Norm(n))) itemByName[Norm(n)] = i.Itemcode;

            var financiers = (await _context.LedgerMasters.AsNoTracking()
                    .Where(l => l.LedgerType == "Financier")
                    .Select(l => new { l.Id, l.LedgerName }).ToListAsync())
                .Where(l => !string.IsNullOrWhiteSpace(l.LedgerName))
                .GroupBy(l => Norm(l.LedgerName)).ToDictionary(g => g.Key, g => g.First().Id);

            var canonicalDealers = dealerCodesInFile.Where(dealers.ContainsKey).Select(d => dealers[d]).ToList();
            var customers = await _context.LedgerMasters.AsNoTracking()
                .Where(l => l.DealerCode != null && canonicalDealers.Contains(l.DealerCode)
                            && (l.LedgerType == "Party" || l.LedgerType == "Institutional"))
                .Select(l => new { l.Id, l.DealerCode, l.LedgerName, l.MobileNumber, l.LedgerCode }).ToListAsync();
            var custByMobile = new Dictionary<string, int>();
            var custByName = new Dictionary<string, int>();
            foreach (var c in customers)
            {
                var d = c.DealerCode!.ToUpperInvariant();
                var m = Digits(c.MobileNumber);
                if (m.Length >= 10 && !custByMobile.ContainsKey(d + "|" + m)) custByMobile[d + "|" + m] = c.Id;
                if (!string.IsNullOrWhiteSpace(c.LedgerName) && !custByName.ContainsKey(d + "|" + Norm(c.LedgerName))) custByName[d + "|" + Norm(c.LedgerName)] = c.Id;
            }

            // ---------- 3. row-level validation ----------
            foreach (var r in rows)
            {
                if (string.IsNullOrWhiteSpace(r.SaleBillNo)) r.Errors.Add("Bill No is blank.");
                if (r.SaleDate == null) r.Errors.Add(r.SaleDateRaw == null ? "Bill Date is blank." : $"Bill Date \"{r.SaleDateRaw}\" is not a valid date.");
                if (string.IsNullOrWhiteSpace(r.ChassisNo)) r.Errors.Add("Chassis No is blank.");
                if (string.IsNullOrWhiteSpace(r.DealerCode)) r.Errors.Add("Dealer Code is blank.");
                else if (!dealers.TryGetValue(r.DealerCode!.ToUpperInvariant(), out var canon)) r.Errors.Add($"Dealer \"{r.DealerCode}\" not found in Dealer Master.");
                else r.DealerCode = canon;
                if (r.ChassisNo?.Length > 50) r.Errors.Add("Chassis No is longer than 50 characters.");
                foreach (var p in new[] { r.SgstPer, r.CgstPer, r.IgstPer })
                    if (p is < 0 or > 100) r.Errors.Add($"GST % {p} is outside 0-100.");

                // Final bill number (SaleBillNo is unique across all dealers)
                if (!string.IsNullOrWhiteSpace(r.SaleBillNo) && !string.IsNullOrWhiteSpace(r.DealerCode))
                {
                    r.FinalBillNo = options.PrefixDealerCodeOnBillNo ? $"{r.DealerCode}/{r.SaleBillNo}" : r.SaleBillNo!;
                    if (r.FinalBillNo.Length > 50) r.Errors.Add($"Bill No \"{r.FinalBillNo}\" is longer than 50 characters.");
                }
            }

            // same bill number used by two different dealers in the file
            if (!options.PrefixDealerCodeOnBillNo)
            {
                foreach (var g in rows.Where(r => r.FinalBillNo != null).GroupBy(r => r.FinalBillNo!, StringComparer.OrdinalIgnoreCase))
                {
                    var owners = g.Select(r => r.DealerCode).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                    if (owners.Count <= 1) continue;
                    foreach (var r in g.Where(r => !string.Equals(r.DealerCode, owners[0], StringComparison.OrdinalIgnoreCase)))
                        r.Errors.Add($"Bill No also used by dealer {owners[0]}. Tick \"Prefix dealer code on Bill No\" to import both.");
                }
            }

            // duplicate chassis inside the file
            var seenChassis = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var r in rows.Where(r => !string.IsNullOrWhiteSpace(r.ChassisNo)))
            {
                if (seenChassis.TryGetValue(r.ChassisNo!, out var firstRow)) r.Errors.Add($"Chassis repeated in file (first on row {firstRow}).");
                else seenChassis[r.ChassisNo!] = r.RowNo;
            }

            // ---------- 4. what already exists in the DB ----------
            var billNos = rows.Where(r => r.FinalBillNo != null).Select(r => r.FinalBillNo!).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var existingBills = new Dictionary<string, (string? Dealer, bool Deleted)>(StringComparer.OrdinalIgnoreCase);
            foreach (var chunk in billNos.Chunk(1000))
            {
                var found = await _context.VehicleSaleBillHeaders.AsNoTracking()
                    .Where(h => chunk.Contains(h.SaleBillNo))
                    .Select(h => new { h.SaleBillNo, h.DealerCode, h.IsDeleted }).ToListAsync();
                foreach (var f in found) existingBills[f.SaleBillNo] = (f.DealerCode, f.IsDeleted);
            }

            var chassisList = seenChassis.Keys.ToList();
            var billedChassis = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var chunk in chassisList.Chunk(1000))
            {
                var found = await (from d in _context.VehicleSaleBillDetails.AsNoTracking()
                                   join h in _context.VehicleSaleBillHeaders.AsNoTracking() on d.VehicleSaleBillId equals h.Id
                                   where chunk.Contains(d.ChassisNo) && !h.IsDeleted
                                   select new { d.ChassisNo, h.SaleBillNo }).ToListAsync();
                foreach (var f in found) billedChassis.TryAdd(f.ChassisNo, f.SaleBillNo);
            }

            // ---------- 5. group into bills ----------
            var bills = rows.Where(r => r.FinalBillNo != null && r.DealerCode != null)
                .GroupBy(r => r.FinalBillNo!, StringComparer.OrdinalIgnoreCase)
                .ToList();
            result.BillsInFile = bills.Count;

            foreach (var r in rows.Where(r => r.FinalBillNo == null || r.DealerCode == null))
            {
                foreach (var e in r.Errors) Issue(r, "Error", e);
                result.RowsWithErrors++;
            }

            var newLedgers = new Dictionary<string, LedgerMaster>();   // key dealer|mobile or dealer|name
            var ledgerCounters = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var ready = new List<(VehicleSaleBillHeader Header, LedgerMaster? NewLedger, List<ImportRow> Rows)>();

            foreach (var g in bills)
            {
                var billRows = g.OrderBy(r => r.RowNo).ToList();
                var first = billRows[0];

                if (existingBills.TryGetValue(g.Key, out var ex))
                {
                    foreach (var r in billRows) Issue(r, "Skipped", ex.Deleted
                        ? "Bill No exists (deleted) in DMS - Sale Bill No must be unique, so it is skipped."
                        : $"Already in DMS (dealer {ex.Dealer}) - skipped.");
                    result.RowsSkipped += billRows.Count;
                    continue;
                }

                // chassis already billed in DMS -> skip that vehicle only
                var usable = new List<ImportRow>();
                foreach (var r in billRows)
                {
                    if (r.ChassisNo != null && billedChassis.TryGetValue(r.ChassisNo, out var otherBill))
                    {
                        Issue(r, "Skipped", $"Chassis already billed in DMS bill {otherBill} - skipped.");
                        result.RowsSkipped++;
                    }
                    else usable.Add(r);
                }
                if (usable.Count == 0) continue;

                var bad = usable.Where(r => r.Errors.Count > 0).ToList();
                if (bad.Count > 0)
                {
                    foreach (var r in bad) foreach (var e in r.Errors) Issue(r, "Error", e);
                    foreach (var r in usable.Except(bad))
                        Issue(r, "Error", $"Bill not imported because row {bad[0].RowNo} of the same bill has errors.");
                    result.RowsWithErrors += usable.Count;
                    continue;
                }

                // header values that differ between rows of the same bill
                if (usable.Select(r => r.SaleDate!.Value.Date).Distinct().Count() > 1)
                    Issue(first, "Warning", "Rows of this bill have different Bill Dates - the first row's date is used.");
                if (usable.Select(r => Norm(r.CustomerName)).Distinct().Count() > 1)
                    Issue(first, "Warning", "Rows of this bill have different Party Names - the first row's party is used.");

                var dealer = first.DealerCode!;
                var dKey = dealer.ToUpperInvariant();

                // ---- location (stored as Loccode) ----
                var dealerLocs = locations.Where(l => string.Equals(l.Dealercode, dealer, StringComparison.OrdinalIgnoreCase)).ToList();
                string? locCode = null;
                if (!string.IsNullOrWhiteSpace(first.Location))
                {
                    var n = Norm(first.Location);
                    locCode = dealerLocs.FirstOrDefault(l => Norm(l.Loccode) == n || Norm(l.Locname) == n)?.Loccode
                              ?? locations.FirstOrDefault(l => Norm(l.Loccode) == n)?.Loccode;
                }
                if (locCode == null && !string.IsNullOrWhiteSpace(options.DefaultLocation)
                    && dealerLocs.Any(l => string.Equals(l.Loccode, options.DefaultLocation, StringComparison.OrdinalIgnoreCase)))
                    locCode = options.DefaultLocation;
                if (locCode == null)
                {
                    locCode = (dealerLocs.FirstOrDefault(l => l.Locareaidno == 1) ?? dealerLocs.FirstOrDefault())?.Loccode;
                    if (!string.IsNullOrWhiteSpace(first.Location) || locCode == null)
                        Issue(first, "Warning", locCode == null
                            ? $"No location found for dealer {dealer}; Location stored as \"{Cut(first.Location, 100)}\"."
                            : $"Location \"{first.Location}\" not found; dealer's showroom {locCode} used.");
                    locCode ??= first.Location;
                }

                // ---- financier ----
                int? financierId = null;
                if (!string.IsNullOrWhiteSpace(first.Financier))
                {
                    if (financiers.TryGetValue(Norm(first.Financier), out var fid)) financierId = fid;
                    else Issue(first, "Warning", $"Financier \"{first.Financier}\" not in Ledger Master - name kept in Narration.");
                }

                // ---- customer ledger ----
                int? ledgerId = null;
                LedgerMaster? newLedger = null;
                var mobile = Digits(first.CustomerMobile);
                if (mobile.Length > 10) mobile = mobile[^10..];
                if (mobile.Length == 10 && custByMobile.TryGetValue(dKey + "|" + mobile, out var cid)) ledgerId = cid;
                else if (!string.IsNullOrWhiteSpace(first.CustomerName) && custByName.TryGetValue(dKey + "|" + Norm(first.CustomerName), out var cid2)) ledgerId = cid2;
                else if (!string.IsNullOrWhiteSpace(first.CustomerName))
                {
                    var key = mobile.Length == 10 ? dKey + "|" + mobile : dKey + "|" + Norm(first.CustomerName);
                    if (options.CreateMissingCustomers)
                    {
                        if (!newLedgers.TryGetValue(key, out newLedger))
                        {
                            newLedger = new LedgerMaster
                            {
                                LedgerName = Cut(first.CustomerName, 100),
                                LedgerType = "Party",
                                MobileNumber = mobile.Length > 0 ? Cut(mobile, 20) : null,
                                EMail = Cut(first.CustomerEmail, 50),
                                Address = Cut(first.CustomerAddress, 200),
                                Gstno = Cut(first.CustomerGstin, 50),
                                DealerCode = dealer,
                                CreatedBy = Cut(userId, 100)!,
                                CreatedDate = DateTime.Now
                            };
                            newLedgers[key] = newLedger;
                        }
                    }
                    else Issue(first, "Warning", "Customer not found in Ledger Master - bill imported without LedgerId (it will not open in the Sale Bill edit screen).");
                }
                else Issue(first, "Warning", "Party Name is blank - bill imported without customer.");

                // ---- sale / customer type ----
                var saleType = NormaliseSaleType(first.SaleType, first.Financier);
                var customerType = (first.CustomerType ?? "").ToUpperInvariant().Contains("B2B") ? "B2B"
                                 : (first.CustomerType ?? "").ToUpperInvariant().Contains("B2C") ? "B2C"
                                 : !string.IsNullOrWhiteSpace(first.CustomerGstin) ? "B2B" : "B2C";

                var header = new VehicleSaleBillHeader
                {
                    SaleBillNo = g.Key,
                    SaleDate = first.SaleDate!.Value,
                    DealerCode = dealer,
                    Location = Cut(locCode, 100),
                    LedgerId = ledgerId,
                    CustomerName = Cut(first.CustomerName, 150),
                    BillingName = Cut(first.BillingName ?? first.CustomerName, 150),
                    CustomerType = customerType,
                    BillType = customerType == "B2B" ? 1 : 2,
                    SaleType = saleType,
                    Financier = financierId,
                    SalesExecutive = Cut(first.SalesExecutive, 100),
                    BookingId = Cut(first.BookingId, 50),
                    TempRegNo = Cut(first.TempRegNo, 50),
                    AccessoryBillNo = Cut(first.AccessoryBillNo, 50),
                    AccessoryAmount = first.AccessoryAmount,
                    Hpamount = first.HpAmount,
                    NoPlateAmount = first.NoPlateAmount,
                    HandlingCharges = first.HandlingCharges,
                    StateSubsidyAmount = first.StateSubsidyAmount,
                    BillFrom = BillFromTag,
                    Status = ImportStatus,
                    IsD2d = false,
                    IsDeleted = false,
                    CreatedBy = Cut(userId, 100)!,
                    CreatedDate = DateTime.Now
                };

                foreach (var r in usable)
                {
                    var d = BuildDetail(r, itemByCode, itemByName, userId, fileName, ws.Name, financierId == null ? first.Financier : null,
                        w => Issue(r, "Warning", w));
                    header.VehicleSaleBillDetails.Add(d);
                }

                var detailTotal = header.VehicleSaleBillDetails.Sum(x => x.FinalAmount);
                header.TotalAmount = first.TotalAmount
                    ?? detailTotal + (first.AccessoryAmount ?? 0) + (first.NoPlateAmount ?? 0)
                       + (first.HandlingCharges ?? 0) + (first.HpAmount ?? 0) - (first.StateSubsidyAmount ?? 0);

                ready.Add((header, newLedger, usable));
            }

            result.BillsReady = ready.Count;
            result.VehiclesReady = ready.Sum(x => x.Header.VehicleSaleBillDetails.Count);
            result.CustomersCreated = newLedgers.Count;
            result.TotalAmount = ready.Sum(x => x.Header.TotalAmount ?? 0);
            if (ready.Count > 0)
            {
                result.FromDate = ready.Min(x => x.Header.SaleDate);
                result.ToDate = ready.Max(x => x.Header.SaleDate);
            }

            // ---------- 6. write ----------
            if (!options.DryRun && ready.Count > 0)
            {
                var prevDetect = _context.ChangeTracker.AutoDetectChangesEnabled;
                _context.ChangeTracker.AutoDetectChangesEnabled = false;
                try
                {
                    // 6a. new customer ledgers (one batch, so the bills can reference their Id)
                    if (newLedgers.Count > 0)
                    {
                        foreach (var grp in newLedgers.Values.GroupBy(l => l.DealerCode!))
                        {
                            var next = await NextLedgerNumber(grp.Key);
                            var suffix = grp.Key.Length >= 4 ? grp.Key[^4..] : grp.Key;
                            foreach (var l in grp) l.LedgerCode = $"LED/{suffix}/{next++}";
                        }
                        _context.LedgerMasters.AddRange(newLedgers.Values);
                        await _context.SaveChangesAsync();
                        _context.ChangeTracker.Clear();
                    }
                    foreach (var x in ready.Where(x => x.NewLedger != null)) x.Header.LedgerId = x.NewLedger!.Id;

                    // 6b. bills in chunks; a failing chunk is retried bill-by-bill to isolate the bad one
                    foreach (var chunk in ready.Chunk(SaveChunk))
                    {
                        try
                        {
                            _context.VehicleSaleBillHeaders.AddRange(chunk.Select(c => c.Header));
                            await _context.SaveChangesAsync();
                            result.BillsImported += chunk.Length;
                            result.VehiclesImported += chunk.Sum(c => c.Header.VehicleSaleBillDetails.Count);
                        }
                        catch (Exception)
                        {
                            _context.ChangeTracker.Clear();
                            foreach (var c in chunk)
                            {
                                try
                                {
                                    ResetKeys(c.Header);
                                    _context.VehicleSaleBillHeaders.Add(c.Header);
                                    await _context.SaveChangesAsync();
                                    result.BillsImported++;
                                    result.VehiclesImported += c.Header.VehicleSaleBillDetails.Count;
                                }
                                catch (Exception exOne)
                                {
                                    _context.ChangeTracker.Clear();
                                    var msg = exOne.GetBaseException().Message;
                                    foreach (var r in c.Rows) Issue(r, "Error", "Database rejected this bill: " + msg);
                                    result.RowsWithErrors += c.Rows.Count;
                                    _logger.LogWarning(exOne, "Cogxim import: bill {Bill} failed", c.Header.SaleBillNo);
                                }
                            }
                        }
                        _context.ChangeTracker.Clear();
                    }
                }
                finally
                {
                    _context.ChangeTracker.AutoDetectChangesEnabled = prevDetect;
                }
                _logger.LogInformation("Cogxim import {File}/{Sheet} by {User}: {Bills} bills, {Vehicles} vehicles, {Customers} customers created",
                    fileName, ws.Name, userId, result.BillsImported, result.VehiclesImported, result.CustomersCreated);
            }

            var done = options.DryRun ? ready.Select(x => x.Header) : ready.Where(x => x.Header.Id > 0).Select(x => x.Header);
            result.Dealers = done.GroupBy(h => h.DealerCode!)
                .Select(gp => new ImportDealerSummary
                {
                    DealerCode = gp.Key,
                    Bills = gp.Count(),
                    Vehicles = gp.Sum(h => h.VehicleSaleBillDetails.Count),
                    Amount = gp.Sum(h => h.TotalAmount ?? 0)
                })
                .OrderByDescending(x => x.Vehicles).ToList();

            result.Warnings = issues.Count(i => i.Level == "Warning");
            result.IssuesTruncated = issues.Count > MaxIssues;
            result.Issues = issues
                .OrderBy(i => i.Level == "Error" ? 0 : i.Level == "Skipped" ? 1 : 2).ThenBy(i => i.RowNo)
                .Take(MaxIssues).ToList();
            return result;
        }

        // ═════════════════════════════════════════════════════════════════════
        // helpers
        // ═════════════════════════════════════════════════════════════════════
        private sealed class ImportRow
        {
            public int RowNo;
            public List<string> Errors = new();
            public string? DealerCode, SaleBillNo, FinalBillNo, SaleDateRaw, BookingId, CustomerName, BillingName, CustomerMobile,
                CustomerEmail, CustomerAddress, CustomerGstin, Location, SalesExecutive, SaleType, Financier, CustomerType,
                TempRegNo, AccessoryBillNo;
            public DateTime? SaleDate, InsStartDate, InsExpDate;
            public decimal? AccessoryAmount, HpAmount, NoPlateAmount, HandlingCharges, StateSubsidyAmount, TotalAmount;
            public string? ChassisNo, ModelName, OemModel, ItemCode, Colour, RegNo, InsNo, MotorNo, Battery, BatteryMake,
                BatteryChemical, BatteryCapacity, ChargerNo, ControllerNo, ConvertorNo, Vcu, KeyNo, BookNo, Segment,
                InstitutionalType, SchemeName, Narration;
            public int? MfgYear;
            public decimal? ItemRate, PreGst, PostGst, FameII, SgstPer, SgstAmt, CgstPer, CgstAmt, IgstPer, IgstAmt,
                RegAmount, InsAmount, FinalAmount;
        }

        private ImportRow ParseRow(IXLRow xr, int rowNo, Dictionary<string, int> colOf, string? defaultDealer)
        {
            var r = new ImportRow { RowNo = rowNo };
            string? S(string key) => colOf.TryGetValue(key, out var c) ? Text(xr.Cell(c)) : null;
            decimal? N(string key)
            {
                if (!colOf.TryGetValue(key, out var c)) return null;
                var v = Number(xr.Cell(c), out var ok);
                if (!ok) r.Errors.Add($"\"{Text(xr.Cell(c))}\" in {Fields.First(f => f.Key == key).Label} is not a number.");
                return v;
            }
            DateTime? D(string key, out string? raw)
            {
                raw = null;
                if (!colOf.TryGetValue(key, out var c)) return null;
                raw = Text(xr.Cell(c));
                return Date(xr.Cell(c));
            }

            r.DealerCode = S("dealerCode") ?? defaultDealer?.Trim();
            if (string.IsNullOrWhiteSpace(r.DealerCode)) r.DealerCode = defaultDealer?.Trim();
            r.SaleBillNo = S("saleBillNo");
            r.SaleDate = D("saleDate", out var raw); r.SaleDateRaw = raw;
            r.BookingId = S("bookingId");
            r.CustomerName = S("customerName");
            r.BillingName = S("billingName");
            r.CustomerMobile = S("customerMobile");
            r.CustomerEmail = S("customerEmail");
            r.CustomerAddress = S("customerAddress");
            r.CustomerGstin = S("customerGstin");
            r.Location = S("location");
            r.SalesExecutive = S("salesExecutive");
            r.SaleType = S("saleType");
            r.Financier = S("financier");
            r.CustomerType = S("customerType");
            r.TempRegNo = S("tempRegNo");
            r.AccessoryBillNo = S("accessoryBillNo");
            r.AccessoryAmount = N("accessoryAmount");
            r.HpAmount = N("hpAmount");
            r.NoPlateAmount = N("noPlateAmount");
            r.HandlingCharges = N("handlingCharges");
            r.StateSubsidyAmount = N("stateSubsidyAmount");
            r.TotalAmount = N("totalAmount");

            r.ChassisNo = S("chassisNo")?.Replace(" ", "").ToUpperInvariant();
            r.ModelName = S("modelName");
            r.OemModel = S("oemModel");
            r.ItemCode = S("itemCode");
            r.Colour = S("colour");
            var year = N("mfgYear");
            r.MfgYear = year is >= 1990 and <= 2100 ? (int)year.Value : null;
            r.ItemRate = N("itemRate");
            r.PreGst = N("preGstDiscount");
            r.PostGst = N("postGstDiscount");
            r.FameII = N("fameII");
            r.SgstPer = N("sgstPer"); r.SgstAmt = N("sgstAmount");
            r.CgstPer = N("cgstPer"); r.CgstAmt = N("cgstAmount");
            r.IgstPer = N("igstPer"); r.IgstAmt = N("igstAmount");
            r.RegAmount = N("regAmount");
            r.InsAmount = N("insuranceAmount");
            r.FinalAmount = N("finalAmount");
            r.RegNo = S("regNo");
            r.InsNo = S("insNo");
            r.InsStartDate = D("insStartDate", out _);
            r.InsExpDate = D("insExpDate", out _);
            r.MotorNo = S("motorNo");
            r.Battery = S("battery");
            r.BatteryMake = S("batteryMake");
            r.BatteryChemical = S("batteryChemical");
            r.BatteryCapacity = S("batteryCapacity");
            r.ChargerNo = S("chargerNo");
            r.ControllerNo = S("controllerNo");
            r.ConvertorNo = S("convertorNo");
            r.Vcu = S("vcu");
            r.KeyNo = S("keyNo");
            r.BookNo = S("bookNo");
            r.Segment = S("segment");
            r.InstitutionalType = S("institutionalType");
            r.SchemeName = S("schemeName");
            r.Narration = S("narration");
            return r;
        }

        private static VehicleSaleBillDetail BuildDetail(ImportRow r, Dictionary<string, string> itemByCode, Dictionary<string, string> itemByName,
            string userId, string fileName, string sheet, string? unmatchedFinancier, Action<string> warn)
        {
            // ItemCode: explicit code, else match model / OEM model name against Item Master
            string? itemCode = null;
            if (!string.IsNullOrWhiteSpace(r.ItemCode) && itemByCode.TryGetValue(Norm(r.ItemCode), out var c1)) itemCode = c1;
            else if (!string.IsNullOrWhiteSpace(r.ModelName) && itemByName.TryGetValue(Norm(r.ModelName), out var c2)) itemCode = c2;
            else if (!string.IsNullOrWhiteSpace(r.OemModel) && itemByName.TryGetValue(Norm(r.OemModel), out var c3)) itemCode = c3;
            if (itemCode == null)
            {
                itemCode = r.ItemCode;
                warn($"Model \"{r.ModelName ?? r.OemModel ?? r.ItemCode}\" not found in Item Master - model-wise reports will show it as Unmapped.");
            }

            var rate = r.ItemRate ?? 0;
            var pre = r.PreGst ?? 0;
            var taxable = rate - pre;
            decimal Tax(decimal? amt, decimal? per) => amt ?? Math.Round(taxable * (per ?? 0) / 100m, 2);
            var sgst = Tax(r.SgstAmt, r.SgstPer);
            var cgst = Tax(r.CgstAmt, r.CgstPer);
            var igst = Tax(r.IgstAmt, r.IgstPer);
            var final = r.FinalAmount
                        ?? taxable + sgst + cgst + igst + (r.RegAmount ?? 0) + (r.InsAmount ?? 0) - (r.PostGst ?? 0) - (r.FameII ?? 0);
            if (final <= 0) warn("Net Amount is 0 or negative.");

            var notes = new List<string>();
            if (!string.IsNullOrWhiteSpace(r.Narration)) notes.Add(r.Narration!);
            if (!string.IsNullOrWhiteSpace(r.MotorNo)) notes.Add("Motor: " + r.MotorNo);
            if (!string.IsNullOrWhiteSpace(unmatchedFinancier)) notes.Add("Financier: " + unmatchedFinancier);
            notes.Add($"Cogxim import {Path.GetFileNameWithoutExtension(fileName)}/{sheet} row {r.RowNo}");

            return new VehicleSaleBillDetail
            {
                ChassisNo = r.ChassisNo!,
                ItemCode = Cut(itemCode, 50),
                ModelName = Cut(r.ModelName ?? r.OemModel, 100),
                Colour = Cut(r.Colour, 50),
                MfgYear = r.MfgYear,
                ItemRate = rate,
                PreGstDiscount = pre,
                PostGstDisc = r.PostGst ?? 0,
                FameIi = r.FameII ?? 0,
                Sgstper = r.SgstPer, Sgstamnt = sgst,
                Cgstper = r.CgstPer, Cgstamnt = cgst,
                Igstper = r.IgstPer, Igstamnt = igst,
                RegAmount = r.RegAmount ?? 0,
                InsuranceAmount = r.InsAmount ?? 0,
                FinalAmount = final,
                RegNo = Cut(r.RegNo, 50),
                InsNo = Cut(r.InsNo, 50),
                InsStartDate = r.InsStartDate,
                InsExpDate = r.InsExpDate,
                Battery = Cut(r.Battery, 50),
                BatteryMake = Cut(r.BatteryMake, 50),
                BatteryChemical = Cut(r.BatteryChemical, 50),
                BatteryCapacity = Cut(r.BatteryCapacity, 50),
                ChargerNo = Cut(r.ChargerNo, 50),
                ControllerNo = Cut(r.ControllerNo, 50),
                ConvertorNo = Cut(r.ConvertorNo, 50),
                Vcu = Cut(r.Vcu, 50),
                Key = Cut(r.KeyNo, 50),
                BookNo = Cut(r.BookNo, 50),
                Segment = Cut(r.Segment, 50),
                InstitutionalType = Cut(r.InstitutionalType, 50),
                SchemeName = Cut(r.SchemeName, 100),
                Narration = Cut(string.Join(" | ", notes), 255),
                IsDelivered = true,
                HasDevice = false,
                HasKit = false,
                IsAgainstExchange = false,
                CreatedBy = Cut(userId, 100)!,
                CreatedDate = DateTime.Now
            };
        }

        private async Task<int> NextLedgerNumber(string dealerCode)
        {
            // Same LED/{last4}/{n} series as LedgerMasterRepo.GetNextLedCode
            var codes = await _context.LedgerMasters.AsNoTracking()
                .Where(x => x.DealerCode == dealerCode && x.LedgerCode != null && x.LedgerCode.StartsWith("LED/"))
                .Select(x => x.LedgerCode!).ToListAsync();
            var max = 0;
            foreach (var c in codes)
            {
                var p = c.Split('/');
                if (p.Length == 3 && int.TryParse(p[2], out var n) && n > max) max = n;
            }
            return max + 1;
        }

        private static void ResetKeys(VehicleSaleBillHeader h)
        {
            h.Id = 0;
            foreach (var d in h.VehicleSaleBillDetails) { d.Id = 0; d.VehicleSaleBillId = 0; }
        }

        private static string NormaliseSaleType(string? raw, string? financier)
        {
            var v = (raw ?? "").Trim().ToLowerInvariant();
            if (v.Contains("cash")) return "Cash";
            if (v.Contains("credit") || v.Contains("financ") || v.Contains("loan") || v.Contains("emi")) return "Credit";
            return string.IsNullOrWhiteSpace(financier) ? "Cash" : "Credit";
        }

        private static IXLWorksheet PickSheet(XLWorkbook wb, string? sheetName)
        {
            if (!string.IsNullOrWhiteSpace(sheetName))
                return wb.Worksheets.FirstOrDefault(s => string.Equals(s.Name, sheetName, StringComparison.OrdinalIgnoreCase))
                       ?? throw new ArgumentException($"Sheet \"{sheetName}\" not found.");
            return wb.Worksheets.FirstOrDefault(s => s.Name.Contains("cogxim", StringComparison.OrdinalIgnoreCase)
                                                    || s.Name.Contains("saleregister", StringComparison.OrdinalIgnoreCase))
                   ?? wb.Worksheets.First();
        }

        private sealed record HeaderCol(string Title, int Column);

        private static List<HeaderCol> ReadHeaders(IXLWorksheet ws, int headerRow)
        {
            var list = new List<HeaderCol>();
            var seen = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var row = ws.Row(headerRow);
            var last = row.LastCellUsed()?.Address.ColumnNumber ?? 0;
            for (var c = 1; c <= last; c++)
            {
                var title = Text(row.Cell(c));
                if (string.IsNullOrWhiteSpace(title)) continue;
                if (seen.TryGetValue(title, out var n)) { seen[title] = n + 1; title = $"{title} ({n + 1})"; }
                else seen[title] = 1;
                list.Add(new HeaderCol(title, c));
            }
            if (list.Count == 0) throw new ArgumentException($"Row {headerRow} of sheet \"{ws.Name}\" has no column titles.");
            return list;
        }

        private static Dictionary<string, string> SuggestMapping(List<string> headers)
        {
            var map = new Dictionary<string, string>();
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var norm = headers.ToDictionary(h => h, Norm);

            // pass 1: exact alias match, in alias priority order
            foreach (var f in Fields)
                foreach (var a in f.Aliases)
                {
                    var h = headers.FirstOrDefault(x => !used.Contains(x) && norm[x] == a);
                    if (h == null) continue;
                    map[f.Key] = h; used.Add(h); break;
                }
            // pass 2: header contains a long alias (e.g. "Party Mobile No" -> partymobile)
            foreach (var f in Fields.Where(f => !map.ContainsKey(f.Key)))
                foreach (var a in f.Aliases.Where(a => a.Length >= 6))
                {
                    var h = headers.FirstOrDefault(x => !used.Contains(x) && norm[x].Contains(a));
                    if (h == null) continue;
                    map[f.Key] = h; used.Add(h); break;
                }
            return map;
        }

        private static Dictionary<string, int> ResolveMapping(Dictionary<string, string>? mapping, List<HeaderCol> headers)
        {
            mapping ??= new();
            if (mapping.Count == 0) mapping = SuggestMapping(headers.Select(h => h.Title).ToList());
            var result = new Dictionary<string, int>();
            foreach (var kv in mapping)
            {
                if (string.IsNullOrWhiteSpace(kv.Value) || Fields.All(f => f.Key != kv.Key)) continue;
                var h = headers.FirstOrDefault(x => string.Equals(x.Title, kv.Value.Trim(), StringComparison.OrdinalIgnoreCase));
                if (h == null) throw new ArgumentException($"Column \"{kv.Value}\" is not in the sheet.");
                result[kv.Key] = h.Column;
            }
            return result;
        }

        // ---------- cell conversion ----------
        private static string? Text(IXLCell cell)
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

        private static decimal? Number(IXLCell cell, out bool ok)
        {
            ok = true;
            var v = cell.Value;
            if (v.IsBlank) return null;
            if (v.IsNumber) return (decimal)v.GetNumber();
            var s = Text(cell);
            if (s == null) return null;
            var neg = s.StartsWith("(") && s.EndsWith(")");
            s = new string(s.Where(ch => char.IsDigit(ch) || ch == '.' || ch == '-').ToArray());
            if (s.Length == 0 || s == "-") return null;
            if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var d)) return neg ? -d : d;
            ok = false;
            return null;
        }

        private static readonly string[] DateFormats =
        {
            "dd-MM-yyyy", "dd/MM/yyyy", "dd.MM.yyyy", "d-M-yyyy", "d/M/yyyy", "d.M.yyyy",
            "dd-MMM-yyyy", "dd MMM yyyy", "d-MMM-yy", "dd-MM-yy", "dd/MM/yy", "yyyy-MM-dd", "yyyy/MM/dd",
            "dd-MM-yyyy HH:mm:ss", "dd/MM/yyyy HH:mm:ss", "dd-MM-yyyy hh:mm:ss tt", "dd/MM/yyyy hh:mm:ss tt",
            "yyyy-MM-dd HH:mm:ss", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-dd HH:mm:ss.fff", "dd/MM/yyyy HH:mm", "dd-MM-yyyy HH:mm"
        };

        private static DateTime? Date(IXLCell cell)
        {
            var v = cell.Value;
            if (v.IsBlank) return null;
            if (v.IsDateTime) return v.GetDateTime();
            if (v.IsNumber)
            {
                var n = v.GetNumber();
                return n is > 20000 and < 80000 ? DateTime.FromOADate(n) : null;   // Excel serial 1954-2119
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

        private static string Digits(string? s) => s == null ? "" : new string(s.Where(char.IsDigit).ToArray());

        private static string? Cut(string? s, int max)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            s = s.Trim();
            return s.Length <= max ? s : s[..max];
        }
    }
}
