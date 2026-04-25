namespace KServerTools.Common;

/// <summary>
/// Storage configuration for Azure Storage Service Log, including a container name.
/// </summary>
/// <param name="containerName">The name of the storage container for logs.</param>
/// <param name="accountName">The Azure Storage Account name.</param>
/// <param name="endpoint">The Azure Storage Account endpoint.</param>
public class AzureStorageServiceLogConfig(string containerName, string accountName, string endpoint)
    : AzureStorageServiceConfig(accountName, endpoint) {
    /// <summary>
    /// Gets the name of the storage container for logs.
    /// </summary>
    public string ContainerName { get; } = containerName;
}
