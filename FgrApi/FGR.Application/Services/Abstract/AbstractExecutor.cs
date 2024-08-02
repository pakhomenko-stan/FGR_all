using System.Reflection;
using CommonInterfaces.Services;
using FGR.Common.Interfaces;
using FGR.Common.RepoUtils;
using Microsoft.Extensions.DependencyInjection;

namespace FGR.Application.Services.Abstract
{
    public abstract class AbstractExecutor<Request, Reply>(IServiceProvider serviceProvider) : IExecutor<Request, Reply>
        where Request : class
        where Reply : class
    {
        protected readonly IServiceProvider serviceProvider = serviceProvider;

        private readonly RepoStore repositories = new();
        private readonly RepoStore customRepositories = new();

        protected IRepository<I> Repository<I>() where I : class => repositories.Get<IRepository<I>, I>(serviceProvider.GetRequiredService<IRepository<I>>);
        protected IRepo CustomRepository<IRepo>() where IRepo : class => customRepositories.Get<IRepo, IRepo>(serviceProvider.GetRequiredService<IRepo>);

        public async Task<Reply?> ExecuteAsync(CancellationToken token) => await ExecuteGenericAsync<object>(default, null, token);
        public async Task<Reply?> ExecuteAsync(Request? input, CancellationToken token) => await ExecuteGenericAsync<object>(default, input, token);
        public async Task<Reply?> ExecuteAsync<TPar>(TPar param, Request? input, CancellationToken token) => await ExecuteGenericAsync(param, input, token);
        public async Task<Reply?> ExecuteAsync<TPar>(TPar param, CancellationToken token) => await ExecuteGenericAsync(param, null, token);


        protected abstract Task<Reply?> ExecuteGenericAsync<TPar>(TPar? param, Request? input, CancellationToken token);

    }
}
