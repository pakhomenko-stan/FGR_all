using FGR.Application.Factories;
using FGR.Application.Services;
using FGR.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Identity.Web;

namespace FGR.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();



            builder.Services.AddFgrApplicationServices();
            builder.Services.AddFgrFactories();
            builder.Services.AddFrgRepositories();

            builder.Services.AddControllers();
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            builder.Services.AddCors(o => o?.AddPolicy("CorsPolicy", policy =>
            {
                var endpoints = builder.Configuration.GetSection("Cors:AllowedHosts")?.GetChildren()?.Select(e => e.Value ?? string.Empty)?.ToArray() ?? [];
                policy
                    ?.AllowAnyMethod()
                    ?.AllowAnyHeader()
                    ?.AllowCredentials()
                    ?.WithOrigins(endpoints);
            }));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseForwardedHeaders();

            app.UseCors("CorsPolicy");
            app.UseDefaultFiles();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            var scopeRequiredByApi = app.Configuration["AzureAd:Scopes"] ?? "";

            app.Run();
        }
    }
}
