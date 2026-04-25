namespace KServerTools.Common;

/// <summary>
/// Configuration for Azure Storage Service. Can be inherited from to add additional configurations and storage accounts.
/// </summary>
/// <param name="accountName">The Azure Storage Account name.</param>
/// <param name="endpoint">The Azure Storage Account endpoint.</param>
public class AzureStorageServiceConfig(string accountName, string endpoint) : IAzureStorageServiceConfig {
    /// <summary>
    /// Gets the Azure Storage Account name.
    /// </summary>
    public string AccountName { get; } = accountName;

    /// <summary>
    /// Gets the Azure Storage Account endpoint.
    /// </summary>
    public string Endpoint { get; } = endpoint;
}
