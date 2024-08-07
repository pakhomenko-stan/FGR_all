using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace Authorization.SSO.Areas.Identity.Pages.Account
{
    public class AccessDeniedModel : PageModel
    {
        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostConfirmationAsync()
        {
            await Task.CompletedTask;
            return RedirectToPage("./Login");
        }

    }
}

