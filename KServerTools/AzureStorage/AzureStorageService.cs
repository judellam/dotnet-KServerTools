namespace KServerTools.Common;

using System.Threading.Tasks;

/// <summary>
/// Azure Storage blob service that wraps internal operations with logging.
/// </summary>
/// <typeparam name="T">The configuration type implementing <see cref="IAzureStorageServiceConfig"/>.</typeparam>
/// <typeparam name="C">The credential type implementing <see cref="ITokenCredentialService"/>.</typeparam>
/// <param name="config">The storage service configuration.</param>
/// <param name="credential">The token credential used for authentication.</param>
/// <param name="logger">The JSON logger instance.</param>
/// <param name="memoryCache">The memory cache for client reuse.</param>
internal class AzureStorageService<T, C>(T config, C credential, IJsonLogger logger, Microsoft.Extensions.Caching.Memory.IMemoryCache memoryCache) : IAzureStorageService<T>, IAzureBlobManagementService<T> where T : class, IAzureStorageServiceConfig where C : ITokenCredentialService {
    private readonly AzureStorageServiceInternal<T, C> service = new(config, credential, memoryCache);
    private readonly IJsonLogger logger = logger;

    /// <summary>
    /// Uploads a blob to the specified container.
    /// </summary>
    /// <param name="containerName">The name of the blob container.</param>
    /// <param name="blobName">The name of the blob.</param>
    /// <param name="stream">The stream containing the blob content.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task UploadBlobAsync(string containerName, string blobName, Stream stream, CancellationToken cancellationToken) =>
        AzureServiceBaseHelpers.LoggedOperationAsync(this.logger,
            $"Uploaded blob {blobName} to container {containerName}",
            () => this.service.UploadBlobAsync(containerName, blobName, stream, cancellationToken));

    /// <summary>
    /// Downloads a blob from the specified container as a stream.
    /// </summary>
    /// <param name="containerName">The name of the blob container.</param>
    /// <param name="blobName">The name of the blob.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="Stream"/> containing the blob content.</returns>
    public Task<Stream> DownloadBlobAsync(string containerName, string blobName, CancellationToken cancellationToken) =>
        AzureServiceBaseHelpers.LoggedOperationAsync(this.logger,
            $"Downloaded blob {blobName} from container {containerName}",
            () => this.service.DownloadBlobAsync(containerName, blobName, cancellationToken));

    /// <summary>
    /// Appends content to an append blob in the specified container.
    /// </summary>
    /// <param name="containerName">The name of the blob container.</param>
    /// <param name="blobName">The name of the append blob.</param>
    /// <param name="stream">The stream containing the content to append.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task AppendAsync(string containerName, string blobName, Stream stream, CancellationToken cancellationToken) =>
        AzureServiceBaseHelpers.LoggedOperationAsync(this.logger,
            $"Appended to blob {blobName} in container {containerName}",
            () => this.service.AppendAsync(containerName, blobName, stream, cancellationToken));

    /// <summary>
    /// Deletes a blob from the specified container if it exists.
    /// </summary>
    /// <param name="containerName">The name of the blob container.</param>
    /// <param name="blobName">The name of the blob to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see langword="true"/> if the blob was deleted; otherwise, <see langword="false"/>.</returns>
    public Task<bool> DeleteBlobAsync(string containerName, string blobName, CancellationToken cancellationToken) =>
        AzureServiceBaseHelpers.LoggedOperationAsync(this.logger,
            $"Deleted blob {blobName} from container {containerName}",
            () => this.service.DeleteBlobAsync(containerName, blobName, cancellationToken));

    /// <summary>
    /// Checks whether a blob exists in the specified container.
    /// </summary>
    /// <param name="containerName">The name of the blob container.</param>
    /// <param name="blobName">The name of the blob.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see langword="true"/> if the blob exists; otherwise, <see langword="false"/>.</returns>
    public Task<bool> BlobExistsAsync(string containerName, string blobName, CancellationToken cancellationToken) =>
        AzureServiceBaseHelpers.LoggedOperationAsync(this.logger,
            $"Checked existence of blob {blobName} in container {containerName}",
            () => this.service.BlobExistsAsync(containerName, blobName, cancellationToken));

    /// <summary>
    /// Lists blob names in the specified container as an async enumerable.
    /// </summary>
    /// <param name="containerName">The name of the blob container.</param>
    /// <param name="prefix">An optional prefix to filter blobs by name.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An async enumerable of blob names.</returns>
    public async IAsyncEnumerable<string> ListBlobsAsync(string containerName, string? prefix, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken) {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        int count = 0;
        try {
            await foreach (var name in this.service.ListBlobsAsync(containerName, prefix, cancellationToken).ConfigureAwait(false)) {
                count++;
                yield return name;
            }
        } finally {
            sw.Stop();
            if (cancellationToken.IsCancellationRequested) {
                this.logger.Warn($"Cancelled: List blobs in {containerName} (prefix: {prefix}), yielded {count} items", null, sw.ElapsedMilliseconds);
            } else {
                this.logger.Info($"Listed {count} blobs in {containerName} (prefix: {prefix})", sw.ElapsedMilliseconds);
            }
        }
    }

    /// <summary>
    /// Lists blob names in the specified container and returns them as a list.
    /// </summary>
    /// <param name="containerName">The name of the blob container.</param>
    /// <param name="prefix">An optional prefix to filter blobs by name.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A read-only list of blob names.</returns>
    public Task<IReadOnlyList<string>> ListBlobsToListAsync(string containerName, string? prefix, CancellationToken cancellationToken) =>
        AzureServiceBaseHelpers.LoggedOperationAsync(this.logger,
            $"Listed blobs in container {containerName} with prefix {prefix}",
            () => this.service.ListBlobsToListAsync(containerName, prefix, cancellationToken));
}
