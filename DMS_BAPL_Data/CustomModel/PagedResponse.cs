using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.CustomModel
{
    public class PagedResponse<T>
    {
        public List<T> Data { get; set; }
        public int TotalRecords { get; set; }
    }
}
