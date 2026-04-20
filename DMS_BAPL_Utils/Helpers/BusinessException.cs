using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.Helpers
{
    public class BusinessException : Exception
    {
        public BusinessException(string message) : base(message) { }
    }
}
