namespace KServerTools.Common;

/// <summary>
/// Represents a service that interacts with Azure Storage.
/// </summary>
/// <remarks>
/// This service is used to upload and download blobs from Azure Storage. The DFS endpoint is currently not supported.
/// </remarks>
public interface IAzureStorageService<T> where T : IAzureStorageServiceConfig {
    /// <summary>
    /// Uploads a blob to the specified container.
    /// </summary>
    public Task UploadBlobAsync(
        string containerName,
        string blobName,
        Stream stream,
        CancellationToken cancellationToken);

    /// <summary>
    /// Appends a blob to the specified container.
    /// </summary>
    public Task AppendAsync(
        string containerName,
        string blobName,
        Stream stream,
        CancellationToken cancellationToken);
    
    /// <summary>
    /// Downloads a blob from the specified container.
    /// </summary>
    public Task<Stream> DownloadBlobAsync(
        string containerName,
        string blobName,
        CancellationToken cancellationToken);
}