using Authorization.Lib.Handlers;
using Authorization.Lib.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace Authorization.Lib
{
    public static class FgrDiExtensions
    {
        public static void AddFgrAdminClient(this IServiceCollection services, IFgrApiConfig apiConfig)
        {
            services.AddSingleton(apiConfig);
            services.AddTransient<RequestAdminHandler>();

            services.AddHttpClient(nameof(ICompanyClient), c =>
            {
                c.BaseAddress = new Uri(apiConfig.BaseUrl);
            })
            .AddTypedClient(c => RestService.For<ICompanyClient>(c))
            .AddHttpMessageHandler<RequestAdminHandler>();
        }

    }
}
