using FGR.Common.Interfaces;
using FGR.Common.RepoUtils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FGR.Infrastructure.Services
{
    public class RepHolder<TContext>(IServiceProvider serviceProvider, TContext context) : IRepHolder where TContext : DbContext
    {
        private readonly RepoStore repositories = new();
        private readonly RepoStore customRepositories = new();

        public IRepository<I> Repository<I>() where I : class => repositories.Get<IRepository<I>, I>(serviceProvider.GetRequiredService<IRepository<I>>);
        public IRepo CustomRepository<IRepo>() where IRepo : class => customRepositories.Get<IRepo, IRepo>(serviceProvider.GetRequiredService<IRepo>);

        public async Task Transaction(Func<IRepHolder, Task> func)
        {
            using var transaction = context.Database.BeginTransaction();
            try
            {
                await func(this);
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                transaction.Dispose();
            }
        }

        public async Task SaveAsync(CancellationToken cancellationToken)
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
