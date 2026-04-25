namespace KServerTools.Common;

using Azure.Core;
using Microsoft.Extensions.Caching.Memory;

/// <summary>
/// Abstract base class for token credential implementations that provides caching of access tokens.
/// </summary>
/// <typeparam name="T">The credential configuration type implementing <see cref="ICredentialConfig"/>.</typeparam>
/// <param name="config">The credential configuration.</param>
public abstract class TokenCredentialBase<T>(T config) : TokenCredential, ITokenCredentialService where T : ICredentialConfig {
    private MemoryCache cache = new(new MemoryCacheOptions());

    /// <summary>
    /// Gets the credential configuration.
    /// </summary>
    public T Config { get; private set; } = config;

    /// <summary>
    /// Synchronously retrieves a cached access token or acquires a new one.
    /// </summary>
    /// <param name="requestContext">The token request context containing scopes.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The cached or newly acquired <see cref="AccessToken"/>.</returns>
    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken) {
        string key = string.Join(":", requestContext.Scopes);
        if (!this.cache.TryGetValue<AccessToken>(key, out AccessToken token)) {
            token = this.GetAccessTokenInternal(requestContext.Scopes, cancellationToken).Result;
            this.cache.Set(key, token, token.ExpiresOn.AddMinutes(-10));
        }

        return token;
    }

    /// <summary>
    /// Asynchronously retrieves a cached access token or acquires a new one.
    /// </summary>
    /// <param name="requestContext">The token request context containing scopes.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The cached or newly acquired <see cref="AccessToken"/>.</returns>
    public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken) {
        string key = string.Join(":", requestContext.Scopes);
        if (!this.cache.TryGetValue<AccessToken>(key, out AccessToken token)) {
            token = await this.GetAccessTokenInternal(requestContext.Scopes, cancellationToken);
            this.cache.Set(key, token, token.ExpiresOn.AddMinutes(-10));
        }

        return token;
    }

    /// <summary>
    /// Gets the underlying <see cref="TokenCredential"/> for this service.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The resolved <see cref="TokenCredential"/>.</returns>
    public abstract Task<TokenCredential> GetCredential(CancellationToken cancellationToken);

    /// <summary>
    /// Acquires an access token from the underlying credential provider.
    /// </summary>
    /// <param name="scopes">The requested authentication scopes.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The acquired <see cref="AccessToken"/>.</returns>
    protected abstract ValueTask<AccessToken> GetAccessTokenInternal(string[] scopes, CancellationToken cancellationToken);
}
