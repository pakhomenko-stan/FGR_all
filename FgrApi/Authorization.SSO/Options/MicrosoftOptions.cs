using Common.Interfaces.Options;

namespace Authorization.SSO.Options
{
    public class MicrosoftOptions : IExternalProviderOprtions
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
