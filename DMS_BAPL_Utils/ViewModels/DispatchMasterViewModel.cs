// ViewModels/DispatchMasterViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace DMS_BAPL_Data.ViewModels
{
    public class DispatchMasterViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Master Type")]
        public string MasterType { get; set; }
        
        [Display(Name = "Name")]
        public string MasterName { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
        public string? UpdatedBy { get; set; }
    }

  
    public class DispatchMasterListViewModel
    {
        public int SrNo { get; set; }
        public int Id { get; set; }
        public string MasterType { get; set; }
        public string MasterName { get; set; }
        public bool IsActive { get; set; }
    }

   
    public class DispatchMasterSearchViewModel
    {
        public string MasterType { get; set; }
        public string Name { get; set; }
        public int PerPageRecords { get; set; } = 25;
        public int PageNumber { get; set; } = 1;
    }
}