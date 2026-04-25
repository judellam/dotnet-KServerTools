using System.Security.Cryptography.X509Certificates;

namespace KServerTools.Common;

/// <summary>
/// Interface for the Secret Resolver backed by Azure Key Vault.
/// </summary>
public interface IAzureKeyVaultInternal {
    /// <summary>
    /// Gets the Secret associated with the secretName.
    /// </summary>
    /// <param name="secretName">The name of the secret to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <remarks>
    /// Requires Key Vault Secrets Officer to have the appropriate permissions to retrieve the secret.
    /// </remarks>
    /// <returns>The secret value as a string.</returns>
    Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the Certificate associated with the secretName.
    /// </summary>
    /// <param name="certificateName">The name of the certificate to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <remarks>
    /// Requires Key Vault Certificate Officer to have the appropriate permissions to retrieve the secret.
    /// </remarks>
    /// <returns>The <see cref="X509Certificate2"/> instance.</returns>
    Task<X509Certificate2> GetCertificate(string certificateName, CancellationToken cancellationToken);
}

/// <summary>
/// The Azure Key Vault Service.
/// </summary>
/// <typeparam name="T">The Azure Key Vault configuration type.</typeparam>
public interface IAzureKeyVaultService<T> : IAzureKeyVaultInternal where T : IAzureKeyVaultConfiguration {
}
