using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity.UI.Services; // For IEmailSender
using blog.Services; // For EmailSender

namespace blog.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordModel(UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; } = null!;

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;
        }

        public void OnGet()
        {
            // Renders the view (your custom form)
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Console.WriteLine("[FORGOT PW DEBUG] OnPostAsync started. Email: " + Input.Email); // Log 1: Entry point

            if (!ModelState.IsValid)
            {
                Console.WriteLine("[FORGOT PW DEBUG] Model invalid - validation error");
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            Console.WriteLine("[FORGOT PW DEBUG] User lookup complete. User found: " + (user != null ? "YES (ID: " + user.Id + ", Email: " + user.Email + ")" : "NO")); // Log 2: User check

            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                Console.WriteLine("[FORGOT PW DEBUG] Skipping send - user null or email not confirmed. Confirmed: " + (user != null ? await _userManager.IsEmailConfirmedAsync(user) : "N/A")); // Log 3: Skip reason
                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            // Generate secure reset token
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            // Build reset URL (links to ResetPassword page)
            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { area = "Identity", code },
                protocol: Request.Scheme);

            // HTML email body (fits your styled site)
            var htmlMessage = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #333;'>Reset Your My Blog Site Password</h2>
                    <p>Hi,</p>
                    <p>You (or someone you know) requested a password reset. Click <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' style='color: #007bff; text-decoration: none; font-weight: bold;'>here to reset your password</a>.</p>
                    <p>If you didn't request this, ignore this email—your account is safe.</p>
                    <p>Thanks,<br><strong>My Blog Site Team</strong></p>
                </div>";

            // Send using your EmailSender
            Console.WriteLine("[FORGOT PW DEBUG] About to send email to: " + Input.Email); // Log 4: Before send

            try
            {
                await _emailSender.SendEmailAsync(Input.Email, "Reset Your Password", htmlMessage);
                Console.WriteLine("[FORGOT PW DEBUG] Email send SUCCESS - check inbox!"); // Log 5: Success
            }
            catch (Exception ex)
            {
                Console.WriteLine("[FORGOT PW DEBUG] Email send FAILED: " + ex.Message + " | Stack: " + ex.StackTrace); // Log 6: Error details
                // Don't re-throw—let it redirect anyway for UX
            }

            // Standalone SMTP Test (temporary - remove after debugging)
            Console.WriteLine("[FORGOT PW DEBUG] Starting standalone SMTP test to hansana687@gmail.com"); // Log 7: Test start
            try
            {
                await _emailSender.SendEmailAsync("hansana687@gmail.com", "Standalone Test from Blog", "<p>This is a test email from your ASP.NET blog! If you see this, SMTP works. <strong>Delete this line after testing.</strong></p>");
                Console.WriteLine("[FORGOT PW DEBUG] Standalone SMTP test SUCCESS - check inbox for 'Standalone Test'!"); // Log 8: Test success
            }
            catch (Exception ex)
            {
                Console.WriteLine("[FORGOT PW DEBUG] Standalone SMTP test FAILED: " + ex.Message + " | Stack: " + ex.StackTrace); // Log 9: Test error
            }

            Console.WriteLine("[FORGOT PW DEBUG] OnPostAsync ending - redirecting to confirmation"); // Log 10: End

            return RedirectToPage("./ForgotPasswordConfirmation");
        }
    }
}