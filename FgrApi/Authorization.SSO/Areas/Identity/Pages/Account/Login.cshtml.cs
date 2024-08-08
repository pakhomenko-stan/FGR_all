using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Authorization.Core.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Authorization.SSO.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel(SignInManager<User> signInManager,
        ILogger<LoginModel> logger,
        UserManager<User> userManager) : PageModel
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly SignInManager<User> _signInManager = signInManager;
        private readonly ILogger<LoginModel> _logger = logger;
        private readonly string replaceableProviderName = "OpenIdConnect";
        private readonly string replacingName = "OKTA";

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }
        public List<(string Name, string DisplayName)> Providers { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Username is required.")]
            [Display(Name = "Username:")]
            [DataType(DataType.Text)]
            public string Login { get; set; }

            [Required(ErrorMessage = "Password is required.")]
            [Display(Name = "Password:")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            Providers = ExternalLogins.Select(x =>
            (x.Name,
                DisplayName: x.DisplayName == replaceableProviderName
                ? replacingName
                : x.DisplayName
            )).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            Providers = ExternalLogins.Select(x =>
            (x.Name,
                DisplayName: x.DisplayName == replaceableProviderName
                ? replacingName
                : x.DisplayName
            )).ToList();

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(Input.Login, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                var user = await _userManager.FindByNameAsync(Input.Login);

                if (result.Succeeded && !user.IsInactive)
                {
                    _logger.LogInformation("User logged in.");

                    return Redirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    //this way to refresh antiforgery cookie (prevent error after success recognition and access deny of deleted user)
                    return RedirectToPage("./Login", new { ReturnUrl = returnUrl, ErrorMessage = "The username or password is incorrect." });
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
