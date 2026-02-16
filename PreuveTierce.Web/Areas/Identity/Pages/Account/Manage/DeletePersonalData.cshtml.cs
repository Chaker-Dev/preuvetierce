using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PreuveTierce.Web.Data;

namespace PreuveTierce.Web.Areas.Identity.Pages.Account.Manage
{
    public class DeletePersonalDataModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<DeletePersonalDataModel> _logger;

        public DeletePersonalDataModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<DeletePersonalDataModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Le mot de passe est obligatoire pour confirmer.")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;
        }

        public bool RequirePassword { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Impossible de charger l'utilisateur.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Impossible de charger l'utilisateur.");
            }

            RequirePassword = await _userManager.HasPasswordAsync(user);
            if (RequirePassword)
            {
                if (string.IsNullOrWhiteSpace(Input.Password))
                {
                    ModelState.AddModelError("Input.Password",
                        "Le mot de passe est obligatoire.");
                    return Page();
                }

                if (!await _userManager.CheckPasswordAsync(user, Input.Password))
                {
                    ModelState.AddModelError("Input.Password", "Le mot de passe est incorrect.");
                    return Page();
                }
            }
            _logger.LogWarning("AUDIT_LOG: Compte supprimé par l'utilisateur {Email} (ID: {UserId}) le {Date}",
                user.Email, user.Id, DateTime.UtcNow);

            var result = await _userManager.DeleteAsync(user);
            var userId = await _userManager.GetUserIdAsync(user);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Une erreur inattendue est survenue lors de la suppression de l'utilisateur.");
            }

            await _signInManager.SignOutAsync();

            _logger.LogInformation("L'utilisateur avec l'ID '{UserId}' s'est supprimé lui-même.", userId);

            return Redirect("~/");
        }
    }
}