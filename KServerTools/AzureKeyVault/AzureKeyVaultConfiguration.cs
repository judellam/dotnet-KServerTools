namespace KServerTools.Common;

/// <summary>
/// Configuration settings for Azure Key Vault.
/// </summary>
public class AzureKeyVaultConfiguration : IAzureKeyVaultConfiguration {
    /// <summary>
    /// Gets or sets the URI of the Azure Key Vault instance.
    /// </summary>
    public required string Uri { get; set; }

    /// <summary>
    /// Gets or sets the cache duration in seconds for secrets and certificates.
    /// </summary>
    public int CacheDurationInSeconds { get; set; } = 300;
}
