using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Authorization.Core
{
    public partial class AuthenticationDbContext(DbContextOptions options) : IdentityDbContext(options)
    {
    }
}
