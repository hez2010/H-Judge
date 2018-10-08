using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace hjudgeWeb
{
    class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var sender = "service";
            if (subject.Contains("账户")) sender = "account";
            if (subject.Contains("邮箱")) sender = "account";
            if (subject.Contains("密码")) sender = "account";
            if (subject.Contains("服务")) sender = "service";
            if (subject.Contains("通知")) sender = "service";
            var msg = new MailMessage
            {
                From = new MailAddress($"{sender}@hjudge.com", "H::Judge", Encoding.UTF8),
                Subject = subject,
                SubjectEncoding = Encoding.UTF8,
                Body = htmlMessage.Replace("http://localhost:5000", "https://hjudge.com")
                    .Replace("https://localhost:5001", "https://hjudge.com"),
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = true
            };
            msg.To.Add(new MailAddress(email));

            var smtp = new SmtpClient
            {
                Host = "in-v3.mailjet.com",
                Port = 587,
                EnableSsl = true,
                Credentials =
                    new NetworkCredential(Secrets.EmailKeyId, Secrets.EmailKeySec)
            };

            return smtp.SendMailAsync(msg);
        }
    }
}