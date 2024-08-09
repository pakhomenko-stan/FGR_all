using Authorization.Lib.Helpers;
using Authorization.Lib.Interfaces;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Authorization.Lib.Handlers;
public static class ClientHandler
{
    public static async Task AddResourceServerClient(IOpenIddictApplicationManager manager, IFgrClientConfig config, string scope = FgrTermsHelper.AdminUIScope, CancellationToken cancellationToken = default)
    {
        await RenewClientApp(config.ClientId, manager, new OpenIddictApplicationDescriptor
        {
            ClientId = config.ClientId,
            ClientSecret = config.ClientSecret,
            DisplayName = config.ClientDisplayName,
            Permissions =
                {
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.ClientCredentials,
                    Permissions.Prefixes.Scope + scope,
                }
        }, cancellationToken);
    }

    private static async Task RenewClientApp(
        string appId,
        IOpenIddictApplicationManager manager,
        OpenIddictApplicationDescriptor applicationDescriptor,
        CancellationToken cancellationToken)
    {
        var client = await manager.FindByClientIdAsync(appId, cancellationToken);
        if (client != null)
            await manager.DeleteAsync(client, cancellationToken);
        await manager.CreateAsync(applicationDescriptor, cancellationToken);
    }

}
