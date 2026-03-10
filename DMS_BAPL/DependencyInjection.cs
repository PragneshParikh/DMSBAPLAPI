using DMS_BAPL_Data;
using DMS_BAPL_Data.Repositories.DealerMasterRepository;
using DMS_BAPL_Data.Repositories.itemMasterRepo;
using DMS_BAPL_Data.Repositories.LocationMasterRepo;
using DMS_BAPL_Data.Services.DealerMasterService;
using DMS_BAPL_Data.Services.itemMasterService;
using DMS_BAPL_Data.Services.LocationMasterService;
using DMS_BAPL_Utils;
using Microsoft.Extensions.DependencyInjection;

namespace DMS_BAPL_Api
{
        public static class DependencyInjection
        {
            public static IServiceCollection AddProjectServices(this IServiceCollection services)
        {

            
            
            // Repository Registration
            services.AddScoped<IitemMasterRepo, ItemMasterRepo>();

            services.AddScoped<ILocationMasterRepo, LocationMasterRepo>();
            services.AddScoped<IDealerMasterRepo, DealerMasterRepo>();
            // Service Registration
            services.AddScoped<IitemMasterService, ItemMasterService>();
            services.AddScoped<ILocationMasterService, LocationMasterService>();
            services.AddScoped<IDealerMasterService, DealerMasterService>();

            return services;
            }
        }
    }
