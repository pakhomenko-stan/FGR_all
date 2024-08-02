using FGR.Application.Services.Abstract;

namespace FGR.Application.Services
{
    public class UserActionsExecutor : AbstractExecutor<UserActionsExecutor.Request, UserActionsExecutor.Reply>
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

        protected override async Task<Reply?> ExecuteGenericAsync<TPar>(TPar? param, Request? input) where TPar : default
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
