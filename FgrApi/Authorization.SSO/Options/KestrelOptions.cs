namespace Authorization.SSO.Options
{
    public class KestrelOptions
    {
        public int Port { get; set; }
        public string CertPath { get; set; } = string.Empty;
        public bool EnableHttps { get; set; }
    }
}