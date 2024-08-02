namespace CommonInterfaces.Options
{
    public interface IConnectStringOptions
    {
        string ConnectString { get; set; }
        int? CommandTimeout { get; set; }
    }
}