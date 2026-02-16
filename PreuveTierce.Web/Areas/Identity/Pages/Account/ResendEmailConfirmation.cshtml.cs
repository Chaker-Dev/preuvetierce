using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using PreuveTierce.Web.Data;
using PreuveTierce.Web.Services.Interfaces; 

namespace PreuveTierce.Web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ResendEmailConfirmation : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ResendEmailConfirmation(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "L'adresse email est obligatoire.")]
            [EmailAddress(ErrorMessage = "Format d'email invalide.")]
            public string Email { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                // Pour des raisons de sécurité, on ne dit pas si l'utilisateur existe ou non
                ModelState.AddModelError(string.Empty, "Lien de vérification envoyé. Veuillez consulter votre boîte mail.");
                return Page();
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { userId = userId, code = code },
                protocol: Request.Scheme);

            // Réutilisation de ton template HTML Brevo
            var emailHtml = $@"
                <div style='font-family:Arial,sans-serif;padding:20px;color:#333;'>
                    <h2 style='color:#000091;'>Activation de votre compte</h2>
                    <p>Bonjour,</p>
                    <p>Vous avez demandé un nouveau lien de confirmation pour votre compte <strong>PreuveTierce</strong>.</p>
                    <p style='margin:30px 0;'>
                        <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' 
                           style='background:#000091;color:white;padding:12px 25px;text-decoration:none;border-radius:5px;font-weight:bold;'>
                           Confirmer mon email
                        </a>
                    </p>
                    <p style='font-size:12px;color:#777;'>Si vous n'êtes pas à l'origine de cette demande, ignorez cet email.</p>
                </div>";

            await _emailSender.SendEmailAsync(
                Input.Email,
                "[PreuveTierce] Confirmation de votre compte",
                emailHtml);

            ModelState.AddModelError(string.Empty, "Lien de vérification envoyé. Veuillez consulter votre boîte mail.");
            return Page();
        }
    }
}