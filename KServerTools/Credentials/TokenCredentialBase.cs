namespace KServerTools.Common;

using Azure.Core;
using Microsoft.Extensions.Caching.Memory;

public abstract class TokenCredentialBase<T>(T config) : TokenCredential, ITokenCredentialService where T: ICredentialConfig {
    private MemoryCache cache = new(new MemoryCacheOptions());

    public T Config { get; private set; } = config;

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken) {
        string key = string.Join(":", requestContext.Scopes);
        if (!this.cache.TryGetValue<AccessToken>(key, out AccessToken token)) {
            token = this.GetAccessTokenInternal(requestContext.Scopes, cancellationToken).Result;
            this.cache.Set(key, token, token.ExpiresOn.AddMinutes(-10));
        }
        return token;
    }
    
    public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken) {
        string key = string.Join(":", requestContext.Scopes);
        if (!this.cache.TryGetValue<AccessToken>(key, out AccessToken token)) {
            token = await this.GetAccessTokenInternal(requestContext.Scopes, cancellationToken);
            this.cache.Set(key, token, token.ExpiresOn.AddMinutes(-10));
        }
        return token;
    }

    public abstract Task<TokenCredential> GetCredential(CancellationToken cancellationToken);

    protected abstract ValueTask<AccessToken> GetAccessTokenInternal(string[] scopes, CancellationToken cancellationToken);
}