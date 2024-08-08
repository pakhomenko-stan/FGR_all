using Authorization.Lib.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace Authorization.SSO.Extensions
{
    public static class OpenIddictServerBuilderExtensions
    {
        public static OpenIddictServerBuilder AddOpenIddictServerUserActions(this OpenIddictServerBuilder builder)
        {
            return builder.SetAuthorizationEndpointUris("/" + FgrTermsHelper.autorizeRoute)
                        .SetUserinfoEndpointUris("/" + FgrTermsHelper.userInfoRoute)
                        .SetLogoutEndpointUris("/" + FgrTermsHelper.logoutRoute)
                        .AllowAuthorizationCodeFlow()
                        .AllowPasswordFlow()
                        .AllowRefreshTokenFlow()
                        .AllowImplicitFlow();

        }
    }
}
