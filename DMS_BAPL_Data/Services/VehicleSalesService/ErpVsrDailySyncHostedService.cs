using DMS_BAPL_Data.Services.VehicleSalesService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DMS_BAPL_Data.Services.VehicleSalesService;

public class ErpVsrDailySyncHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<ErpVsrDailySyncHostedService> _logger;

    public ErpVsrDailySyncHostedService(
        IServiceScopeFactory scopeFactory,
        IConfiguration config,
        ILogger<ErpVsrDailySyncHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var syncHour = _config.GetValue<int>("AutoGeniusERP:SyncHour", 3);
        var syncMinute = _config.GetValue<int>("AutoGeniusERP:SyncMinute", 0);

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var nextRun = new DateTime(now.Year, now.Month, now.Day, syncHour, syncMinute, 0);
            if (now >= nextRun) nextRun = nextRun.AddDays(1);

            var delay = nextRun - now;
            _logger.LogInformation("Next VSR sync scheduled at {time} (in {delay})", nextRun, delay);

            try { await Task.Delay(delay, stoppingToken); }
            catch (TaskCanceledException) { break; }

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<IErpVsrSyncService>();
                var yesterday = DateTime.UtcNow.Date.AddDays(-1);
                var today = DateTime.UtcNow.Date;

                // pulls yesterday + today to catch late-posted invoices too
                var (fetched, inserted, updated) = await svc.SyncVsrForRangeAsync(yesterday, today);

                _logger.LogInformation(
                    "Daily VSR sync done: {f} fetched, {i} inserted, {u} updated",
                    fetched, inserted, updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Daily VSR sync failed");
            }
        }
    }
}