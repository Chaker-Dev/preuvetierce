using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using PreuveTierce.Web.Data;
using PreuveTierce.Web.Services.Interfaces;

namespace PreuveTierce.Web.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "L'adresse email est requise.")]
            [EmailAddress(ErrorMessage = "Veuillez entrer une adresse email valide.")]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // On ne révèle pas que l'utilisateur n'existe pas ou n'est pas confirmé
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // Génération du code de réinitialisation
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                var emailHtml = $@"
                    <div style='font-family:Arial,sans-serif;padding:20px;color:#333;line-height:1.5;'>
                        <h2 style='color:#4F46E5;'>Réinitialisation de votre mot de passe</h2>
                        <p>Bonjour,</p>
                        <p>Une demande de réinitialisation de mot de passe a été effectuée pour votre compte <strong>PreuveTierce</strong>.</p>
                        <p>Pour choisir un nouveau mot de passe, cliquez sur le bouton ci-dessous :</p>
                        <p style='margin:30px 0;'>
                            <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' 
                               style='background:#4F46E5;color:white;padding:12px 25px;text-decoration:none;border-radius:6px;font-weight:bold;display:inline-block;'>
                               Réinitialiser mon mot de passe
                            </a>
                        </p>
                        <p style='font-size:12px;color:#666;'>Si vous n'avez pas demandé cette réinitialisation, vous pouvez ignorer cet email en toute sécurité. Votre mot de passe actuel restera inchangé.</p>
                        <hr style='border:none;border-top:1px solid #eee;margin:20px 0;' />
                        <p style='font-size:11px;color:#999;'>PreuveTierce - Sécurité & Traçabilité</p>
                    </div>";

                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "[PreuveTierce] Réinitialisation de votre mot de passe",
                    emailHtml);

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}