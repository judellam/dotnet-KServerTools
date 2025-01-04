namespace KServerTools.Common;

/// <summary>
/// Interface for an Azure service principal credential.
/// </summary>
/// <typeparam name="T">Configuration for the credential.</typeparam>
public interface IServicePrincipalCredential<T> : ICredentialResolver where T: ICredentialConfig {
    /// <summary>
    /// Gets the access token. The access token is cached and will be refreshed if it's near expiry.
    /// </summary>
    Task<string> GetAccessToken(string[] scopes, CancellationToken cancellationToken);
}