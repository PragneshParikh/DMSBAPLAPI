using Azure.Core;
using Azure.Identity;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Http;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using ContentType = MimeKit.ContentType;
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

        private sealed class ActiveEmailConfig
        {
            public string Provider { get; set; } = "Smtp";
            public string? TenantId { get; set; }
            public string? ClientId { get; set; }
            public string? ClientSecret { get; set; }
            public string? SenderAddress { get; set; }
            public string? SmtpHost { get; set; }
            public int SmtpPort { get; set; } = 587;
            public string? SmtpUser { get; set; }
            public string? SmtpPass { get; set; }
        }

        private async Task<ActiveEmailConfig> GetActiveConfigAsync(CancellationToken cancellationToken)
        {
            var connStr = _config.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrWhiteSpace(connStr))
            {
                await using var conn = new SqlConnection(connStr);
                await conn.OpenAsync(cancellationToken);

                const string sql = """
                    SELECT TOP 1 Provider, TenantId, ClientId, ClientSecret, SenderAddress,
                                 SmtpHost, SmtpPort, SmtpUser, SmtpPass
                    FROM EmailConfiguration
                    WHERE IsActive = 1
                    ORDER BY Id DESC
                    """;

                await using var cmd = new SqlCommand(sql, conn);
                await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
                if (await reader.ReadAsync(cancellationToken))
                {
                    return new ActiveEmailConfig
                    {
                        Provider = reader["Provider"]?.ToString() ?? "Graph",
                        TenantId = reader["TenantId"] as string,
                        ClientId = reader["ClientId"] as string,
                        ClientSecret = reader["ClientSecret"] as string,
                        SenderAddress = reader["SenderAddress"] as string,
                        SmtpHost = reader["SmtpHost"] as string,
                        SmtpPort = reader["SmtpPort"] == DBNull.Value ? 587 : Convert.ToInt32(reader["SmtpPort"]),
                        SmtpUser = reader["SmtpUser"] as string,
                        SmtpPass = reader["SmtpPass"] as string
                    };
                }
            }

            // Fallback to appsettings.json if no active DB row exists
            return new ActiveEmailConfig
            {
                Provider = !string.IsNullOrWhiteSpace(_config["Email:Graph:TenantId"]) ? "Graph" : "Smtp",
                TenantId = _config["Email:Graph:TenantId"],
                ClientId = _config["Email:Graph:ClientId"],
                ClientSecret = _config["Email:Graph:ClientSecret"],
                SenderAddress = _config["Email:Graph:SenderAddress"] ?? _config["Email:From"],
                SmtpHost = _config["Email:SmtpHost"],
                SmtpPort = int.Parse(_config["Email:SmtpPort"] ?? "587"),
                SmtpUser = _config["Email:SmtpUser"],
                SmtpPass = _config["Email:SmtpPass"]
            };
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            var cfg = await GetActiveConfigAsync(CancellationToken.None);

            if (string.Equals(cfg.Provider, "Graph", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(cfg.TenantId))
            {
                await SendViaGraphAsync(cfg, toEmail, subject, htmlMessage, null, null, CancellationToken.None);
                return;
            }

            var email = CreateMessage(cfg.SenderAddress ?? _config["Email:From"]!, toEmail, subject);
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlMessage };
            await SendSmtpAsync(cfg, email, CancellationToken.None);
        }

        public async Task SendEmailWithPdfAttachmentAsync(
            string toEmail,
            string subject,
            string htmlMessage,
            byte[] attachmentBytes,
            string attachmentName,
            CancellationToken cancellationToken)
        {
            var cfg = await GetActiveConfigAsync(cancellationToken);

            if (string.Equals(cfg.Provider, "Graph", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(cfg.TenantId))
            {
                await SendViaGraphAsync(
                    cfg, toEmail, subject, htmlMessage, attachmentBytes, attachmentName, cancellationToken);
                return;
            }

            var email = CreateMessage(cfg.SenderAddress ?? _config["Email:From"]!, toEmail, subject);
            var builder = new BodyBuilder { HtmlBody = htmlMessage };
            builder.Attachments.Add(attachmentName, attachmentBytes, new ContentType("application", "pdf"));
            email.Body = builder.ToMessageBody();

            await SendSmtpAsync(cfg, email, cancellationToken);
        }

        private async Task SendViaGraphAsync(
            ActiveEmailConfig cfg,
            string toEmail,
            string subject,
            string htmlMessage,
            byte[]? attachmentBytes,
            string? attachmentName,
            CancellationToken cancellationToken)
        {
            var credential = new ClientSecretCredential(cfg.TenantId, cfg.ClientId, cfg.ClientSecret);
            var token = await credential.GetTokenAsync(
                new TokenRequestContext(new[] { "https://graph.microsoft.com/.default" }),
                cancellationToken);

            var attachments = (attachmentBytes != null && !string.IsNullOrWhiteSpace(attachmentName))
                ? new object[]
                {
                    new Dictionary<string, object>
                    {
                        ["@odata.type"] = "#microsoft.graph.fileAttachment",
                        ["name"] = attachmentName,
                        ["contentType"] = "application/pdf",
                        ["contentBytes"] = Convert.ToBase64String(attachmentBytes)
                    }
                }
                : Array.Empty<object>();

            var graphMessage = new
            {
                message = new
                {
                    subject,
                    body = new { contentType = "HTML", content = htmlMessage },
                    toRecipients = new[] { new { emailAddress = new { address = toEmail } } },
                    attachments
                },
                saveToSentItems = true
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://graph.microsoft.com/v1.0/users/{Uri.EscapeDataString(cfg.SenderAddress!)}/sendMail");
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

        private static MimeMessage CreateMessage(string fromEmail, string toEmail, string subject)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(fromEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;
            return email;
        }

        private static async Task SendSmtpAsync(ActiveEmailConfig cfg, MimeMessage email, CancellationToken cancellationToken)
        {
            using var smtp = new SmtpClient();
            smtp.CheckCertificateRevocation = false;
            await smtp.ConnectAsync(cfg.SmtpHost, cfg.SmtpPort, SecureSocketOptions.StartTls, cancellationToken);
            await smtp.AuthenticateAsync(cfg.SmtpUser, cfg.SmtpPass, cancellationToken);
            await smtp.SendAsync(email, cancellationToken);
            await smtp.DisconnectAsync(true, cancellationToken);
        }
    }
}