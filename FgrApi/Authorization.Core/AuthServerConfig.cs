using Authorization.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Authorization.Core;

public static partial class AuthServerConfig
{
    public static void AddAuthenticationServerDbConfig<TContext>(this IServiceCollection services, string connectString, int? timeOut) where TContext : DbContext
    {
        var migrationAssembly = typeof(TContext).Assembly.FullName;

        services.AddDbContext<AuthenticationDbContext>(options =>
        {
            options.UseSqlServer(connectString, opt =>
            {
                opt.MigrationsAssembly(migrationAssembly);
                opt.CommandTimeout(timeOut ?? 30);
            });
            options.UseOpenIddict<long>();
        });
    }
}
