namespace KServerTools.Common;

using Azure.Core;
using Azure.Identity;

/// <summary>
/// Gets a service principal credential.
/// </summary>
/// <typeparam name="T">The type of config.</typeparam>
/// <param name="config">The config that contains information about the credential.</param>
/// <remarks>
/// Example configuration:
/// Note: Secret Data is resolved by an ISecretResolver implementation (it can be routed to a local file or AKV)
///   "ServicePrincipalConfiguration": {
///    "TenantId": "{{guid}}",
///    "ApplicationId": "{{guid}}",
///    "SecretData": "akv://SpClientSecret"
///  }.
/// </remarks>
internal class ServicePrincipalCredential<T>(T config) : TokenCredentialBase<T>(config), IServicePrincipalCredential<T> where T : IServicePrincipalConfig {
    /// <summary>
    /// Gets the client secret credential for this service principal.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="TokenCredential"/> configured with the service principal's client secret.</returns>
    public override async Task<TokenCredential> GetCredential(CancellationToken cancellationToken) {
        string secret = await this.Config.GetSecret(cancellationToken).ConfigureAwait(false);
        return new ClientSecretCredential(
            this.Config.TenantId,
            this.Config.ApplicationId,
            secret);
    }

    /// <summary>
    /// Acquires an access token using the service principal's client secret credential.
    /// </summary>
    /// <param name="scopes">The requested authentication scopes.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The acquired <see cref="AccessToken"/>.</returns>
    protected override async ValueTask<AccessToken> GetAccessTokenInternal(string[] scopes, CancellationToken cancellationToken) {
        TokenCredential tokenCredential = await this.GetCredential(cancellationToken);
        return await tokenCredential.GetTokenAsync(new TokenRequestContext(scopes), cancellationToken)
            .ConfigureAwait(false);
    }
}
