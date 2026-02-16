using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using PreuveTierce.Web.Data;
using PreuveTierce.Web.Services.Interfaces;
using System.Text;

namespace PreuveTierce.Web.Areas.Identity.Pages.Account.Error
{
    public class EmailExpiredModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public EmailExpiredModel(
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty(SupportsGet = true)]
        public string UserId { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null)
                return RedirectToPage("/");

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.PageLink(
                "/Account/ConfirmEmail",
                values: new { area = "Identity", userId = user.Id, code },
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(
                user.Email,
                "[PreuveTierce] Nouveau lien de confirmation",
                $@"
            <h2>Confirmez votre compte</h2>
            <p>Votre précédent lien a expiré.</p>
            <a href='{callbackUrl}'>Confirmer mon compte</a>
            ");

            return RedirectToPage("/Account/RegisterConfirmation",
                new { email = user.Email });
        }
    }
}