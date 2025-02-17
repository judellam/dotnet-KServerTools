namespace KServerTools.Common;

/// <summary>
/// Configuration for Azure Storage Service. Can be inheritted from to add additional configurations and storage accounts.
/// </summary>
public class AzureStorageServiceConfig(string AccountName, string Endpoint) : IAzureStorageServiceConfig {
    public string AccountName { get; } = AccountName;
    public string Endpoint { get; } = Endpoint;
}


/// <summary>
/// Storage container name for Azure Storage Service Log.
/// </summary>
public class AzureStorageServiceLogConfig(string ContainerName, string AccountName, string Endpoint)
    : AzureStorageServiceConfig(AccountName, Endpoint) {
    public string ContainerName { get; } = ContainerName;
}


/// <summary>
/// Implement this interface to configure an Azure Storage Service for each unique storage account/configuration.
/// </summary>
public interface IAzureStorageServiceConfig {
    /// <summary>
    /// Name of the Azure Storage Account.
    /// </summary>
    public string AccountName { get; }
    /// <summary>
    /// Endpoint of the Azure Storage Account (like blob.core.windows.net or queue.core.windows.net).
    /// </summary>
    public string Endpoint { get; }
}