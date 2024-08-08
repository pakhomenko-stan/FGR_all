using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authorization.SSO.Areas.Identity.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            var url = $"~/Identity/Account/Login?ReturnUrl={returnUrl?.Replace("&", "%26")}";
            return LocalRedirectPermanentPreserveMethod(url);
        }

        [AllowAnonymous]
        public IActionResult Logout(string returnUrl = null)
        {
            var url = $"~/Identity/Account/Logout?ReturnUrl={returnUrl?.Replace("&", "%26")}";
            return LocalRedirectPermanentPreserveMethod(url);
        }
    }
}
