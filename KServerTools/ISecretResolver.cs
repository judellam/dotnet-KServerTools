namespace KServerTools.Common;

/// <summary>
/// A service for resolving a secret which can be stored in Azure Key Vault, locally, etc.
/// </summary>
/// <remarks>
/// 1. See <see cref="CanonicalSecretResolves"/> for supported secret types.
/// 2. Should be used in configuration settings where a secret is required.
/// </remarks>
public interface ISecretResolver {
    /// <summary>
    /// Resolves a secret value from the given secret reference string.
    /// </summary>
    /// <param name="secret">The secret reference string (e.g., "akv://secretName").</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The resolved secret value.</returns>
    ValueTask<string> Resolve(string secret, CancellationToken cancellationToken);

    /// <summary>
    /// Registers an Azure Key Vault service instance for secret resolution.
    /// </summary>
    /// <param name="keyVaultService">The Key Vault service to register.</param>
    void RegisterKeyVaultService(IAzureKeyVaultInternal keyVaultService);
}

/// <summary>
/// Supported secret storage types.
/// </summary>
public static class CanonicalSecretResolves {
    /// <summary>
    /// Azure Key Vault.
    /// </summary>
    /// <remarks>
    /// Example: akv://{{secretName}}
    /// The secret resolver will be given an instance of <see cref="IAzureKeyVaultService{T}"/> to resolve the secret.
    /// The assumption is it's in the format of "akv://{secretName}" and you have one AKV service to resolve all secrets.
    /// </remarks>
    public const string AzureKeyVaultConfiguration = "akv";
}
