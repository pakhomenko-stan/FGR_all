using FGR.Application.Services.Abstract;
using FGR.Domain.Interfaces;
using FGR.Infrastructure.Models;

namespace FGR.Application.Services
{
    public class UserActionsExecutor(IServiceProvider serviceProvider) : AbstractExecutor<UserActionsExecutor.Request, UserActionsExecutor.Reply>(serviceProvider)
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

        protected override async Task<Reply?> ExecuteGenericAsync<TPar>(TPar? param, Request? input, CancellationToken token) where TPar : default
        {
            await Task.CompletedTask;
            var user = input?.GetUser();

            await Repository<IUser>().Transaction(async (rep, action) =>
            {
                if (user == null) throw new ArgumentNullException(nameof(input));
                var usr = await rep.AddEntityAsync(user, token);
                var sp1 = await rep.SaveAsync(token);
                action?.Invoke(sp1);
                if ((usr?.Id ?? 0) < 1000) throw new Exception("Wrong Id of user");
            });

            if (param is not null && param.GetType() != typeof(int) && param.GetType() != typeof(string)) throw new Exception("Wrong type of parameter ))");

            var result = input is null
                    ? new Reply { Name = $"it is get request {param?.ToString() ?? string.Empty}" }
                    : new Reply { Name = $"it is POST request {input?.Name ?? string.Empty}" };

            return result;
        }

    }

    static class UserExtensions
    {
        public static IUser GetUser(this UserActionsExecutor.Request request) => new User { Id = request.Id, Name = request.Name }; 
    }
}
