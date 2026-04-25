namespace KServerTools.Common;

using Azure.Core;

/// <summary>
/// Service for obtaining Azure token credentials.
/// </summary>
public interface ITokenCredentialService {
    /// <summary>
    /// Gets an access token synchronously for the specified request context.
    /// </summary>
    /// <param name="requestContext">The token request context specifying the required scopes.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The <see cref="AccessToken"/> for the requested scopes.</returns>
    AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken);

    /// <summary>
    /// Gets an access token asynchronously for the specified request context.
    /// </summary>
    /// <param name="requestContext">The token request context specifying the required scopes.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The <see cref="AccessToken"/> for the requested scopes.</returns>
    ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the credential.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="TokenCredential"/> instance.</returns>
    Task<TokenCredential> GetCredential(CancellationToken cancellationToken);
}
