using System.Net.Mail;
using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;



namespace CarPoolHTLVB.EmailService
{

    public class EmailService
{
    private readonly SmtpClient _smtpClient;

    public EmailService()
    {
        // Configure your SMTP settings (replace with your own values)
        _smtpClient = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential("carpoolhtlvb@gmail.com", "hxle elzt wyiv jopq"),
            EnableSsl = true,
        };
    }

    public async Task SendConfirmationEmailAsync(string to, string token)
    {
        try
        {
                var subject = "Confirm your email address";
                var body = token;

            var mailMessage = new MailMessage
            {
                From = new MailAddress("carpoolhtlvb@gmail.com"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(to);

            await _smtpClient.SendMailAsync(mailMessage);
        }
        catch (Exception ex)
        {
            // Handle exception (log, throw, etc.)
            Console.Clear();
            Console.WriteLine($"Error sending confirmation email: {ex.Message}");
        }
    }
}
    
}
