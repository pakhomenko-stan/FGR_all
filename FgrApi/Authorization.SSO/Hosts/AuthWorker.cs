using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Authorization.Core.Infrastructure;
using Authorization.Lib.Handlers;
using Authorization.Lib.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;

namespace Authorization.SSO.Hosts
{
    internal class AuthWorker(IServiceProvider serviceProvider) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AuthenticationDbContext>();
            await context.Database.EnsureCreatedAsync(cancellationToken);
            await RegisterClientsAsync(scope.ServiceProvider, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;


        private static async Task RegisterClientsAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var manager = serviceProvider.GetRequiredService<IOpenIddictApplicationManager>();
            var options = serviceProvider.GetRequiredService<ServerOptions>();
            var inUris = BuildRedirectUriPool(options.ApiClientUris, options.AllowedRedirectUris);
            var outUris = BuildRedirectUriPool(options.ApiClientUris, options.LogoutUris);

            await ClientHandler.AddWebClient(manager, inUris, outUris, cancellationToken);
            await ClientHandler.AddResourceServerClient(manager, options, scope: FgrTermsHelper.AdminUIScope, cancellationToken: cancellationToken);
        }

        private static IEnumerable<Uri> BuildRedirectUriPool(IEnumerable<string> uriBase, IEnumerable<string> allowedRedirectUris) =>
            allowedRedirectUris.SelectMany(a => uriBase.Select(uri => new Uri($"{uri}/{a}")));
    }
}