using System.Text.Json;
using System.Text.Json.Serialization;
using CommonConverters;
using FGR.Application.Services.Abstract;
using FGR.Common.Interfaces;
using FGR.Domain.Interfaces;
using FGR.Infrastructure.Models;

namespace FGR.Application.Services
{
    public class UserActionsExecutor(IRepHolder repHolder) : AbstractExecutor<UserActionsExecutor.Request, UserActionsExecutor.Reply>(repHolder)
    {
        public class Request
        {
            public long Id { get; set; }
            public string Name { get; set; } = null!;

            [JsonConverter(typeof(DateTimeShiftJsonConverter))]
            public DateTime CreatedDate { get; set; }

        }

        public class Reply
        {
            public string Name { get; set; } = null!;

            [JsonConverter(typeof(DateTimeShiftJsonConverter))]
            public DateTime CreatedDate { get; set; }

        }

        protected override async Task<Reply?> ExecuteGenericAsync<TPar>(TPar? param, Request? input, CancellationToken token) where TPar : default
        {
            await Task.CompletedTask;
            var user = input?.GetUser();
            var role = input?.GetRole();
            IUser? reply = null;

            await RepHolder.Transaction(async rep =>
            {
                if (user == null || role == null) throw new ArgumentNullException(nameof(input));
                var initName = user.Name;

                var rol = await rep.Repository<IRole>().AddEntityAsync(role, token);
                await rep.SaveAsync(token);

                user.Name += $" with role: {rol?.Name ?? string.Empty}";
                var usr = await rep.Repository<IUser>().AddEntityAsync(user, token);
                await rep.SaveAsync(token);

                if (initName.Replace(" ","").Equals("wronguser", StringComparison.CurrentCultureIgnoreCase)) throw new Exception("Wrong user!");

                reply = usr;
            });

            if (param is not null && param.GetType() != typeof(int) && param.GetType() != typeof(string)) throw new Exception("Wrong type of parameter ))");

            var result = input is null
                    ? new Reply { Name = $"it is get request {param?.ToString() ?? string.Empty}" }
                    : reply?.GetReply();

            return result;
        }

    }

    static class UserExtensions
    {
        public static IUser GetUser(this UserActionsExecutor.Request request) => new User { Name = request.Name, CreatedDate = request.CreatedDate };
        public static IRole GetRole(this UserActionsExecutor.Request request) => new Role { Name = $"{request.Name}_ROLE" };

        public static UserActionsExecutor.Reply GetReply(this IUser user) => new() { CreatedDate = user.CreatedDate, Name = user.Name };
    }
}
