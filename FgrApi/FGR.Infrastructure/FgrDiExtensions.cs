using CommonInterfaces.Options;
using FGR.Common.Implementation;
using FGR.Common.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FGR.Infrastructure
{
    public static partial class FgrDiExtensions
    {
        public static void AddFrgRepositories(this IServiceCollection services)
        {
            services.AddSingleton<IConnectStringOptions>(s => s.GetRequiredService<IOptions<DalOptions>>().Value);
            services.AddRepositories<DalOptions, FgrContext>(RepositoryImplementator.ActionPlainRepositories);
        }
    }
}
