using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;
using System;

namespace WebBanMayTinh.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Giả lập việc gửi email (ghi log ra console hoặc bạn có thể tích hợp SMTP sau)
            Console.WriteLine($"Sending email to: {email}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Message: {htmlMessage}");

            return Task.CompletedTask;
        }
    }
}
