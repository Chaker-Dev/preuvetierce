using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PreuveTierce.Web.Data;
using PreuveTierce.Web.Services.Interfaces; 
namespace PreuveTierce.Web.Areas.Identity.Pages.Account.Manage
{
    public class EnableAuthenticatorModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<EnableAuthenticatorModel> _logger;
        private readonly UrlEncoder _urlEncoder;
        private readonly IQrCodeService _qrCodeService;

        private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

        public EnableAuthenticatorModel(
            UserManager<ApplicationUser> userManager,
            ILogger<EnableAuthenticatorModel> logger,
            UrlEncoder urlEncoder,
            IQrCodeService qrCodeService)
        {
            _userManager = userManager;
            _logger = logger;
            _urlEncoder = urlEncoder;
            _qrCodeService = qrCodeService;
        }

        public string SharedKey { get; set; }
        public string QrCodeImageBase64 { get; set; } // Propriété pour l'image

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Le code de vérification est requis.")]
            [StringLength(7, ErrorMessage = "Le code doit comporter 6 chiffres.", MinimumLength = 6)]
            [Display(Name = "Code de vérification")]
            public string Code { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("Utilisateur introuvable.");

            await LoadSharedKeyAndQrCodeUriAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound("Utilisateur introuvable.");

            if (!ModelState.IsValid)
            {
                await LoadSharedKeyAndQrCodeUriAsync(user);
                return Page();
            }

            var verificationCode = Input.Code.Replace(" ", string.Empty).Replace("-", string.Empty);
            var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
                user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (!is2faTokenValid)
            {
                ModelState.AddModelError("Input.Code", "Le code de vérification est invalide.");
                await LoadSharedKeyAndQrCodeUriAsync(user);
                return Page();
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);
            StatusMessage = "Votre application d'authentification a été activée.";

            return RedirectToPage("./TwoFactorAuthentication");
        }

        private async Task LoadSharedKeyAndQrCodeUriAsync(ApplicationUser user)
        {
            var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            SharedKey = FormatKey(unformattedKey);
            var email = await _userManager.GetEmailAsync(user);
            var uri = GenerateQrCodeUri(email, unformattedKey);

            // Utilisation de TON service pour générer le PNG
            var qrCodeBytes = _qrCodeService.GeneratePng(uri);
            QrCodeImageBase64 = Convert.ToBase64String(qrCodeBytes);
        }

        private string FormatKey(string unformattedKey) =>
            string.Join(" ", Enumerable.Range(0, unformattedKey.Length / 4)
                .Select(i => unformattedKey.Substring(i * 4, 4))).ToLowerInvariant();

        private string GenerateQrCodeUri(string email, string unformattedKey) =>
            string.Format(AuthenticatorUriFormat, _urlEncoder.Encode("PreuveTierce"), _urlEncoder.Encode(email), unformattedKey);
    }
}