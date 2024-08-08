namespace Authorization.Lib.Interfaces
{
    public interface IFgrApiConfig
    {
        string BaseUrl { get; set; }
        string ClientId { get; set; }
        string ClientSecret { get; set; }
        string ClientDisplayName { get; set; }

    }
}
