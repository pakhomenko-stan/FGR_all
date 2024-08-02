using System.Reflection;
using FGR.Common.Attributes;
using FGR.Common.Implementation;
using Microsoft.Extensions.DependencyInjection;

namespace FGR.Application.Factories
{
    public static class FactoriesDiExtensions
    {
        public static void AddFgrFactories(this IServiceCollection services)
        {
            services.AddImplementable(Assembly.GetExecutingAssembly(), ImplementableType.Factory);
        }
    }
}
