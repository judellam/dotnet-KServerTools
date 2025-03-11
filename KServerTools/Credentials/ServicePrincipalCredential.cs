namespace KServerTools.Common;

using Azure.Core;
using Azure.Identity;

/// <summary>
/// Gets a service principal credential.
/// </summary>
/// <typeparam name="T">The type of config</typeparam>
/// <param name="config">The config that contains information about the credential.</param>
/// <remarks>
/// Example configuration:
/// Note: Secret Data is resolved by an ISecretResolver implementation (it can be routed to a local file or AKV)
///   "ServicePrincipalConfiguration": {
///    "TenantId": "{{guid}}",
///    "ApplicationId": "{{guid}}",
///    "SecretData": "akv://SpClientSecret"
///  },
/// </remarks>
internal class ServicePrincipalCredential<T>(T config) : TokenCredentialBase<T>(config), IServicePrincipalCredential<T> where T: IServicePrincipalConfig {
    public override async Task<TokenCredential> GetCredential(CancellationToken cancellationToken) {
        string secret = await this.Config.GetSecret(cancellationToken).ConfigureAwait(false);
        return new ClientSecretCredential(
            this.Config.TenantId, 
            this.Config.ApplicationId, 
            secret);
    }

    protected override async ValueTask<AccessToken> GetAccessTokenInternal(string[] scopes, CancellationToken cancellationToken) {
        TokenCredential tokenCredential = await this.GetCredential(cancellationToken);
        return await tokenCredential.GetTokenAsync(new TokenRequestContext(scopes), cancellationToken)
            .ConfigureAwait(false);
    }
}