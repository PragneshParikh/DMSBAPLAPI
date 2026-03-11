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

        // Excel Constants
        public const string DealerExcelSheetName = "Dealer List";
        public const string DealerExcelFileName = "DealerList.xlsx";
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
    }
}
