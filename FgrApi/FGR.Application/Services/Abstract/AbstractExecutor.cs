using CommonInterfaces.Services;
using FGR.Common.Interfaces;

namespace FGR.Application.Services.Abstract
{
    public abstract class AbstractExecutor<Request, Reply>(IRepHolder repHolder) : IExecutor<Request, Reply>
        where Request : class
        where Reply : class
    {
        protected IRepHolder RepHolder { get; } = repHolder;

        public async Task<Reply?> ExecuteAsync(CancellationToken token) => await ExecuteGenericAsync<object>(default, null, token);
        public async Task<Reply?> ExecuteAsync(Request? input, CancellationToken token) => await ExecuteGenericAsync<object>(default, input, token);
        public async Task<Reply?> ExecuteAsync<TPar>(TPar param, Request? input, CancellationToken token) => await ExecuteGenericAsync(param, input, token);
        public async Task<Reply?> ExecuteAsync<TPar>(TPar param, CancellationToken token) => await ExecuteGenericAsync(param, null, token);

        protected abstract Task<Reply?> ExecuteGenericAsync<TPar>(TPar? param, Request? input, CancellationToken token);

    }
}
