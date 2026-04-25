namespace KServerTools.Common;

/// <summary>
/// Azure Key Vault configuration settings.
/// </summary>
/// <remarks>
/// Example configuration found in appsettings.json:
///   "AzureKeyVaultConfiguration": {
///     "Uri": "https://{{akvname}}.vault.azure.net/",
///     "CacheDurationInSeconds": 300
///   }.
/// </remarks>
public interface IAzureKeyVaultConfiguration {
    /// <summary>
    /// Gets or sets the URI of the Azure Key Vault instance.
    /// </summary>
    string Uri { get; set; }

    /// <summary>
    /// Gets or sets the duration in seconds for which secrets are cached.
    /// </summary>
    int CacheDurationInSeconds { get; set; }
}
