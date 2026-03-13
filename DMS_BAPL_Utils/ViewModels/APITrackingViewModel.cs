using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.ViewModels
{
    public class APITrackingViewModel
    {
        public int Id { get; set; }

        public string Endpoint { get; set; } = null!;

        public DateTime Dateofhit { get; set; }

        public string? Payload { get; set; }

        public string? Status { get; set; }

        public string? Response { get; set; }
    }
}
