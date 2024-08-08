using Authorization.Lib.Handlers;
using Authorization.Lib.Helpers;
using Authorization.Lib.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using Authorization.Core;
using Authorization.Lib.Interfaces.Options;
using CommonInterfaces.Options;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Authorization.Lib
{
    public static class FgrDiExtensions
    {
        public static void AddFgrAdminClient(this IServiceCollection services, IFgrClientConfig apiConfig)
        {
            services.AddSingleton(apiConfig);
            services.AddTransient<RequestAdminHandler>();

            services.AddHttpClient(nameof(IAdminClient), c =>
            {
                c.BaseAddress = new Uri(apiConfig.BaseUrl);
            })
            .AddTypedClient(c => RestService.For<IAdminClient>(c))
            .AddHttpMessageHandler<RequestAdminHandler>();
        }

        public static void AddFgrApiServerConfig<TContext>(this IServiceCollection services, IConnectStringOptions authConnect, IFgrApiOptions apiOptions) where TContext : DbContext
        {
            services.AddAuthenticationServerDbConfig<TContext>(authConnect.ConnectString, authConnect.CommandTimeout);

            services.AddAuthorization(options =>
            {
                options.AddPolicy(FgrTermsHelper.AdminUIPolicy, policy =>
                {
                    policy.AddAuthenticationSchemes(FgrTermsHelper.AdminUIAuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", FgrTermsHelper.AdminUIScope);
                });
                options.AddPolicy(FgrTermsHelper.PaymentPolicy, policy =>
                {
                    policy.AddAuthenticationSchemes(FgrTermsHelper.PaymentAuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", FgrTermsHelper.PaymentScope);
                    policy.RequireClaim(FgrTermsHelper.GetClientClaim(FgrTermsHelper.CompanyIdClaimType));
                    policy.RequireClaim(FgrTermsHelper.GetClientClaim(FgrTermsHelper.ProjectIdClaimType));
                    policy.RequireClaim(FgrTermsHelper.GetClientClaim(FgrTermsHelper.TypeClaimType));

                    policy.RequireClaim(FgrTermsHelper.GetClientClaim(FgrTermsHelper.TransactionKeyIdClaimType));
                    policy.RequireClaim(FgrTermsHelper.GetClientClaim(FgrTermsHelper.TransactionPublicKeyClaimType));
                });
            });

            services.AddOpenIddict()
                .AddCore(configuration =>
                {
                    configuration.UseEntityFrameworkCore()
                                 .UseDbContext<TContext>()
                                 .ReplaceDefaultEntities<long>();
                })
                .AddServer(configuration =>
                {
                    configuration
                        .RegisterScopes(Scopes.Profile, FgrTermsHelper.AdminUIScope, FgrTermsHelper.PaymentScope)
                        .SetTokenEndpointUris("/" + FgrTermsHelper.tokenRoute)
                        .AllowClientCredentialsFlow();

                    if (apiOptions.UseDevelopmentCertificates)
                    {
                        configuration
                            .AddDevelopmentEncryptionCertificate()
                            .AddDevelopmentSigningCertificate();
                    }
                    else
                    {
                        configuration
                            .AddEncryptionCertificate(apiOptions.EncriptionSertificateThumbprint)
                            .AddSigningCertificate(apiOptions.SigningSertificateThumbprint);
                    }

                    configuration.DisableAccessTokenEncryption(); // change of this disable leads to client access token evaluation subsystem reengeneiring

                    var aspBulder = configuration.UseAspNetCore()
                        .EnableTokenEndpointPassthrough();

                    if (!apiOptions.EnableHttps)
                        aspBulder.DisableTransportSecurityRequirement();  // Never use https if use load balancer
                })
                .AddValidation(configuration =>
                {
                    configuration.UseLocalServer();
                });
        }

    }
}
