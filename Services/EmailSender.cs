using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace blog.Services
{
    /// <summary>
    /// Simple development email sender that writes messages to the debug output.
    /// Replace with a real SMTP or provider-based implementation for production.
    /// </summary>
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            Debug.WriteLine("==== EmailSender ====");
            Debug.WriteLine($"To: {email}");
            Debug.WriteLine($"Subject: {subject}");
            Debug.WriteLine($"Body (HTML): {htmlMessage}");
            Debug.WriteLine("=====================");

            return Task.CompletedTask;
        }
    }
}


