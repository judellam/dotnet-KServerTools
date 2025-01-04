namespace KServerTools.Common;

internal class SecretResolver(IAzureKeyVaultService keyVaultService) : ISecretResolver {
    private enum SecretType {
        KeyVault,
        Local,
    }

    private readonly IAzureKeyVaultService keyVaultService = keyVaultService;

    public async Task<string> Resolve(string secret, CancellationToken cancellationToken) {
        var (type, value) = GetSecretType(secret);
        return type switch
        {
            SecretType.KeyVault => await this.keyVaultService.GetSecretAsync(value, cancellationToken).ConfigureAwait(false),
            SecretType.Local => value,
            _ => secret,
        };
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