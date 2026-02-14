// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using PreuveTierce.Web.Data;

namespace PreuveTierce.Web.Areas.Identity.Pages.Account
{
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ConfirmEmailModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }
        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            if (userId == null || code == null)
                return Redirect("/identity/Account/Error/InvalidLink");

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return Redirect("/identity/Account/Error/InvalidLink");

            if (await _userManager.IsEmailConfirmedAsync(user))
                return Redirect("/identity/Account/Error/AlreadyConfirmed");


            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            StatusMessage = result.Succeeded ? "Thank you for confirming your email." : "Error confirming your email.";
            if (result.Succeeded)
            {
                return Page();
            }

            if (result.Errors.Any(e => e.Code.Contains("InvalidToken")))
            {
                return RedirectToPage("/Error/EmailExpired", new { userId });
            }

            return RedirectToPage("/Error/InvalidLink");
        }
    }
}
