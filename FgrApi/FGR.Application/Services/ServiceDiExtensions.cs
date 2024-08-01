using Microsoft.Extensions.DependencyInjection;

namespace FGR.Application.Services
{
    public static class ServiceDiExtensions
    {
        public static void AddFgrApplicationServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<UserActionsExecutor>();
        }
    }
}
