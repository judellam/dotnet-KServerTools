namespace KServerTools.Common;

using Azure.Core;
using Azure.Identity;

internal class DefaultCredentialConfig : ICredentialConfig {
    public ServiceCredentalType CredentialType => ServiceCredentalType.DefaultCredential;

    public Task<string> GetResolvedSecret(CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }
}

internal class DefaultCredential<T>(T config) : TokenCredentialBase<T>(config), IDefaultCredential where T: ICredentialConfig {
    /// <summary>
    /// Retrieves the default Azure credential.
    /// </summary>
    /// <returns>A <see cref="TokenCredential"/> instance representing the default Azure credential.</returns>
    public override async Task<TokenCredential> GetCredential(CancellationToken cancellationToken) {
        return await Task.FromResult<TokenCredential>(new DefaultAzureCredential());
    }

    protected override async ValueTask<AccessToken> GetAccessTokenInternal(string[] scopes, CancellationToken cancellationToken) {
        TokenCredential tokenCredential = await this.GetCredential(cancellationToken);
        return await tokenCredential.GetTokenAsync(new TokenRequestContext(scopes), cancellationToken);
    }
}