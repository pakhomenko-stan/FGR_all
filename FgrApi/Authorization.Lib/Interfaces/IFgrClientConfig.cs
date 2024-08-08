namespace Authorization.Lib.Interfaces
{
    public interface IFgrClientConfig
    {
        string BaseUrl { get; set; }
        string ClientId { get; set; }
        string ClientSecret { get; set; }
        string ClientDisplayName { get; set; }

    }
}
