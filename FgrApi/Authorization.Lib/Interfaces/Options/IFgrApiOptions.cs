using System.Security.Cryptography.X509Certificates;

namespace Authorization.Lib.Interfaces.Options
{
    public interface IFgrApiOptions : IFgrClientConfig
    {
        bool UseDevelopmentCertificates { get; set; }
        string EncriptionSertificateThumbprint { get; set; }
        string SigningSertificateThumbprint { get; set; }
        bool EnableHttps { get; set; }
        public IEnumerable<string> AllowedIPAddresses { get; set; }

    }
}
