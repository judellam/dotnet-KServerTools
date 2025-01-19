namespace KServerTools.Common;

internal class AzureKeyVaultConfiguration : IAzureKeyVaultConfiguration {
    public required string Uri { get; set ;}
    public int CacheDurationInSeconds { get; set; } = 300;
}