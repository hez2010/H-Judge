using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace hjudgeWeb
{
    class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            throw new NotImplementedException();
        }
    }
}