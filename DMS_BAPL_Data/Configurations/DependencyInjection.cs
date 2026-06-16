using DMS_BAPL_Data;
using DMS_BAPL_Data.Repositories.AgreeTaxcodeRepo;
using DMS_BAPL_Data.Repositories.APITracking;
using DMS_BAPL_Data.Repositories.BatteryCapacityMasterRepo;
using DMS_BAPL_Data.Repositories.ChassisBatteryDetailRepo;
using DMS_BAPL_Data.Repositories.ChassisDetailRepo;
using DMS_BAPL_Data.Repositories.ChassisDetailsRepo;
using DMS_BAPL_Data.Repositories.ChassisRepo;
using DMS_BAPL_Data.Repositories.CircularAttachmentRepo;
using DMS_BAPL_Data.Repositories.CircularDealerAssignmentRepo;
using DMS_BAPL_Data.Repositories.CircularRepo;
using DMS_BAPL_Data.Repositories.CityRepo;
using DMS_BAPL_Data.Repositories.Color;
using DMS_BAPL_Data.Repositories.ComplaintMasterRepo;
using DMS_BAPL_Data.Repositories.DealerMasterRepository;
using DMS_BAPL_Data.Repositories.DepartmentRepo;
using DMS_BAPL_Data.Repositories.DesignationRepo;
using DMS_BAPL_Data.Repositories.EmployeeMasterRepo;
using DMS_BAPL_Data.Repositories.ExtendedBatteryWarrantyRepo;
using DMS_BAPL_Data.Repositories.FFIRRepo;
using DMS_BAPL_Data.Repositories.Form22MasterRepo;
using DMS_BAPL_Data.Repositories.GroupMasterRepo;
using DMS_BAPL_Data.Repositories.HSNCodeMaterRepo;
using DMS_BAPL_Data.Repositories.HSNWiseTaxCodeRepo;
using DMS_BAPL_Data.Repositories.HSRPRepo;
using DMS_BAPL_Data.Repositories.itemMasterRepo;
using DMS_BAPL_Data.Repositories.JobCardRepo;
using DMS_BAPL_Data.Repositories.KitDetailsRepo;
using DMS_BAPL_Data.Repositories.KitHeaderRepo;
using DMS_BAPL_Data.Repositories.LabourMasterRepo;
using DMS_BAPL_Data.Repositories.LeadMasterRep;
using DMS_BAPL_Data.Repositories.LeadMasterRepo;
using DMS_BAPL_Data.Repositories.LedgerMasterRepo;
using DMS_BAPL_Data.Repositories.LocationMasterRepo;
using DMS_BAPL_Data.Repositories.LOTInspectionRepo;
using DMS_BAPL_Data.Repositories.MaterialTransferRepo;
using DMS_BAPL_Data.Repositories.MenuMasterRepo;
using DMS_BAPL_Data.Repositories.ModelWiseServieScheduleRepo;
using DMS_BAPL_Data.Repositories.OccupationMasterRepo;
using DMS_BAPL_Data.Repositories.OEMModelMasterRepo;
using DMS_BAPL_Data.Repositories.OEMModelWarrantyRepo;
using DMS_BAPL_Data.Repositories.PartInventoryRepo;
using DMS_BAPL_Data.Repositories.PartInwardRepo;
using DMS_BAPL_Data.Repositories.PDIChecklistMasterRepo;
using DMS_BAPL_Data.Repositories.PerformaInvoiceRepo;
using DMS_BAPL_Data.Repositories.PrefixRepo;
using DMS_BAPL_Data.Repositories.PurchaseOrderRepo;
using DMS_BAPL_Data.Repositories.ReceiptEntryRepo;
using DMS_BAPL_Data.Repositories.RepairBillRepo;
using DMS_BAPL_Data.Repositories.ReportRepo;
using DMS_BAPL_Data.Repositories.RoleRepo;
using DMS_BAPL_Data.Repositories.RoleWiseMenuRightRepo;
using DMS_BAPL_Data.Repositories.StateRepo;
using DMS_BAPL_Data.Repositories.TaxCodeMasterRepo;
using DMS_BAPL_Data.Repositories.TermConditionMasterRepo;
using DMS_BAPL_Data.Repositories.VehicleDispatchRepo;
using DMS_BAPL_Data.Repositories.VehicleSaleBillRepo;
using DMS_BAPL_Data.Repositories.VehicleStockTransferRepo;
using DMS_BAPL_Data.Services.AgreetaxcodeService;
using DMS_BAPL_Data.Services.APITrackingService;
using DMS_BAPL_Data.Services.BatteryCapacityMasterService;
using DMS_BAPL_Data.Services.ChassisBatteryDetailService;
using DMS_BAPL_Data.Services.ChassisDetailsService;
using DMS_BAPL_Data.Services.ChassisService;
using DMS_BAPL_Data.Services.CircularAttachmentService;
using DMS_BAPL_Data.Services.CircularDealerAssignmentService;
using DMS_BAPL_Data.Services.CityService;
using DMS_BAPL_Data.Services.ColorMasterService;
using DMS_BAPL_Data.Services.DealerMasterService;
using DMS_BAPL_Data.Services.DepartmentService;
using DMS_BAPL_Data.Services.DesignationService;
using DMS_BAPL_Data.Services.EmailService;
using DMS_BAPL_Data.Services.EmployeeMasterService;
using DMS_BAPL_Data.Services.ExcelServices;
using DMS_BAPL_Data.Services.ExtendedBatteryWarrantyService;
using DMS_BAPL_Data.Services.FileService;
using DMS_BAPL_Data.Services.Form22Services;
using DMS_BAPL_Data.Services.HSNCodeMaterService;
using DMS_BAPL_Data.Services.HSNWiseTaxcodeService;
using DMS_BAPL_Data.Services.HSRPService;
using DMS_BAPL_Data.Services.InventoryService;
using DMS_BAPL_Data.Services.itemMasterService;
using DMS_BAPL_Data.Services.KitDetailsService;
using DMS_BAPL_Data.Services.KitHeaderService;
using DMS_BAPL_Data.Services.LeadMasterService;
using DMS_BAPL_Data.Services.LedgerMasterService;
using DMS_BAPL_Data.Services.LocationMasterService;
using DMS_BAPL_Data.Services.LOTInspectionService;
using DMS_BAPL_Data.Services.MaterialTransferService;
using DMS_BAPL_Data.Services.MenuMasterService;
using DMS_BAPL_Data.Services.NewsBulletinAttachmentService;
using DMS_BAPL_Data.Services.NewsBulletinService;
using DMS_BAPL_Data.Services.OccupationMasterService;
using DMS_BAPL_Data.Services.OEMModelMasterService;
using DMS_BAPL_Data.Services.OEMModelWarrantyService;
using DMS_BAPL_Data.Services.PartsInwardService;
using DMS_BAPL_Data.Services.PerformaInvoiceService;
using DMS_BAPL_Data.Services.PrefixService;
using DMS_BAPL_Data.Services.PurchaseOrder;
using DMS_BAPL_Data.Services.ReceiptEntryService;
using DMS_BAPL_Data.Services.ReportService;
using DMS_BAPL_Data.Services.RoleService;
using DMS_BAPL_Data.Services.RoleWiseMenuRightService;
using DMS_BAPL_Data.Services.StateService;
using DMS_BAPL_Data.Services.TaxCodeMasterService;
using DMS_BAPL_Data.Services.TaxServices;
using DMS_BAPL_Data.Services.VehicleDispatchService;
using DMS_BAPL_Data.Services.VehicleSaleBillService;
using DMS_BAPL_Data.Services.VehicleStockTransferService;
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
            services.AddScoped<IRoleRepo, RoleRepo>();
            services.AddScoped<IRoleWiseMenuRightRepo, RoleWiseMenuRightRepo>();
            services.AddScoped<IAgreetaxcodeRepo, AgreetaxcodeRepo>();
            services.AddScoped<IHSNCodeMaterRepo, HSNCodeMaterRepo>();
            services.AddScoped<ITaxCodeMasterRepo, TaxCodeMasterRepo>();
            services.AddScoped<IHSNWiseTaxcodeRepo, HSNWiseTaxcodeRepo>();
            services.AddScoped<IPurchaseOrderRepo, PurchaseOrderRepo>();
            services.AddScoped<IVehicleInwardRepo, VehicleInwardRepo>();
            services.AddScoped<ILotInspection, LotInspectionRepo>();
            services.AddScoped<ILotInspectionDetails, LotInspectionDetailsRepo>();
            services.AddScoped<IReceiptEntryRepo, ReceiptEntryRepo>();
            services.AddScoped<ILedgerMasterRepo, LedgerMasterRepo>();
            services.AddScoped<IVehicleSaleBillRepo, VehicleSaleBillRepo>();
            services.AddScoped<IKitHeaderRepo, KitHeaderRepo>();
            services.AddScoped<IKitDetailsRepo, KitDetailsRepo>();
            services.AddScoped<IPrefixRepo, PrefixRepo>();
            services.AddScoped<IJobCardRepo, JobCardRepo>();
            services.AddScoped<IPartInventoryRepo, PartInventoryRepo>();
            services.AddScoped<IStateRepo, StateRepo>();
            services.AddScoped<ICityRepo, CityRepo>();
            services.AddScoped<IMaterialTransferRepo, MaterialTransferRepo>();
            services.AddScoped<IOEMModelWarrantyRepo, OEMModelWarrantyRepo>();
            services.AddScoped<IModelwiseServiceSchedule, ModelwiseServiceScheduleRepo>();
            services.AddScoped<IPdiCheckListMaster, PdiChecklistMasterRepo>();
            services.AddScoped<IExtendedBatteryWarrantyRepo, ExtendedBatteryWarrantyRepo>();
            services.AddScoped<IModelwiseServiceSchedule, ModelwiseServiceScheduleRepo>();
            services.AddScoped<IPerformaInvoiceRepo, PerformaInvoiceRepo>();
            services.AddScoped<IFFIRRepo, FFIRRepo>();
            services.AddScoped<IPartInwardRepo, PartInwardRepo>();
            services.AddScoped<IChassisRepo, ChassisRepo>();
            services.AddScoped<IHSRPRepo, HSRPRepo>();
            services.AddScoped<IReportRepo, ReportRepo>();
            services.AddScoped<ILabourMasterRepo, LabourMasterRepo>();
            services.AddScoped<ICircularRepo, CircularRepo>();
            services.AddScoped<ICircularAttachmentRepo, CircularAttachmentRepo>();
            services.AddScoped<IEmployeeMasterRepo, EmployeeMasterRepo>();
            services.AddScoped<IRepairBillRepo, RepairBillRepo>();
            services.AddScoped<IChassisDetailRepo, ChassisDetailRepo>();
            services.AddScoped<IChassisBatteryDetailRepo, ChassisBatteryDetailRepo>();
            services.AddScoped<ICircularDealerAssignmentRepo, CircularDealerAssignmentRepo>();
            services.AddScoped<IVehicleStockTransferRepo, VehicleStockTransferRepo>();
            services.AddScoped<IComplaintMaster, ComplaintMaster>();
            services.AddScoped<IGroupMasterRepo, GroupMasterRepo>();
            services.AddScoped<ITermConditionMasterRepo, TermConditionMasterRepo>();
            services.AddScoped<IDepartmentRepo, DepartmentRepo>();
            services.AddScoped<IDesignationRepo, DesignationRepo>();
            services.AddScoped<IOccupationMasterRepo, OccupationMasterRepo>();

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
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IRoleWiseMenuRightService, RoleWiseMenuRightService>();
            services.AddScoped<IAgreegateTaxcodeService, AgreegateTaxcodeService>();
            services.AddScoped<IHSNCodeMaterService, HSNCodeMaterService>();
            services.AddScoped<ITaxCodeMasterService, TaxCodeMasterService>();
            services.AddScoped<IHSNWiseTaxcodeservice, HSNWiseTaxCodeService>();
            services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
            services.AddScoped<IVehicleInwardService, VehicleInwardService>();
            services.AddScoped<ILotInspectionService, LotInspectionService>();
            services.AddScoped<ILotInspectionDetailsService, LotInspectionDetailsService>();
            services.AddScoped<IReceiptEntryService, ReceiptEntryService>();
            services.AddScoped<ILedgerMasterService, LedgerMasterService>();
            services.AddScoped<IVehicleSaleBillService, VehicleSaleBillService>();
            services.AddScoped<IKitHeaderService, KitHeaderService>();
            services.AddScoped<IKitDetailsService, KitDetailsService>();
            services.AddScoped<IPrefixService, PrefixService>();
            services.AddScoped<IPartInventoryService, PartInventoryService>();
            services.AddScoped<IStateService, StateService>();
            services.AddScoped<ICityService, CityService>();
            services.AddScoped<ITaxServices, TaxServices>();
            services.AddScoped<IOEMModelWarrantyService, OEMModelWarrantyService>();
            services.AddScoped<IMaterialTransferService, MaterialTransferService>();
            services.AddScoped<IExtendedBatteryWarrantyService, ExtendedBatteryWarrantyService>();
            services.AddScoped<IPerformaInvoiceService, PerformaInvoiceService>();
            services.AddScoped<IPartInwardService, PartInwardService>();
            services.AddScoped<IChassisService, ChassisService>();
            services.AddScoped<IHSRPService, HSRPService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<ICircularService, CircularService>();
            services.AddScoped<ICircularAttachmentService, CircularAttachmentService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IChassisDetailService, ChassisDetailService>();
            services.AddScoped<IChassisBatteryDetailService, ChassisBatteryDetailService>();
            services.AddScoped<ICircularDealerAssignmentService, CircularDealerAssignmentService>();
            services.AddScoped<IVehicleStockTransferService, VehicleStockTransferService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<IDesignationService, DesignationService>();
            services.AddScoped<IVehicleStockTransferService,VehicleStockTransferService>();
            services.AddScoped<IOccupationMasterService,OccupationMasterService>();


            // Email SErvice
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IFileService, FileService>();

            #endregion

            return services;
        }
    }
}
