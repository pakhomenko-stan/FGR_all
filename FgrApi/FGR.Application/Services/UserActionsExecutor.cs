using CommonInterfaces.Services;

namespace FGR.Application.Services
{
    public class UserActionsExecutor: IExecutor<UserActionsExecutor.Request, UserActionsExecutor.Reply>
    {
        public class Request
        {
            public long Id { get; set; }
            public string Name { get; set; } = null!;
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
            await Task.CompletedTask;

            if (param is not null && param.GetType() != typeof(int) && param.GetType() != typeof(string)) throw new Exception("Wrong type of parameter ))");

            var result = input is null
                    ? new Reply { Name = $"it is get request {param?.ToString() ?? string.Empty}" }
                    : new Reply { Name = $"it is POST request {input?.Name ?? string.Empty}" };

            return result;


        }

    }
}
