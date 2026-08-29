using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.Constants
{
    [Flags]
    public enum MenuPermission
    {
        None = 0,
        View = 1,
        Create = 2,
        Edit = 4,
        Delete = 8,
        Download = 16
    }
}
