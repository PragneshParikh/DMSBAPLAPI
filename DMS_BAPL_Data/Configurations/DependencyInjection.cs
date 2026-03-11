using DMS_BAPL_Data;
using DMS_BAPL_Data.Repositories.APITracking;
using DMS_BAPL_Data.Repositories.Color;
using DMS_BAPL_Data.Repositories.DealerMasterRepository;
using DMS_BAPL_Data.Repositories.itemMasterRepo;
using DMS_BAPL_Data.Repositories.LocationMasterRepo;
using DMS_BAPL_Data.Services.APITrackingService;
using DMS_BAPL_Data.Services.ColorMasterService;
using DMS_BAPL_Data.Services.DealerMasterService;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Data.Services.itemMasterService;
using DMS_BAPL_Data.Services.LocationMasterService;
using Microsoft.Extensions.DependencyInjection;

namespace DMS_BAPL_Data.Configurations
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddProjectServices(this IServiceCollection services)
        {

            #region Repositories Registration

            services.AddScoped<IitemMasterRepo, ItemMasterRepo>();
            services.AddScoped<ILocationMasterRepo, LocationMasterRepo>();
            services.AddScoped<IDealerMasterRepo, DealerMasterRepo>();
            services.AddScoped<IColorMasterRepo, ColorMasterRepo>();
            services.AddScoped<IAPITrackingRepo, APITrackingRepo>();

            #endregion

            #region Services Registration

            services.AddScoped<IitemMasterService, ItemMasterService>();
            services.AddScoped<ILocationMasterService, LocationMasterService>();
            services.AddScoped<IDealerMasterService, DealerMasterService>();
            services.AddScoped<IColorMasterService, ColorMasterService>();
            services.AddScoped<IAPITrackingService, APITrackingService>();
            services.AddScoped<IExcelService, ExcelService>();

            #endregion

            return services;
        }
    }
}
