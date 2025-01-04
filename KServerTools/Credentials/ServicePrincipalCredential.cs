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
internal class ServicePrincipalCredential<T>(T config) : IServicePrincipalCredential<T> where T: ICredentialConfig {
    private readonly T config = config;
    private string? accessToken;
    private DateTimeOffset? accessTokenExpiresOn;

    public async Task<string> GetAccessToken(string[] scopes, CancellationToken cancellationToken) {
        if (this.accessToken is null || this.accessTokenExpiresOn is null || this.accessTokenExpiresOn < DateTimeOffset.UtcNow) {
            TokenCredential credential = await this.GetCredential(cancellationToken);
            AccessToken token = await credential.GetTokenAsync(new TokenRequestContext(scopes), cancellationToken)
                .ConfigureAwait(false);
            this.accessToken = token.Token;
            this.accessTokenExpiresOn = token.ExpiresOn.AddMinutes(-10);
        }

        return this.accessToken;
    }

    public async Task<TokenCredential> GetCredential(CancellationToken cancellationToken) {
        string secret = await config.GetResolvedSecret(cancellationToken).ConfigureAwait(false);
        return new ClientSecretCredential(
            this.config.TenantId, 
            this.config.ApplicationId, 
            secret);
    }
}