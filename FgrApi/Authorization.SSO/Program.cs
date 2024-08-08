using System.Reflection;
using Authorization.SSO.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Authorization.SSO
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, configuration) =>
                {
                    configuration.Sources.Clear();
                    var env = hostingContext.HostingEnvironment;
                    var assemblyConfigurationAttribute = typeof(Program).Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>();
                    var buildConfigurationName = assemblyConfigurationAttribute?.Configuration;
                    configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);

                    // IIS can only read Environment from web.config
                    configuration.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: false);

                    // config for local develpment
                    configuration.AddJsonFile($"appsettings.{buildConfigurationName}.json", optional: true, reloadOnChange: false);

                    //add values that docker composer provides
                    configuration.AddEnvironmentVariables();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureKestrel((context, options) =>
                    {
                        var kestrelOptions = context.Configuration.GetSection("Kestrel").Get<KestrelOptions>();
                        options.ListenAnyIP(kestrelOptions?.Port ?? 80, listenOptions =>
                        {
                            if (kestrelOptions?.EnableHttps ?? false)
                            {
                                if (!string.IsNullOrEmpty(kestrelOptions.CertPath)) listenOptions.UseHttps(kestrelOptions.CertPath);
                                else listenOptions.UseHttps();
                            }
                        });
                    });
                    webBuilder.UseKestrel();
                });
    }
}
