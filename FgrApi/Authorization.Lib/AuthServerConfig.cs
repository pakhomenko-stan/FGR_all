using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Authorization.Lib;

public static partial class AuthServerConfig
{
    public static void AddAuthenticationServerInMemoryConfig<TContext>(this IServiceCollection services) where TContext : DbContext
    {
        var migrationAssembly = typeof(TContext).Assembly.FullName;

        services.AddDbContext<TContext>(options =>
        {
            options.UseInMemoryDatabase(nameof(TContext));
            options.UseOpenIddict<long>();
        });
    }
}
