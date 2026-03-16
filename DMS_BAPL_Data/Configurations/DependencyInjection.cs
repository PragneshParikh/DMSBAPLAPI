using DMS_BAPL_Data;
using DMS_BAPL_Data.Repositories.APITracking;
using DMS_BAPL_Data.Repositories.BatteryCapacityMasterRepo;
using DMS_BAPL_Data.Repositories.Color;
using DMS_BAPL_Data.Repositories.DealerMasterRepository;
using DMS_BAPL_Data.Repositories.Form22MasterRepo;
using DMS_BAPL_Data.Repositories.itemMasterRepo;
using DMS_BAPL_Data.Repositories.LeadMasterRep;
using DMS_BAPL_Data.Repositories.LeadMasterRepo;
using DMS_BAPL_Data.Repositories.LocationMasterRepo;
using DMS_BAPL_Data.Repositories.MenuMasterRepo;
using DMS_BAPL_Data.Repositories.OEMModelMasterRepo;
using DMS_BAPL_Data.Services.APITrackingService;
using DMS_BAPL_Data.Services.BatteryCapacityMasterService;
using DMS_BAPL_Data.Services.ColorMasterService;
using DMS_BAPL_Data.Services.DealerMasterService;
using DMS_BAPL_Data.Services.EmailService;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Data.Services.Form22Services;
using DMS_BAPL_Data.Services.itemMasterService;
using DMS_BAPL_Data.Services.LeadMasterService;
using DMS_BAPL_Data.Services.LocationMasterService;
using DMS_BAPL_Data.Services.MenuMasterService;
using DMS_BAPL_Data.Services.OEMModelMasterService;
using Microsoft.Extensions.DependencyInjection;

namespace DMS_BAPL_Data.Configurations
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddProjectServices(this IServiceCollection services)
        {

            #region Repositories Registration

            services.AddScoped<IitemMasterRepo, ItemMasterRepo>();
            services.AddScoped<ILeadMasterRepo, LeadMasterRepo>();
            services.AddScoped<ILocationMasterRepo, LocationMasterRepo>();
            services.AddScoped<IDealerMasterRepo, DealerMasterRepo>();
            services.AddScoped<IColorMasterRepo, ColorMasterRepo>();
            services.AddScoped<IAPITrackingRepo, APITrackingRepo>();
            services.AddScoped<IForm22MasterRepo, Form22MasterRepo>();
            services.AddScoped<IMenuRepo, MenuRepo>();
            services.AddScoped<IBatteryCapacityMasterRepo, BatteryCapacityMasterRepo>();
            services.AddScoped<IOEMModelMasterRepo, OEMModelMasterRepo>();

            #endregion

            #region Services Registration

            services.AddScoped<IitemMasterService, ItemMasterService>();
            services.AddScoped<ILeadMasterService, LeadMasterService>();
            services.AddScoped<ILocationMasterService, LocationMasterService>();
            services.AddScoped<IDealerMasterService, DealerMasterService>();
            services.AddScoped<IColorMasterService, ColorMasterService>();
            services.AddScoped<IAPITrackingService, APITrackingService>();
            services.AddScoped<IExcelService, ExcelService>();
            services.AddScoped<IForm22Service, Form22Service>();
            services.AddScoped<IMenuService, MenuService>();
            services.AddScoped<IBatteryCapacityMasterService, BatteryCapacityMasterService>();
            services.AddScoped<IOEMModelMasterService, OEMModelMasterService>();

            // Email SErvice
            services.AddScoped<IEmailService, EmailService>();

            #endregion

            return services;
        }
    }
}
