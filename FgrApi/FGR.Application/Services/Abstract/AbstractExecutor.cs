using CommonInterfaces.Services;

namespace FGR.Application.Services.Abstract
{
    public abstract class AbstractExecutor<Request, Reply> : IExecutor<Request, Reply>
        where Request : class
        where Reply : class
    {
        public async Task<Reply?> ExecuteAsync() => await ExecuteGenericAsync<object>(default, null);
        public async Task<Reply?> ExecuteAsync(Request? input) => await ExecuteGenericAsync<object>(default, input);
        public async Task<Reply?> ExecuteAsync<TPar>(TPar param, Request? input) => await ExecuteGenericAsync(param, input);
        public async Task<Reply?> ExecuteAsync<TPar>(TPar param) => await ExecuteGenericAsync(param, null);


        protected abstract Task<Reply?> ExecuteGenericAsync<TPar>(TPar? param, Request? input);

    }
}
