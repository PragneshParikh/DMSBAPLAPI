using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.Constants
{
    public class StringConstants
    {
        //Dealer Master
        public const string DealerCreated = "Dealer created successfully.";
        public const string DealerFetched = "Dealers fetched successfully.";
        public const string DealerNotFound = "No dealers found.";
        public const string DealerCreationFailed = "Failed to create dealer.";
        public const string DealerUpdated = "Dealer updated successfully.";
        public const string DealerUpdateFailed = "Failed to update dealer.";
        public const string DealerDefaultPassword = "Dealer@123";
        public const string DealerText = "Dealer";



        // Excel Constants
        public const string DealerExcelSheetName = "Dealer List";
        public const string ColorExcelSheet = "Color List";
        public const string APIExcelSheetName = "API List";
        public const string BatteryCapacityMasterExcelSheetName = "Battery Capacity Master List";
        public const string DealerExcelFileName = "DealerList.xlsx";
        public const string BatteryCapacityMasterName = "BatteryCapacityMaster.xlsx";
        public const string SlNo = "SL No";

        // Audit Fields (Exclude from Excel)
        public const string CreatedBy = "CreatedBy";
        public const string CreatedDate = "CreatedDate";
        public const string UpdatedDate = "UpdatedDate";
        public const string UpdatedBy = "UpdatedBy";
        public const string ModifiedBy = "ModifiedBy";
        public const string ModifiedDate = "ModifiedDate";
        public const string DeletedBy = "DeletedBy";
        public const string DeletedDate = "DeletedDate";

        //Battery Capacity Master related
        public const string BatteryCapacityMasterCreated ="Battery Capacity Master created successfully.";
        public const string BatteryCapacityMasterUpdateFailed ="Battery Capacity Master Update failed.";
        public const string BatteryCapacityMasterUpdated = "Battery Capacity Master Updated successfully.";

        // Form22Master
        public const string Form22MasterCreated = "Form22Master created successfully.";
        public const string Form22MasterFetched = "Form22Masters fetched successfully.";
        public const string Form22MasterUpdated = "Form22Master updated successfully.";

        // Aggregate Tax Code
        public const string AggregateTaxCodeCreated = "Aggregate Tax Code created successfully.";
        public const string AggregateTaxCodeFetched = "Aggregate Tax Codes fetched successfully.";
        public const string AggregateTaxCodeUpdated = "Aggregate Tax Code updated successfully.";

        //HSNCode Related
        public const string HSNCodeCreatedSuccessfully = "HSN Code created successfully";
        public const string HSNCodeUpdatedSuccessfully = "HSN Code updated successfully";
        public const string HSNCodeExcelSheetName = "HSN Code List";
        public const string HSNCodeExists = "HSN Code already exists";

        //HSNWiseTaxCode Related
        public const string HSNWiseTaxCodeCreatedSuccessfully = "HSN-wise Tax Code created successfully";
    }
}
