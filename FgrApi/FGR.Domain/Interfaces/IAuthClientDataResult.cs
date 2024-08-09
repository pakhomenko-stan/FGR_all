using FGR.Common.Attributes;

namespace FGR.Domain.Interfaces
{
    [RepositoryInterface]
    public interface IAuthClientDataResult
    {
        string ClientId { get; set; }
        string ClientSecret { get; set; }
        Guid CompanyId { get; set; }
        Guid? CompanyProjectId { get; set; }
        Guid? TransactionKeyId { get; set; }
        string TransactionPublicKey { get; set; }
    }
}