using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels.Erp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DMS_BAPL_Data.Services.VehicleSalesService;

public interface IErpVsrSyncService
{
    Task<(int fetched, int inserted, int updated)> SyncVsrForDateAsync(DateTime date);
    Task<(int fetched, int inserted, int updated)> SyncVsrForRangeAsync(DateTime from, DateTime to);
}

public class ErpVsrSyncService : IErpVsrSyncService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _config;
    private readonly BapldmsvadContext _db;
    private readonly ILogger<ErpVsrSyncService> _logger;

    // simple in-memory token cache — no extra table needed for this project
    private static string? _cachedToken;
    private static DateTime _tokenExpiresAt = DateTime.MinValue;
    private static readonly SemaphoreSlim _tokenLock = new(1, 1);

    public ErpVsrSyncService(
        IHttpClientFactory httpFactory,
        IConfiguration config,
        BapldmsvadContext db,
        ILogger<ErpVsrSyncService> logger)
    {
        _httpFactory = httpFactory;
        _config = config;
        _db = db;
        _logger = logger;
    }

    private async Task<string> GetTokenAsync()
    {
        if (_cachedToken != null && DateTime.UtcNow < _tokenExpiresAt)
            return _cachedToken;

        await _tokenLock.WaitAsync();
        try
        {
            if (_cachedToken != null && DateTime.UtcNow < _tokenExpiresAt)
                return _cachedToken;

            var baseUrl = _config["AutoGeniusERP:BaseUrl"]!;
            var loginUrl = baseUrl + _config["AutoGeniusERP:LoginUrl"];

            var req = new LoginRequest
            {
                Username = _config["AutoGeniusERP:Username"]!,
                Password = _config["AutoGeniusERP:PasswordBase64"]!
            };

            using var http = _httpFactory.CreateClient("ErpApi");
            var content = new StringContent(JsonConvert.SerializeObject(req),
                System.Text.Encoding.UTF8, "application/json");

            var resp = await http.PostAsync(loginUrl, content);
            var body = await resp.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<ErpApiResponse<LoginValue>>(body)
                ?? throw new Exception("Login response was null");

            if (!result.Valid || result.Value == null || !result.Value.Any())
                throw new Exception($"ERP login failed: {result.Description}");

            var val = result.Value.First();
            var refreshHours = _config.GetValue<int>("AutoGeniusERP:TokenRefreshHours", 23);

            _cachedToken = val.AccessToken!;
            _tokenExpiresAt = DateTime.UtcNow.AddHours(refreshHours);

            _logger.LogInformation("ERP token refreshed, valid until {exp}", _tokenExpiresAt);
            return _cachedToken;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    public async Task<(int, int, int)> SyncVsrForDateAsync(DateTime date)
        => await SyncVsrForRangeAsync(date, date);

    public async Task<(int fetched, int inserted, int updated)> SyncVsrForRangeAsync(DateTime from, DateTime to)
    {
        var token = await GetTokenAsync();
        var dealerCodes = _config.GetSection("AutoGeniusERP:DealerCodes").Get<List<string>>() ?? new();
        var vendorId = _config.GetValue<int>("AutoGeniusERP:VendorId", 14);

        if (!dealerCodes.Any())
        {
            _logger.LogWarning("No dealer codes configured under AutoGeniusERP:DealerCodes — nothing to sync.");
            return (0, 0, 0);
        }

        int fetched = 0, inserted = 0, updated = 0;
        var delayMs = _config.GetValue<int>("AutoGeniusERP:ApiDelayMs", 500);

        foreach (var dealerCode in dealerCodes)
        {
            try
            {
                var sales = await FetchVsrAsync(dealerCode, from, to, vendorId, token);
                fetched += sales.Count;

                _logger.LogInformation("Dealer {dc}: {n} raw records fetched", dealerCode, sales.Count);

                var deduped = sales
                    .Where(s => !string.IsNullOrEmpty(s.InvoiceNo) && !string.IsNullOrEmpty(s.DealerCode))
                    .GroupBy(s => $"{s.DealerCode}|{s.InvoiceNo}")
                    .Select(g => g.Last())
                    .ToList();

                var droppedCount = sales.Count - deduped.Count;
                if (droppedCount > 0)
                {
                    _logger.LogWarning(
                        "Dealer {dc}: {dropped} of {total} records dropped by InvoiceNo/DealerCode filter or dedup",
                        dealerCode, droppedCount, sales.Count);
                }

                if (!deduped.Any())
                {
                    _logger.LogWarning("Dealer {dc}: 0 records survived filtering — nothing to insert/update. " +
                        "Check whether 'Invoice No' / 'Dealer Code' are actually populated in the raw API response.",
                        dealerCode);
                    continue;
                }

                foreach (var sale in deduped)
                {
                    try
                    {
                        var parsed = MapToEntity(sale);

                        if (string.IsNullOrEmpty(parsed.DealerName) || parsed.NetAmount == 0)
                        {
                            _logger.LogWarning(
                                "VSR field-drift check: invoice {inv} dealer {dc} parsed DealerName='{dn}' NetAmount={na}.",
                                parsed.InvoiceNo, dealerCode, parsed.DealerName, parsed.NetAmount);
                        }

                        var existing = await _db.ErpVehicleSales
                            .FirstOrDefaultAsync(x => x.DealerCode == parsed.DealerCode && x.InvoiceNo == parsed.InvoiceNo);

                        if (existing == null)
                        {
                            parsed.CreatedAt = DateTime.UtcNow;
                            parsed.UpdatedAt = DateTime.UtcNow;
                            parsed.LastSyncedAt = DateTime.UtcNow;
                            _db.ErpVehicleSales.Add(parsed);
                            inserted++;
                        }
                        else
                        {
                            CopyFields(existing, parsed);
                            existing.UpdatedAt = DateTime.UtcNow;
                            existing.LastSyncedAt = DateTime.UtcNow;
                            updated++;
                        }
                    }
                    catch (Exception itemEx)
                    {
                        // THIS is the part that was missing — per-item try/catch so one
                        // bad record can't silently abort the whole dealer's batch.
                        _logger.LogError(itemEx,
                            "Failed to process invoice {inv} for dealer {dc}: {msg}",
                            sale.InvoiceNo, dealerCode, itemEx.Message);
                    }
                }

                await _db.SaveChangesAsync();
                await Task.Delay(delayMs);
            }
            catch (Exception ex)
            {
                // Log full exception, not just message — this is what was hiding the real cause
                _logger.LogError(ex, "VSR sync failed for dealer {dc}: {msg}", dealerCode, ex.ToString());
            }
        }

        _logger.LogInformation("VSR sync {from}→{to}: {f} fetched, {i} inserted, {u} updated",
            from.ToString("dd-MM-yyyy"), to.ToString("dd-MM-yyyy"), fetched, inserted, updated);

        return (fetched, inserted, updated);
    }

    private async Task<List<VsrValue>> FetchVsrAsync(
        string dealerCode, DateTime from, DateTime to, int vendorId, string token)
    {
        var baseUrl = _config["AutoGeniusERP:BaseUrl"]!;
        var url = baseUrl + _config["AutoGeniusERP:VsrUrl"];

        var req = new VsrRequest
        {
            DealerCode = dealerCode,
            VendorId = vendorId,
            StartDate = from.ToString("dd-MM-yyyy"),
            EndDate = to.ToString("dd-MM-yyyy"),
            SubVendorCode = "",
            DealerStatus = "1",
            AadharPanReq = "0",
            FameReq = "2"
        };

        var timeoutSeconds = _config.GetValue<int>("AutoGeniusERP:HttpTimeoutSeconds", 120);

        using var http = _httpFactory.CreateClient("ErpApi");
        using var msg = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonConvert.SerializeObject(req),
                System.Text.Encoding.UTF8, "application/json")
        };
        msg.Headers.TryAddWithoutValidation("Authorization", $"Token {token}");

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
        var resp = await http.SendAsync(msg, cts.Token);
        var bytes = await resp.Content.ReadAsByteArrayAsync();
        var body = StripBom(bytes);

        var result = JsonConvert.DeserializeObject<ErpApiResponse<VsrValue>>(body,
            new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Ignore });

        return result?.Valid == true ? result.Value ?? new() : new();
    }

    private static string StripBom(byte[] bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            return System.Text.Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
        return System.Text.Encoding.UTF8.GetString(bytes);
    }

    private static ErpVehicleSale MapToEntity(VsrValue v) => new()
    {
        DealerName = v.DealerName,
        DealerCode = v.DealerCode,
        InvoiceNo = v.InvoiceNo,
        InvoiceDate = ParseDate(v.InvoiceDate),
        Location = v.Location,
        LocCode = v.LocCode,
        LocationCity = v.LocationCity,
        CustDob = ParseDate(v.CustDOB),
        Gender = v.Gender,
        SoldTo = v.SoldTo,
        AccountType = v.AccountType,
        PartyEmail = v.PartyEmail,
        CusMob = v.CusMob,
        Address1 = v.Address1,
        Address2 = v.Address2,
        City = v.City,
        State = v.State,
        ExecutiveName = v.ExecutiveName,
        Pin = v.Pin,
        ChassisNo = v.ChassisNo,
        MotorNo = v.MotorNo,
        Remarks = v.Remarks,
        ItemModel = v.ItemModel,
        Oemmodel = v.OEMModel,
        ColorCode = v.ColorCode,
        VehicleType = v.VehicleType,
        VehicleGroup = v.VehicleGroup,
        Hsnsaccode = v.HSNSACCode,
        SaleType = v.SaleType,
        FinancedBy = v.FinancedBy,
        FinAmount = ParseDecimal(v.FinAmount),
        ItemRate = ParseDecimal(v.ItemRate),
        InsuAmount = ParseDecimal(v.InsuAmount),
        RegnAmount = ParseDecimal(v.RegnAmount),
        AcsryAmount = ParseDecimal(v.AcsryAmount),
        PreGstdiscAmount = ParseDecimal(v.PreGSTDiscAmount),
        DiscTypeName = v.DiscTypeName,
        PostGstdisc = ParseDecimal(v.PostGSTDisc),
        FameIi = ParseDecimal(v.FameII),
        StateFameIi = ParseDecimal(v.StateFameII),
        Sgstper = ParseDecimal(v.SGSTPer),
        Sgstamount = ParseDecimal(v.SGSTAmount),
        Cgstper = ParseDecimal(v.CGSTPer),
        Cgstamount = ParseDecimal(v.CGSTAmount),
        Igstper = ParseDecimal(v.IGSTPer),
        Igstamount = ParseDecimal(v.IGSTAmount),
        NetAmount = ParseDecimal(v.NetAmount),
        ReferenceNo = v.ReferenceNo,
        BookingDate = ParseDate(v.BookingDate),
        TotalCount = v.TotalCount,
        Battery = v.Battery,
        BatteryChemical = v.BatteryChemical,
        BatteryCapacity = v.BatteryCapacity,
        BatteryMake = v.BatteryMake,
        ChargerNo = v.ChargerNo,
        ChargerNo2 = v.ChargerNo2,
        Converter = v.Converter,
        Vcu = v.VCU,
        ControllerNo = v.ControllerNo,
        FameIirequired = v.FameIIRequired,
        SegmentName = v.SegmentName,
        InstitutionalName = v.InstitutionalName,
        SchemeName = v.SchemeName
    };

    private static void CopyFields(ErpVehicleSale e, ErpVehicleSale n)
    {
        e.DealerName = n.DealerName; e.InvoiceDate = n.InvoiceDate;
        e.Location = n.Location; e.LocCode = n.LocCode; e.LocationCity = n.LocationCity;
        e.CustDob = n.CustDob; e.Gender = n.Gender; e.SoldTo = n.SoldTo;
        e.AccountType = n.AccountType; e.PartyEmail = n.PartyEmail; e.CusMob = n.CusMob;
        e.Address1 = n.Address1; e.Address2 = n.Address2; e.City = n.City;
        e.State = n.State; e.ExecutiveName = n.ExecutiveName; e.Pin = n.Pin;
        e.ChassisNo = n.ChassisNo; e.MotorNo = n.MotorNo; e.Remarks = n.Remarks;
        e.ItemModel = n.ItemModel; e.Oemmodel = n.Oemmodel; e.ColorCode = n.ColorCode;
        e.VehicleType = n.VehicleType; e.VehicleGroup = n.VehicleGroup;
        e.Hsnsaccode = n.Hsnsaccode; e.SaleType = n.SaleType; e.FinancedBy = n.FinancedBy;
        e.FinAmount = n.FinAmount; e.ItemRate = n.ItemRate; e.InsuAmount = n.InsuAmount;
        e.RegnAmount = n.RegnAmount; e.AcsryAmount = n.AcsryAmount;
        e.PreGstdiscAmount = n.PreGstdiscAmount; e.DiscTypeName = n.DiscTypeName;
        e.PostGstdisc = n.PostGstdisc; e.FameIi = n.FameIi; e.StateFameIi = n.StateFameIi;
        e.Sgstper = n.Sgstper; e.Sgstamount = n.Sgstamount;
        e.Cgstper = n.Cgstper; e.Cgstamount = n.Cgstamount;
        e.Igstper = n.Igstper; e.Igstamount = n.Igstamount; e.NetAmount = n.NetAmount;
        e.ReferenceNo = n.ReferenceNo; e.BookingDate = n.BookingDate;
        e.TotalCount = n.TotalCount; e.Battery = n.Battery;
        e.BatteryChemical = n.BatteryChemical; e.BatteryCapacity = n.BatteryCapacity;
        e.BatteryMake = n.BatteryMake; e.ChargerNo = n.ChargerNo;
        e.ChargerNo2 = n.ChargerNo2; e.Converter = n.Converter;
        e.Vcu = n.Vcu; e.ControllerNo = n.ControllerNo;
        e.FameIirequired = n.FameIirequired; e.SegmentName = n.SegmentName;
        e.InstitutionalName = n.InstitutionalName; e.SchemeName = n.SchemeName;
    }

    private static DateOnly? ParseDate(string? val)
    {
        if (string.IsNullOrWhiteSpace(val)) return null;
        if (DateTime.TryParseExact(val, "dd-MM-yyyy",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out var d))
            return DateOnly.FromDateTime(d);
        return DateTime.TryParse(val, out var d2) ? DateOnly.FromDateTime(d2) : null;
    }

    private static decimal ParseDecimal(string? val)
    {
        if (string.IsNullOrWhiteSpace(val)) return 0;
        return decimal.TryParse(val, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : 0;
    }
}