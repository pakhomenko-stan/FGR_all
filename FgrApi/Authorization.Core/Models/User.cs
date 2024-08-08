using Microsoft.AspNetCore.Identity;

namespace Authorization.Core.Models
{
    public class User : IdentityUser
    {
        public bool IsInactive { get; set; }
    }
}
