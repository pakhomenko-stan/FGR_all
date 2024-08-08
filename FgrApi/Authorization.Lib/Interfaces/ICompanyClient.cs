using Refit;

namespace Authorization.Lib.Interfaces
{
    public interface ICompanyClient
    {
        public interface ICompanyClient
        {
            [Get("api/companies")]
            Task<List<object>> GetEntries();
        }

    }
}
