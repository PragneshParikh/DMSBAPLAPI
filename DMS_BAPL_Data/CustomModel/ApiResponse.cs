using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.CustomModel
{
    public class ApiResponse
    {
        public bool Valid { get; set; }
        public string? Description { get; set; } = null;
        public object? Value { get; set; } = null;
    }
}
