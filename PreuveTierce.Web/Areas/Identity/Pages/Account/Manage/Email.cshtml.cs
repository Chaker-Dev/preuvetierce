// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using PreuveTierce.Web.Data;
using PreuveTierce.Web.Services.Interfaces;

namespace PreuveTierce.Web.Areas.Identity.Pages.Account.Manage
{
    public class EmailModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public EmailModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public bool IsEmailConfirmed { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "New email")]
            public string NewEmail { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var email = await _userManager.GetEmailAsync(user);
            Email = email;

            Input = new InputModel
            {
                NewEmail = email,
            };

            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Impossible de charger l'utilisateur avec l'ID  '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostChangeEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Impossible de charger l'utilisateur avec l'ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }
            var email = await _userManager.GetEmailAsync(user);

            if (Input.NewEmail != email)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateChangeEmailTokenAsync(user, Input.NewEmail);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmailChange",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, email = Input.NewEmail, code = code },
                    protocol: Request.Scheme);
                var subject = "Confirmation de changement d’adresse email – PreuveTierce";

                var htmlMessage = $@"
                    <div style='font-family:Arial,sans-serif;line-height:1.6'>
                        <h2>Demande de modification de votre adresse email</h2>

                        <p>Bonjour,</p>

                        <p>
                            Une demande de changement d’adresse email a été effectuée
                            pour votre compte <strong>PreuveTierce</strong>.
                        </p>

                        <p>
                            Nouvelle adresse demandée :
                            <strong>{HtmlEncoder.Default.Encode(Input.NewEmail)}</strong>
                        </p>

                        <p>
                            Pour confirmer cette modification, cliquez sur le bouton ci-dessous :
                        </p>

                        <p style='margin:30px 0'>
                            <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'
                               style='background:#2563eb;
                                      color:white;
                                      padding:12px 20px;
                                      text-decoration:none;
                                      border-radius:6px;
                                      font-weight:bold'>
                                Confirmer le changement d’email
                            </a>
                        </p>

                        <p style='font-size:13px;color:#b91c1c'>
                            ⚠️ Si vous n’êtes pas à l’origine de cette demande,
                            ignorez cet email et changez immédiatement votre mot de passe.
                        </p>

                        <hr/>

                        <p style='font-size:12px;color:#999'>
                            Horodatage : {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC<br/>
                            IP estimée : {HttpContext.Connection.RemoteIpAddress}
                        </p>
                    </div>";

                await _emailSender.SendEmailAsync(Input.NewEmail, subject, htmlMessage);
                StatusMessage = "Un lien de confirmation a été envoyé à la nouvelle adresse email. \" +\r\n        \"La modification sera effective après validation.";
                return RedirectToPage();
            }

            StatusMessage = "La nouvelle adresse email est identique à l’actuelle.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Impossible de charger l'utilisateur avec l'ID  '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, code = code },
                protocol: Request.Scheme);
            var subject = "Confirmez votre adresse email – PreuveTierce";
            var htmlMessage = $@"
                    <div style='font-family:Arial,sans-serif;line-height:1.6'>
                        <h2>Confirmation de votre adresse email</h2>

                        <p>Bonjour,</p>

                        <p>
                            Merci d’avoir créé un compte sur <strong>PreuveTierce</strong>.
                        </p>

                        <p>
                            Pour activer votre espace sécurisé et finaliser votre inscription,
                            veuillez confirmer votre adresse email en cliquant sur le bouton ci-dessous :
                        </p>

                        <p style='margin:30px 0'>
                            <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'
                               style='background:#2563eb;
                                      color:white;
                                      padding:12px 20px;
                                      text-decoration:none;
                                      border-radius:6px;
                                      font-weight:bold'>
                                Confirmer mon adresse email
                            </a>
                        </p>

                        <p style='font-size:13px;color:#666'>
                            Si vous n’êtes pas à l’origine de cette inscription,
                            vous pouvez ignorer cet email.
                        </p>

                        <hr/>

                        <p style='font-size:12px;color:#999'>
                            Cet email a été envoyé automatiquement par PreuveTierce.<br/>
                            Horodatage : {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC
                        </p>
                    </div>";

            await _emailSender.SendEmailAsync(email, subject, htmlMessage);

            StatusMessage = "Email de vérification envoyé. Veuillez consulter votre boîte de réception.";

            return RedirectToPage();
        }
    }
}
