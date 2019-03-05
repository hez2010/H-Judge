using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace hjudgeWebHost.Services
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
        private readonly IConfiguration Configuration;
        public EmailSender(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public Task SendAsync(string subject, string content, EmailType type, string[] targets)
        {
            var username = Configuration["EmailConfig:ApiId"];
            var password = Configuration["EmailConfig:ApiSec"];
            var domain = Configuration["EmailConfig:Domain"];
            var hostname = Configuration["HostName"];
            var smtpHost = Configuration["EmailConfig:Smtp:Host"];
            var smtpPort = int.Parse(Configuration["EmailConfig:Smtp:Port"]);
            var smtpEnableSsl = bool.Parse(Configuration["EmailConfig:Smtp:EnableSsl"]);

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
                Body = content.Replace("localhost:5001", hostname).Replace("localhost:5000", hostname),
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = true
            };

            foreach (var address in targets)
            {
                msg.To.Add(new MailAddress(address));
            }

            var smtp = new SmtpClient
            {
                Host = smtpHost,
                Port = smtpPort,
                EnableSsl = smtpEnableSsl,
                Credentials = new NetworkCredential(username, password)
            };

            return smtp.SendMailAsync(msg);
        }
    }
}
