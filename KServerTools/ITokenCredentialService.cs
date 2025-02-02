namespace KServerTools.Common;

using Azure.Core;

public interface ITokenCredentialService {
    AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken);
    ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken);
    /// <summary>
    /// Retrieves the credential.
    /// </summary>
    /// <returns>A <see cref="TokenCredential"/> instance.</returns>
    Task<TokenCredential> GetCredential(CancellationToken cancellationToken);
}