using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace hjudge.WebHost.Services
{
    public enum EmailType
    {
        Account,
        Service,
        Notification,
        General
    }
    public interface IEmailSender
    {
        Task SendAsync(string subject, string content, EmailType type, string[] targets);
    }
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration configuration;
        public EmailSender(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public async Task SendAsync(string subject, string content, EmailType type, string[] targets)
        {
            var username = configuration["EmailConfig:UserName"];
            var password = configuration["EmailConfig:Password"];
            var domain = configuration["EmailConfig:Domain"];
            var hostname = configuration["HostName"];
            var smtpHost = configuration["EmailConfig:Smtp:Host"];
            var smtpPort = int.Parse(configuration["EmailConfig:Smtp:Port"]);
            var smtpEnableSsl = bool.Parse(configuration["EmailConfig:Smtp:EnableSsl"]);

            var sender = type switch
            {
                EmailType.Account => "account",
                EmailType.Notification => "notification",
                EmailType.Service => "service",
                _ => "general"
            };

            var msg = new MailMessage
            {
                From = new MailAddress($"{sender}@{domain}"),
                Subject = subject,
                SubjectEncoding = Encoding.UTF8,
                Body = content.Replace("localhost:5001", hostname).Replace("localhost:5000", hostname), // replace host name for reverse proxy
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = true
            };

            foreach (var address in targets)
            {
                msg.To.Add(new MailAddress(address));
            }

            using var smtp = new SmtpClient
            {
                Host = smtpHost,
                Port = smtpPort,
                EnableSsl = smtpEnableSsl,
                Credentials = new NetworkCredential(username, password)
            };

            await smtp.SendMailAsync(msg);
        }
    }
}
