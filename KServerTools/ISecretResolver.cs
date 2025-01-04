namespace KServerTools.Common;

/// <summary>
/// Supported secret storage types.
/// </summary>
public static class CanonicalSecretResolves {
    /// <summary>
    /// Azure Key Vault.
    /// </summary>
    /// <remarks>
    /// Example: akv://{{secretName}}
    /// The secret resolver will be given an instance of <see cref="IAzureKeyVaultService"/> to resolve the secret.
    /// The assumption is it's in the format of "akv://{secretName}" and you have one AKV service to resolve all secrets.
    /// </remarks>
    public const string AzureKeyVaultConfiguration = "akv"; 
}

/// <summary>
/// A service for resolving a secret which can be stored in Azure Key Vault, locally, etc.
/// </summary>
/// <remarks>
/// 1. See <see cref="CanonicalSecretResolves"/> for supported secret types.
/// 2. Should be used in configuration settings where a secret is required.
/// </remarks>
public interface ISecretResolver {
    Task<string> Resolve(string secret, CancellationToken cancellationToken);
}
