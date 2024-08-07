using System.Collections.Generic;

namespace Authorization.SSO
{
    public class ServerOptions
    {

        public IEnumerable<string> ApiClientUris { get; set; }
        public IEnumerable<string> AllowedRedirectUris { get; set; }
        public IEnumerable<string> AllowedIPAddresses { get; set; }
        public IEnumerable<string> LogoutUris { get; set; }
        public string IdentityDbConnectString { get; set; }
        public string AppDomain { get; set; }
        public string EMailMessage { get; set; }
        public string EmailFrom { get; set; }
        public string SmtpServer { get; set; }
        public bool UseSmtp { get; set; }
        public string EmailPassword { get; set; }
        public int SmtpPort { get; set; }
        public string EncriptionSertificateThumbprint { get; set; }
        public string SigningSertificateThumbprint { get; set; }
        public bool EnableHttps { get; set; }
        public bool UseDevelopmentCertificates { get; set; }
        public string Homepage { get; set; }
    }
}