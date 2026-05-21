using DocumentFormat.OpenXml.Office2013.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Utils.ViewModels
{
    public class NewsBulletinFileViewModel
    {
        public int AttachmentId { get; set; }
        public string FileName { get; set; } = null!;

        public string ContentType { get; set; } = null!;

        public byte[]? FileData { get; set; } = null!;
        public string? Status { get; set; }
    }
    public class NewsBulletinMasterViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public DateTime PublishDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public bool IsActive { get; set; }

        public List<NewsBulletinFileViewModel>? Files { get; set; }

        public string CreatedBy { get; set; } = null!;

        public DateTime CreatedDate { get; set; }

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}
