using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Authorization.Lib;

public static partial class AuthServerConfig
{
    private static readonly InMemoryDatabaseRoot databaseRoot = new();
    public static void AddAuthenticationServerInMemoryConfig<TContext>(this IServiceCollection services) where TContext : DbContext
    {
        var migrationAssembly = typeof(TContext).Assembly.FullName;

        services.AddDbContext<TContext>(options =>
        {
            options.UseInMemoryDatabase(nameof(TContext), databaseRoot);
            options.UseOpenIddict<long>();
        });
    }
}
