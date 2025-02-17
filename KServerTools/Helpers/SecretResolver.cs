namespace KServerTools.Common;

internal class SecretResolver : ISecretResolver {
    private enum SecretType {
        KeyVault,
        Local,
    }

    public SecretResolver() { 
        Console.WriteLine("SecretResolver created.");
    }

    private IAzureKeyVaultInternal? keyVaultService = null;

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