using System.Security.Claims;
using CommonInterfaces.Services;
using FGR.Common.Interfaces;

namespace FGR.Application.Services.Abstract
{
    public abstract class AbstractExecutor<Request, Reply>(IRepHolder repHolder) : IExecutor<Request, Reply>
        where Request : class
        where Reply : class
    {
        protected IRepHolder RepHolder { get; } = repHolder;

        public async Task<Reply?> ExecuteAsync(ClaimsPrincipal principal, CancellationToken token) => await ExecuteGenericAsync<object>(default, null, principal, token);
        public async Task<Reply?> ExecuteAsync(Request? input, ClaimsPrincipal principal, CancellationToken token) => await ExecuteGenericAsync<object>(default, input, principal, token);
        public async Task<Reply?> ExecuteAsync<TPar>(TPar param, Request? input, ClaimsPrincipal principal, CancellationToken token) => await ExecuteGenericAsync(param, input, principal, token);
        public async Task<Reply?> ExecuteAsync<TPar>(TPar param, ClaimsPrincipal principal, CancellationToken token) => await ExecuteGenericAsync(param, null, principal, token);

        protected abstract Task<Reply?> ExecuteGenericAsync<TPar>(TPar? param, Request? input, ClaimsPrincipal principal, CancellationToken token);

    }
}
