namespace KServerTools.Common;

/// <summary>
/// Configuration for Azure Storage Service. Can be inheritted from to add additional configurations and storage accounts.
/// </summary>
public class AzureStorageServiceConfig(string accountName, string endpoint) : IAzureStorageServiceConfig {
    public string AccountName { get; } = accountName;
    public string Endpoint { get; } = endpoint;
}

/// <summary>
/// Storage container name for Azure Storage Service Log.
/// </summary>
public class AzureStorageServiceLogConfig(string containerName, string accountName, string endpoint)
    : AzureStorageServiceConfig(accountName, endpoint) {
    public string ContainerName { get; } = containerName;
}

/// <summary>
/// Implement this interface to configure an Azure Storage Service for each unique storage account/configuration.
/// </summary>
public interface IAzureStorageServiceConfig {
    /// <summary>
    /// Gets name of the Azure Storage Account.
    /// </summary>
    public string AccountName { get; }

    /// <summary>
    /// Gets endpoint of the Azure Storage Account (like blob.core.windows.net or queue.core.windows.net).
    /// </summary>
    public string Endpoint { get; }
}
