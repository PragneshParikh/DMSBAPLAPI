using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public record AssignedDealerInfo(
        int EmployeeId,
        string EmployeeCode,
        string EmployeeName,
        string DealerCode         
    );
}
