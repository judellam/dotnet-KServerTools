using System.Security.Cryptography.X509Certificates;

namespace KServerTools.Common;

/// <summary>
/// Interface for the Sceret Resolver
/// </summary>
public interface IAzureKeyVaultInternal {
        /// <summary>
    /// Gets the Secret associated with the secretName
    /// </summary>
    /// <remarks>
    /// Requires Key Vault Secrets Officer to have the appropriate permissions to retrieve the secret.
    /// </remarks>
    Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the Certificate associated with the secretName
    /// </summary>
    /// <remarks>
    /// Requires Key Vault Certificate Officer to have the appropriate permissions to retrieve the secret.
    /// </remarks>
    Task<X509Certificate2>GetCertificate(string certificateName, CancellationToken cancellationToken);
}

/// <summary>
/// The Azure Key Value Service
/// </summary>
public interface IAzureKeyVaultService<T> : IAzureKeyVaultInternal where T : IAzureKeyVaultConfiguration {
}