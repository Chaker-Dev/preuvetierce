using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using PreuveTierce.Web.Data;
using System.Text;

namespace PreuveTierce.Web.Areas.Identity.Pages.Account
{
    public class ConfirmEmailChangeModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public ConfirmEmailChangeModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public string StatusMessage { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId, string email, string code)
        {
            if (userId == null || email == null || code == null)
            {
                StatusMessage = "Lien invalide.";
                IsSuccess = false;
                return Page();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                StatusMessage = "Utilisateur introuvable.";
                IsSuccess = false;
                return Page();
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

            var result = await _userManager.ChangeEmailAsync(user, email, code);

            if (!result.Succeeded)
            {
                StatusMessage = "Le lien est invalide ou expiré.";
                IsSuccess = false;
                return Page();
            }

            await _userManager.SetUserNameAsync(user, email);

            IsSuccess = true;
            StatusMessage = "Votre adresse email a été mise à jour avec succès.";

            return Page();
        }
    }
}
