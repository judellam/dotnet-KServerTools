namespace KServerTools.Common;

/// <summary>
/// Azure Key Vault configuration settings.
/// </summary>
/// <remarks>
/// Example configuration found in appsettings.json:
///   "AzureKeyVaultConfiguration": {
///     "Uri": "https://{{akvname}}.vault.azure.net/",
///     "CacheDurationInSeconds": 300
///   },
/// </remarks>
public interface IAzureKeyVaultConfiguration {
    string Uri { get; set; }
    int CacheDurationInSeconds { get; set; }
}

internal class AzureKeyVaultConfiguration : IAzureKeyVaultConfiguration {
    public required string Uri { get; set ;}
    public int CacheDurationInSeconds { get; set; } = 300;
}