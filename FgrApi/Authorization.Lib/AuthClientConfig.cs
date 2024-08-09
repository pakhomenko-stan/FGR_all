using Authorization.Lib.Helpers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Validation.AspNetCore;

namespace Authorization.Lib
{
    public static class AuthClientConfig
    {
        public static void AddFgrAuthClientConfiguration(this IServiceCollection services)
        {
            services.AddAuthentication(options =>
                {
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(50);
                    options.SlidingExpiration = true;
                })
                .AddPolicyScheme(FgrTermsHelper.AdminUIPolicy, FgrTermsHelper.AdminUIPolicy, options =>
                {
                    options.ForwardDefaultSelector = context =>
                    {
                        return OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
                    };
                });
        }
    }
}
