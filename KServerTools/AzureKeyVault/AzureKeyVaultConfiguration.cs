namespace KServerTools.Common;

public class AzureKeyVaultConfiguration : IAzureKeyVaultConfiguration {
    public required string Uri { get; set ;}
    public int CacheDurationInSeconds { get; set; } = 300;
}