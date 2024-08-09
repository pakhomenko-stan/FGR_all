using FGR.Domain.Interfaces;

namespace FGR.Infrastructure.Models
{
    public class AuthClientDataResult : IAuthClientDataResult
    {
        public long Id { get; set; }
        public Guid CompanyId { get; set; }
        public Guid? CompanyProjectId { get; set; } = null!;

        public string ClientId { get; set; } = null!;
        public string ClientSecret { get; set; } = null!;

        public Guid? TransactionKeyId { get; set; } = null!;
        public string TransactionPublicKey { get; set; } = null!;

    }
}
