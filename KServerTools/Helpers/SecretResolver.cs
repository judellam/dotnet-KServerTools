namespace KServerTools.Common;

/// <summary>
/// Resolves secret values from Key Vault URIs or returns local plaintext values.
/// </summary>
internal class SecretResolver : ISecretResolver {
    private IAzureKeyVaultInternal? keyVaultService = null;
    private int registered = 0;

    private enum SecretType {
        /// <summary>
        /// Secret stored in Azure Key Vault.
        /// </summary>
        KeyVault,

        /// <summary>
        /// Local plaintext secret.
        /// </summary>
        Local,
    }

    /// <summary>
    /// Resolves a secret value from its URI or returns it as a local value.
    /// </summary>
    /// <param name="secret">The secret URI (e.g., "akv://SecretName") or plaintext value.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The resolved secret value.</returns>
    public async ValueTask<string> Resolve(string secret, CancellationToken cancellationToken) {
        var (type, value) = GetSecretType(secret);
        return type switch {
            SecretType.KeyVault => this.keyVaultService != null ?
                await this.keyVaultService.GetSecretAsync(value, cancellationToken).ConfigureAwait(false) :
                throw new InvalidOperationException("KeyVault service not registered."),
            SecretType.Local => value,
            _ => secret,
        };
    }

    /// <summary>
    /// Registers the Key Vault service for resolving "akv://" secret URIs.
    /// </summary>
    /// <param name="keyVaultService">The Key Vault service instance.</param>
    public void RegisterKeyVaultService(IAzureKeyVaultInternal keyVaultService) {
        if (Interlocked.CompareExchange(ref this.registered, 1, 0) != 0) {
            throw new InvalidOperationException("KeyVault service has already been registered. SecretResolver binding is immutable after initialization.");
        }

        this.keyVaultService = keyVaultService;
    }

    private static (SecretType, string) GetSecretType(string secret) {
        if (Uri.TryCreate(secret, UriKind.Absolute, out var uri)) {
            switch (uri.Scheme) {
                case "akv":
                    return (SecretType.KeyVault, uri.Host);
            }
        }

        return (SecretType.Local, secret);
    }
}
