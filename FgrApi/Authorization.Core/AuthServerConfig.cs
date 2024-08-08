using Authorization.Core.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Authorization.Core;

public static class AuthServerConfig
{
    public static void AddAuthenticationServerDbConfig(this IServiceCollection services, string connectString, object caller)
    {
        var migrationAssembly = caller.GetType().Assembly.GetName().Name;

        services.AddDbContext<AuthenticationDbContext>(options =>
        {
            options.UseSqlServer(connectString, opt => opt.MigrationsAssembly(migrationAssembly));
            options.UseOpenIddict<long>();
        });


        //??
        services.Configure<IdentityOptions>(options =>
        {
            options.ClaimsIdentity.UserNameClaimType = Claims.Name;
            options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
            options.ClaimsIdentity.RoleClaimType = Claims.Role;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredUniqueChars = 1;
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 1;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
        });
    }
}
