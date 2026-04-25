namespace KServerTools.Common;

using Azure.Core;
using Azure.Identity;

/// <summary>
/// Provides a default Azure credential using <see cref="DefaultAzureCredential"/>.
/// </summary>
/// <typeparam name="T">The credential configuration type.</typeparam>
/// <param name="config">The credential configuration instance.</param>
internal class DefaultCredential<T>(T config) : TokenCredentialBase<T>(config), IDefaultCredential where T : ICredentialConfig {
    /// <summary>
    /// Retrieves the default Azure credential.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A <see cref="TokenCredential"/> instance representing the default Azure credential.</returns>
    public override async Task<TokenCredential> GetCredential(CancellationToken cancellationToken) {
        return await Task.FromResult<TokenCredential>(new DefaultAzureCredential());
    }

    /// <summary>
    /// Obtains an access token for the specified scopes using the default credential.
    /// </summary>
    /// <param name="scopes">The resource scopes to request.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>An <see cref="AccessToken"/> for the requested scopes.</returns>
    protected override async ValueTask<AccessToken> GetAccessTokenInternal(string[] scopes, CancellationToken cancellationToken) {
        TokenCredential tokenCredential = await this.GetCredential(cancellationToken);
        return await tokenCredential.GetTokenAsync(new TokenRequestContext(scopes), cancellationToken);
    }
}
