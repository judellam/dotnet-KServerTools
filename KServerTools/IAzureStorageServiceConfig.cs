namespace KServerTools.Common;

/// <summary>
/// Inherit from this class to configure an Azure Storage Service for each unique storage account/configuration.
/// </summary>
public class AzureStorageServiceConfig(string AccountName, string Endpoint) : IAzureStorageServiceConfig {
    public string AccountName { get; } = AccountName;
    public string Endpoint { get; } = Endpoint;
}


public class AzureStorageServiceLogConfig(string ContainerName, string AccountName, string Endpoint)
    : AzureStorageServiceConfig(AccountName, Endpoint)
{
    public string ContainerName { get; } = ContainerName;
}


public interface IAzureStorageServiceConfig {
    public string AccountName { get; }
    public string Endpoint { get; }
}