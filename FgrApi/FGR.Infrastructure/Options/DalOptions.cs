using CommonInterfaces.Options;

namespace FGR.Common.Options
{
    public class DalOptions : IConnectStringOptions
    {
        public string ConnectString { get; set; } = null!;
        public int? CommandTimeout { get; set; } = 30;
    }
}