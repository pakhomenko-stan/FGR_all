using System.Security.Cryptography.X509Certificates;

namespace Authorization.Lib.Interfaces.Options
{
    public interface IFgrApiOptions : IFgrClientConfig
    {
        bool UseDevelopmentCertificates { get; set; }
        X509Certificate2 EncriptionSertificateThumbprint { get; set; }
        X509Certificate2 SigningSertificateThumbprint { get; set; }
        bool EnableHttps { get; set; }
        public IEnumerable<string> AllowedIPAddresses { get; set; }

    }
}
