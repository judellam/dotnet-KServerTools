namespace KServerTools.Common;

/// <summary>
/// Represents a service that interacts with Azure Storage.
/// </summary>
/// <typeparam name="T">The Azure Storage service configuration type.</typeparam>
/// <remarks>
/// This service is used to upload and download blobs from Azure Storage. The DFS endpoint is currently not supported.
/// </remarks>
public interface IAzureStorageService<T> where T : IAzureStorageServiceConfig {
    /// <summary>
    /// Uploads a blob to the specified container.
    /// </summary>
    /// <param name="containerName">The name of the blob container.</param>
    /// <param name="blobName">The name of the blob.</param>
    /// <param name="stream">The stream containing the blob content.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task UploadBlobAsync(
        string containerName,
        string blobName,
        Stream stream,
        CancellationToken cancellationToken);

    /// <summary>
    /// Appends a blob to the specified container.
    /// </summary>
    /// <param name="containerName">The name of the blob container.</param>
    /// <param name="blobName">The name of the blob.</param>
    /// <param name="stream">The stream containing the content to append.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task AppendAsync(
        string containerName,
        string blobName,
        Stream stream,
        CancellationToken cancellationToken);

    /// <summary>
    /// Downloads a blob from the specified container.
    /// </summary>
    /// <param name="containerName">The name of the blob container.</param>
    /// <param name="blobName">The name of the blob.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="Stream"/> containing the blob content.</returns>
    public Task<Stream> DownloadBlobAsync(
        string containerName,
        string blobName,
        CancellationToken cancellationToken);
}
