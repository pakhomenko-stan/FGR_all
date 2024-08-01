using CommonInterfaces.Services;

namespace FGR.Application.Services
{
    public class UserActionsExecutor: IExecutor<UserActionsExecutor.Request, UserActionsExecutor.Reply>
    {
        public class Request
        {
            public string Id { get; set; } = null!;
            public string Name { get; set; } = null!;
            public string Description { get; set; } = null!;
        }

        public class Reply
        {
            public string Name { get; set; } = null!;

        }

        public async Task<Reply?> ExecuteAsync() => await ExecuteGenericAsync<object>(default, null);
        public async Task<Reply?> ExecuteAsync(Request? input) => await ExecuteGenericAsync<object>(default, input);

        public async Task<Reply?> ExecuteAsync<TPar>(TPar param, Request? input) => await ExecuteGenericAsync(param, input);

        public async Task<Reply?> ExecuteAsync<TPar>(TPar param) => await ExecuteGenericAsync(param, null);

        
        private async Task<Reply?> ExecuteGenericAsync<TPar>(TPar? param, Request? input)
        {
            if (param is not null && param.GetType() != typeof(int) && param.GetType() != typeof(string)) throw new Exception("Wrong type of parameter ))");

            await Task.CompletedTask;
            var result = new Reply { Name = $"random name {param?.ToString() ?? string.Empty}" };
            return result;
        }

    }
}
