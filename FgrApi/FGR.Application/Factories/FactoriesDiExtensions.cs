using FGR.Domain.Factories;
using Microsoft.Extensions.DependencyInjection;

namespace FGR.Application.Factories
{
    public static class FactoriesDiExtensions
    {
        public static void AddFgrFactories(this IServiceCollection services)
        {
            services.AddScoped(typeof(IWrapperFactory<>), typeof(WrapperFactory<>));
        }
    }
}
