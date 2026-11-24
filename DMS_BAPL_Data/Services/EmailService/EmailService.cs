using Azure.Core;
using Azure.Identity;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Http;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Threading;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace DMS_BAPL_Data.Services.EmailService
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;

        public EmailService(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            var email = CreateMessage(toEmail, subject);
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = htmlMessage
            };

            await SendAsync(email, CancellationToken.None);
        }

        public async Task SendEmailWithPdfAttachmentAsync(
            string toEmail,
            string subject,
            string htmlMessage,
            byte[] attachmentBytes,
            string attachmentName,
            CancellationToken cancellationToken)
        {
            var tenantId = GetRequiredGraphSetting("TenantId");
            var clientId = GetRequiredGraphSetting("ClientId");
            var clientSecret = GetRequiredGraphSetting("ClientSecret");
            var senderAddress = GetRequiredGraphSetting("SenderAddress");

            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            var token = await credential.GetTokenAsync(
                new TokenRequestContext(new[] { "https://graph.microsoft.com/.default" }),
                cancellationToken);

            var graphMessage = new
            {
                message = new
                {
                    subject,
                    body = new { contentType = "HTML", content = htmlMessage },
                    toRecipients = new[]
                    {
                        new { emailAddress = new { address = toEmail } }
                    },
                    attachments = new object[]
                    {
                        new Dictionary<string, object>
                        {
                            ["@odata.type"] = "#microsoft.graph.fileAttachment",
                            ["name"] = attachmentName,
                            ["contentType"] = "application/pdf",
                            ["contentBytes"] = Convert.ToBase64String(attachmentBytes)
                        }
                    }
                },
                saveToSentItems = true
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://graph.microsoft.com/v1.0/users/{Uri.EscapeDataString(senderAddress)}/sendMail");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
            request.Content = JsonContent.Create(graphMessage);

            using var response = await _httpClientFactory.CreateClient().SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException(
                    $"Microsoft Graph sendMail failed with status {(int)response.StatusCode}: {responseBody}",
                    null,
                    response.StatusCode);
            }
        }

        private string GetRequiredGraphSetting(string name)
        {
            var value = _config[$"Email:Graph:{name}"];
            return string.IsNullOrWhiteSpace(value)
                ? throw new InvalidOperationException($"Missing configuration setting Email:Graph:{name}.")
                : value;
        }

        private MimeMessage CreateMessage(string toEmail, string subject)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config["Email:From"]));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;
            return email;
        }

        private async Task SendAsync(MimeMessage email, CancellationToken cancellationToken)
        {
            using var smtp = new SmtpClient();
            smtp.CheckCertificateRevocation = false;
            await smtp.ConnectAsync(
                _config["Email:SmtpHost"],
                int.Parse(_config["Email:SmtpPort"]),
                MailKit.Security.SecureSocketOptions.StartTls,
                cancellationToken);
            await smtp.AuthenticateAsync(_config["Email:SmtpUser"], _config["Email:SmtpPass"], cancellationToken);
            await smtp.SendAsync(email, cancellationToken);
            await smtp.DisconnectAsync(true, cancellationToken);
        }
    }
}
