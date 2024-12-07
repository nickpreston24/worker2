using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using CodeMechanic.Types;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace worker2.Services;

public class EmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string message)
    {
        if (email.IsEmpty())
            throw new ArgumentNullException(nameof(email));
        if (message.IsEmpty())
            throw new ArgumentNullException(nameof(message));
        if (!Regex.IsMatch(message, @"\s*<\w+"))
            throw new ArgumentException("email must contain valid HTML!");
        string mail = "michael_n_preston@outlook.com";
        string pw = Environment.GetEnvironmentVariable("OUTLOOK_PWD");

        var client = new SmtpClient("smtp-mail.outlook.com", 587)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(mail, pw),
        };

        return client.SendMailAsync(new MailMessage(from: mail, to: email, subject, message));
    }
}
