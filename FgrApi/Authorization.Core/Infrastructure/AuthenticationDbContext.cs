using Authorization.Core.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Authorization.Core.Infrastructure
{
    public partial class AuthenticationDbContext(DbContextOptions<AuthenticationDbContext> options) : IdentityDbContext<User>(options)
    {
    }
}
