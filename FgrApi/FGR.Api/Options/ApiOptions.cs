using System.Security.Cryptography.X509Certificates;
using Authorization.Lib.Interfaces.Options;

namespace FGR.Api.Options
{
    public class ApiOptions : IFgrApiOptions
    {
        public bool UseDevelopmentCertificates { get; set; }
        public string EncriptionSertificateThumbprint { get; set; } = null!;
        public string SigningSertificateThumbprint { get; set; } = null!;
        public bool EnableHttps { get; set; }
        public IEnumerable<string> AllowedIPAddresses { get; set; } = null!;
        public string BaseUrl { get; set; } = null!;
        public string ClientId { get; set; } = null!;
        public string ClientSecret { get; set; } = null!;
        public string ClientDisplayName { get; set; } = null!;
    }
}
