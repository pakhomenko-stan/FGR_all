using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(Authorization.SSO.Areas.Identity.IdentityHostingStartup))]
namespace Authorization.SSO.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
            });
        }
    }
}