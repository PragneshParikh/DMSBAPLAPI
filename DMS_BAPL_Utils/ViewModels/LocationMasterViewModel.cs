using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class LocationMasterViewModel
    {

        public int Id { get; set; }

        public string? Action { get; set; }

        public string Loccode { get; set; } = null!;

        public string Locname { get; set; } = null!;

        public int Locareaidno { get; set; }

        public string Add1 { get; set; } = null!;

        public string? Add2 { get; set; }

        public string State { get; set; } = null!;

        public string City { get; set; } = null!;

        public string Pincode { get; set; } = null!;

        public string? Gstinno { get; set; }

        public string Email { get; set; } = null!;

        public string Mobileno { get; set; } = null!;

        public string Contpername1 { get; set; } = null!;

        public string? Contpername2 { get; set; }

        public string Contpermob1 { get; set; } = null!;

        public string? Contpermob2 { get; set; }

        public string? Contperemail1 { get; set; }

        public string? Contperemail2 { get; set; }

        public int? Compid { get; set; }

        public int? Acntidno { get; set; }

        public string Formtype { get; set; } = null!;

        public string Dealercode { get; set; } = null!;

        public int? Lineno { get; set; }

        public int Rrglocationidno { get; set; }

        public string? Active { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public string? UpdateBy { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }

    public class LocationTypewiseNameViewModel
    {
        public string locname { get; set; }
        public int locareadidNo { get; set; }
    }


}
