using System.Reflection;
using FGR.Common.Extensions;
using CommonInterfaces.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FGR.Infrastructure
{
    public static class RepositoriesDiExtensions
    {
        public static IServiceCollection AddRepositories<TOpt, TContext>(
            this IServiceCollection services,
            Action<IServiceCollection, IEnumerable<Type>, Assembly, Func<Type, Type, Type>> action)
            where TContext : DbContext
            where TOpt : class, IConnectStringOptions
        {
            services.AddDbContext<TContext>((serviceProvider, dbContextOptions) =>
            {
                var dalValue = serviceProvider.GetRequiredService<IOptions<TOpt>>().Value;
                dbContextOptions.UseSqlServer(
                    dalValue.ConnectString,
                    sqlServerOptions =>
                    {
                        sqlServerOptions.CommandTimeout(dalValue.CommandTimeout ?? 30);
                    });
            });
            return services.AddRepositories<TContext>(action);
        }

        public static IServiceCollection AddRepositories<TContext>(
            this IServiceCollection services,
            Action<IServiceCollection, IEnumerable<Type>, Assembly, Func<Type, Type, Type>> action)
            where TContext : DbContext
        {
            static Type implementation(Type implementedInterfaceToRepository, Type implementingRepositoryType)
            {
                var baseType = typeof(Repository<,,>);
                Type[] typeArgs = [implementedInterfaceToRepository, implementingRepositoryType, typeof(TContext)];
                var repositoryType = baseType.MakeGenericType(typeArgs);
                return repositoryType;
            }

            services.AddScoped(s => ActivatorUtilities.CreateInstance(s, typeof(TContext)));
            var sp = services.BuildServiceProvider();
            var context = sp.GetRequiredService<TContext>();
            var contextTypes = context.GetType().GetProperties().Select(p => p.PropertyType);
            var dbSets = contextTypes.Where(t => t.Name.Equals(typeof(DbSet<>).Name));
            var entities = dbSets.SelectMany(s => s.GenericTypeArguments);
            var byAssemblies = entities
                .GroupBy(t => t.Assembly)
                .Select(g => (assembly: g.Key, interfaces: g.SelectMany(e => e.GetInterfaces())));

            byAssemblies.ForEach(item => action(services, item.interfaces, item.assembly, implementation));

            return services;
        }

    }
}
