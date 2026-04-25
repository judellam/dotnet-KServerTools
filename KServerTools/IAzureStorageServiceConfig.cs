namespace KServerTools.Common;

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
