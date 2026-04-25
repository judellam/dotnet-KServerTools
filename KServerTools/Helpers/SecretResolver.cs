namespace KServerTools.Common;

internal class SecretResolver : ISecretResolver {
    private enum SecretType {
        KeyVault,
        Local,
    }

    private IAzureKeyVaultInternal? keyVaultService = null;
    private int registered = 0;

    public async ValueTask<string> Resolve(string secret, CancellationToken cancellationToken) {
        var (type, value) = GetSecretType(secret);
        return type switch
        {
            SecretType.KeyVault => this.keyVaultService != null ? 
                await this.keyVaultService.GetSecretAsync(value, cancellationToken).ConfigureAwait(false) : 
                throw new InvalidOperationException("KeyVault service not registered."),
            SecretType.Local => value,
            _ => secret,
        };
    }

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