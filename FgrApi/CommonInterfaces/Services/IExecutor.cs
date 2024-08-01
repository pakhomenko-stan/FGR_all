namespace CommonInterfaces.Services
{
    public interface IExecutor<TOut>
        where TOut : class
    {
        Task<TOut?> ExecuteAsync();
        Task<TOut?> ExecuteAsync<TPar>(TPar param);
    }

    public interface IExecutor<TIn, TOut>: IExecutor<TOut>
        where TOut : class
        where TIn : class
    {
        Task<TOut?> ExecuteAsync(TIn? input);
        Task<TOut?> ExecuteAsync<TPar>(TPar param, TIn? input);
    }
}
