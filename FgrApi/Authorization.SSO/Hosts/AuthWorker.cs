namespace Authorization.SSO.Hosts
{
    internal class AuthWorker : IHostedService
    {
        private readonly IServiceProvider serviceProvider;

        public AuthWorker(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

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

            await ClientHelper.AddWebClient(manager, inUris, outUris, cancellationToken);
            await ClientHelper.AddPostmanClient(manager, cancellationToken);
            await ClientHelper.AddResourceServerClient(manager, cancellationToken);
        }

        private static IEnumerable<Uri> BuildRedirectUriPool(IEnumerable<string> uriBase, IEnumerable<string> allowedRedirectUris) =>
            allowedRedirectUris.SelectMany(a => uriBase.Select(uri => new Uri($"{uri}/{a}")));
    }
}