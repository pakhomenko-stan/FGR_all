using Authorization.Lib.Helpers;
using Authorization.Lib.Interfaces;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Authorization.Lib.Handlers;
public static class ClientHandler
{
    public static async Task AddWebClient(
        IOpenIddictApplicationManager manager,
        IEnumerable<Uri> inUris,
        IEnumerable<Uri> outUris,
        CancellationToken cancellationToken)
    {
        var descriptor = new OpenIddictApplicationDescriptor()
        {
            ClientId = FgrTermsHelper.WebClientId,
            ConsentType = ConsentTypes.Implicit,
            DisplayName = FgrTermsHelper.WebClientDisplayName,
            PostLogoutRedirectUris = { },
            RedirectUris = { },
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Logout,
                Permissions.Endpoints.Token,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.GrantTypes.RefreshToken,
                Permissions.GrantTypes.Password,
                Permissions.ResponseTypes.Code,
                Permissions.Scopes.Profile,
                Permissions.Scopes.Roles,
                Permissions.Scopes.Email,
                Permissions.Prefixes.Scope + Scopes.OfflineAccess,
            },
            Requirements =
            {
                Requirements.Features.ProofKeyForCodeExchange
            }
        };

        inUris.ToList().ForEach(uri => descriptor.RedirectUris.Add(uri));
        outUris.ToList().ForEach(uri => descriptor.PostLogoutRedirectUris.Add(uri));
        await RenewClientApp(FgrTermsHelper.WebClientId, manager, descriptor, cancellationToken);
    }

    public static async Task AddResourceServerClient(IOpenIddictApplicationManager manager, IFgrClientConfig config, string scope = FgrTermsHelper.AdminUIScope, CancellationToken cancellationToken = default)
    {
        await RenewClientApp(config.ClientId, manager, new OpenIddictApplicationDescriptor
        {
            ClientId = config.ClientId,
            ClientSecret = config.ClientId,
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
