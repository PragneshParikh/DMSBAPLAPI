using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace DMS_BAPL_Data.Services.EmailService
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
        Task SendEmailWithPdfAttachmentAsync(
            string toEmail,
            string subject,
            string htmlMessage,
            byte[] attachmentBytes,
            string attachmentName,
            CancellationToken cancellationToken);
    }
}
