using System;
using System.Collections.Generic;

namespace DMS_BAPL_Utils.ViewModels
{
    // =========================================================
    // Simple lookup ViewModel — only Id and ProfileName
    // =========================================================

    public class EmployeeProfileMasterViewModel
    {
        public int Id { get; set; }
        public string ProfileName { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }    
}