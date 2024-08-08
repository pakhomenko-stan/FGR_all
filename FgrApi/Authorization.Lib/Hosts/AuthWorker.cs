using Authorization.Core.Infrastructure;
using Authorization.Lib.Handlers;
using Authorization.Lib.Interfaces;
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
            var options = serviceProvider.GetRequiredService<IFgrClientConfig>();

            await ClientHandler.AddResourceServerClient(manager, options, cancellationToken: cancellationToken);
        }
    }
}