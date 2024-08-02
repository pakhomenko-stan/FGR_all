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

        public async Task<Reply?> ExecuteAsync() => await ExecuteGenericAsync<object>(default, null);
        public async Task<Reply?> ExecuteAsync(Request? input) => await ExecuteGenericAsync<object>(default, input);
        public async Task<Reply?> ExecuteAsync<TPar>(TPar param, Request? input) => await ExecuteGenericAsync(param, input);
        public async Task<Reply?> ExecuteAsync<TPar>(TPar param) => await ExecuteGenericAsync(param, null);


        protected abstract Task<Reply?> ExecuteGenericAsync<TPar>(TPar? param, Request? input);

    }
}
