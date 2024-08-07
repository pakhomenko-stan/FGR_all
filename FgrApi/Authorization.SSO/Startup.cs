using System.Globalization;
using System.Linq;
using Authorization.Core;
using Authorization.SSO.Filters;
using Authorization.SSO.Hosts;
using Authorization.SSO.Options;
using Authorization.SSO.Providers;
using Authorization.SSO.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Authorization.SSO
{
    public class Startup
    {
        public Startup(
            IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            IdentityModelEventSource.ShowPII = true;

            var serverOptionsConfig = Configuration.GetSection("ServerOptions");
            var serverOptions = serverOptionsConfig.Get<ServerOptions>();
            var msOptions = Configuration.GetSection("Microsoft").Get<MicrosoftOptions>();
            services.Configure<ServerOptions>(serverOptionsConfig);

            services.AddAuthenticationServerConfig(serverOptions.IdentityDbConnectString, this);
            services.AddAuthentication()
                .AddMicrosoftAccount(options =>
                {
                    options.ClientId = msOptions.ClientId;
                    options.ClientSecret = msOptions.ClientSecret;
                });


            services.AddCors(o => o.AddPolicy("CorsPolicy", builder =>
            {
                builder
                    .WithOrigins(serverOptions.ApiClientUris.ToArray())
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            }));

            services.AddOpenIddict()
                .AddCore(configuration =>
                {
                    configuration.UseEntityFrameworkCore()
                                 .UseDbContext<AuthenticationDbContext>()
                                 .ReplaceDefaultEntities<long>();
                })
                .AddServer(configuration =>
                {
                    configuration
                        .RegisterScopes(Scopes.OpenId, Scopes.Profile, Scopes.OfflineAccess, Scopes.Roles)
                        .SetTokenEndpointUris("/" + AuthParams.AuthParams.tokenRoute)
                        .SetAuthorizationEndpointUris("/" + AuthParams.AuthParams.autorizeRoute)
                        .SetUserinfoEndpointUris("/" + AuthParams.AuthParams.userInfoRoute)
                        .SetLogoutEndpointUris("/" + AuthParams.AuthParams.logoutRoute)
                        .AllowAuthorizationCodeFlow()
                        .AllowPasswordFlow()
                        .AllowRefreshTokenFlow()
                        .AllowImplicitFlow()
                        .AllowClientCredentialsFlow();

                    if (serverOptions.UseDevelopmentCertificates)
                    {
                        configuration
                            .AddDevelopmentEncryptionCertificate()
                            .AddDevelopmentSigningCertificate();
                    }
                    else
                    {
                        configuration
                            .AddEncryptionCertificate(serverOptions.EncriptionSertificateThumbprint)
                            .AddSigningCertificate(serverOptions.SigningSertificateThumbprint);
                    }

                    configuration.DisableAccessTokenEncryption(); // change of this disable leads to client access token evaluation subsystem reengeneiring

                    var aspBulder = configuration.UseAspNetCore()
                        .EnableTokenEndpointPassthrough()
                        .EnableAuthorizationEndpointPassthrough()
                        .EnableLogoutEndpointPassthrough()
                        .EnableUserinfoEndpointPassthrough();

                    if (!serverOptions.EnableHttps)
                        aspBulder.DisableTransportSecurityRequirement();  // Never use https if use load balancer
                })
                .AddValidation(configuration =>
                {
                    configuration.UseLocalServer();
                    configuration.UseAspNetCore();
                });

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });
            services.AddSingleton(s => serverOptions);
            services.AddSingleton<IEmailSender, EmailService>();
            services.AddSingleton<ListProvider>();
            services.AddScoped<RequestAddressFilter>();

            services.AddHostedService<AuthWorker>();
            services.AddControllersWithViews();
            services.AddRazorPages();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            var supportedCultures = new[]{
                new CultureInfo("en-US")
            };
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-US"),
                SupportedCultures = supportedCultures,
                FallBackToParentCultures = false
            });
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CreateSpecificCulture("en-US");


            app.UseStaticFiles();
            app.UseRouting();
            app.UseForwardedHeaders();

            app.UseCors("CorsPolicy");
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
