using FGR.Common.Implementation;
using FGR.Common.Options;
using Microsoft.Extensions.DependencyInjection;

namespace FGR.Infrastructure
{
    public static class FgrDiExtensions
    {
        public static void AddFrgRepositories(this IServiceCollection services)
        {
            services.AddRepositories<DalOptions, FgrContext>(RepositoryImplementator.ActionPlainRepositories);
        }
    }
}
