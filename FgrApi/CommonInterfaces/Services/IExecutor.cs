using System.Security.Claims;

namespace CommonInterfaces.Services
{
    public interface IExecutor<TOut>
        where TOut : class
    {
        Task<TOut?> ExecuteAsync(ClaimsPrincipal principal, CancellationToken token);
        Task<TOut?> ExecuteAsync<TPar>(TPar param, ClaimsPrincipal principal, CancellationToken token);
    }

    public interface IExecutor<TIn, TOut>: IExecutor<TOut>
        where TOut : class
        where TIn : class
    {
        Task<TOut?> ExecuteAsync(TIn? input, ClaimsPrincipal principal, CancellationToken token);
        Task<TOut?> ExecuteAsync<TPar>(TPar param, TIn? input, ClaimsPrincipal principal, CancellationToken token);
    }
}
