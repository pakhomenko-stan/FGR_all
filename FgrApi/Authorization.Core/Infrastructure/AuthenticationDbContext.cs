using Authorization.Core.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Authorization.Core.Infrastructure
{
    public partial class AuthenticationDbContext(DbContextOptions options) : IdentityDbContext<User>(options)
    {
    }
}
