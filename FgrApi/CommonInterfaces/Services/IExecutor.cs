namespace CommonInterfaces.Services
{
    public interface IExecutor<TOut>
        where TOut : class
    {
        Task<TOut?> ExecuteAsync(CancellationToken token);
        Task<TOut?> ExecuteAsync<TPar>(TPar param, CancellationToken token);
    }

    public interface IExecutor<TIn, TOut>: IExecutor<TOut>
        where TOut : class
        where TIn : class
    {
        Task<TOut?> ExecuteAsync(TIn? input, CancellationToken token);
        Task<TOut?> ExecuteAsync<TPar>(TPar param, TIn? input, CancellationToken token);
    }
}
