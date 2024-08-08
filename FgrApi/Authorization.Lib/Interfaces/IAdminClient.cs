using Refit;

namespace Authorization.Lib.Interfaces
{
    public interface IAdminClient
    {
        [Get("api/users/list")]
        Task<List<object>> GetUsers();
    }
}
