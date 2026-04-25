namespace KServerTools.Common;

using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Caching.Memory;

/// <summary>
/// Internal Azure Storage blob service that provides direct blob operations without logging wrappers.
/// </summary>
/// <typeparam name="T">The configuration type implementing <see cref="IAzureStorageServiceConfig"/>.</typeparam>
/// <typeparam name="C">The credential type implementing <see cref="ITokenCredentialService"/>.</typeparam>
/// <param name="config">The storage service configuration.</param>
/// <param name="credential">The token credential used for authentication.</param>
/// <param name="memoryCache">The memory cache for client reuse.</param>
internal class AzureStorageServiceInternal<T, C>(T config, C credential, IMemoryCache memoryCache) : AzureStorageBase<T, C>(config, credential, memoryCache), IAzureStorageService<T>, IAzureBlobManagementService<T> where T : class, IAzureStorageServiceConfig where C : ITokenCredentialService {
    /// <summary>
    /// Uploads a blob to the specified container.
    /// </summary>
    /// <param name="containerName">The name of the blob container.</param>
    /// <param name="blobName">The name of the blob.</param>
    /// <param name="stream">The stream containing the blob content.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task UploadBlobAsync(string containerName, string blobName, Stream stream, CancellationToken cancellationToken) {
        Verify(containerName, blobName);

        BlobContainerClient containerClient = await this.GetContainerClient(containerName, true, cancellationToken)
            .ConfigureAwait(false);

        BlobClient blobClient = containerClient.GetBlobClient(blobName);

        await blobClient.UploadAsync(stream, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Downloads a blob from the specified container as a stream.
    /// </summary>
    /// <param name="containerName">The name of the blob container.</param>
    /// <param name="blobName">The name of the blob.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="Stream"/> containing the blob content.</returns>
    public async Task<Stream> DownloadBlobAsync(string containerName, string blobName, CancellationToken cancellationToken) {
        Verify(containerName, blobName);

        BlobContainerClient blobContainerClient = await this.GetContainerClient(containerName, false, cancellationToken)
            .ConfigureAwait(false);

        BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);

        Stream stream = new MemoryStream();
        await blobClient.DownloadToAsync(stream, cancellationToken)
            .ConfigureAwait(false);

        stream.Position = 0;
        return stream;
    }

    /// <summary>
    /// Appends content to an append blob in the specified container.
    /// </summary>
    /// <param name="containerName">The name of the blob container.</param>
    /// <param name="blobName">The name of the append blob.</param>
    /// <param name="stream">The stream containing the content to append.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task AppendAsync(string containerName, string blobName, Stream stream, CancellationToken cancellationToken) {
        Verify(containerName, blobName);
        BlobContainerClient blobContainerClient = await this.GetContainerClient(containerName, true, cancellationToken)
            .ConfigureAwait(false);

        AppendBlobClient blobClient = blobContainerClient.GetAppendBlobClient(blobName);
        await blobClient.CreateIfNotExistsAsync()
            .ConfigureAwait(false);

        await blobClient.AppendBlockAsync(stream, null, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes a blob from the specified container if it exists.
    /// </summary>
    /// <param name="containerName">The name of the blob container.</param>
    /// <param name="blobName">The name of the blob to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see langword="true"/> if the blob was deleted; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> DeleteBlobAsync(string containerName, string blobName, CancellationToken cancellationToken) {
        Verify(containerName, blobName);

        BlobContainerClient containerClient = await this.GetContainerClient(containerName, false, cancellationToken)
            .ConfigureAwait(false);

        BlobClient blobClient = containerClient.GetBlobClient(blobName);
        var response = await blobClient.DeleteIfExistsAsync(Azure.Storage.Blobs.Models.DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return response.Value;
    }

    /// <summary>
    /// Checks whether a blob exists in the specified container.
    /// </summary>
    /// <param name="containerName">The name of the blob container.</param>
    /// <param name="blobName">The name of the blob.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see langword="true"/> if the blob exists; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> BlobExistsAsync(string containerName, string blobName, CancellationToken cancellationToken) {
        Verify(containerName, blobName);

        BlobContainerClient containerClient = await this.GetContainerClient(containerName, false, cancellationToken)
            .ConfigureAwait(false);

        BlobClient blobClient = containerClient.GetBlobClient(blobName);
        var response = await blobClient.ExistsAsync(cancellationToken).ConfigureAwait(false);
        return response.Value;
    }

    /// <summary>
    /// Lists blob names in the specified container as an async enumerable.
    /// </summary>
    /// <param name="containerName">The name of the blob container.</param>
    /// <param name="prefix">An optional prefix to filter blobs by name.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An async enumerable of blob names.</returns>
    public async IAsyncEnumerable<string> ListBlobsAsync(string containerName, string? prefix, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNullOrEmpty(containerName, nameof(containerName));

        BlobContainerClient containerClient = await this.GetContainerClient(containerName, false, cancellationToken)
            .ConfigureAwait(false);

        await foreach (var blobItem in containerClient.GetBlobsAsync(prefix: prefix, cancellationToken: cancellationToken).ConfigureAwait(false)) {
            yield return blobItem.Name;
        }
    }

    /// <summary>
    /// Lists blob names in the specified container and returns them as a list.
    /// </summary>
    /// <param name="containerName">The name of the blob container.</param>
    /// <param name="prefix">An optional prefix to filter blobs by name.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A read-only list of blob names.</returns>
    public async Task<IReadOnlyList<string>> ListBlobsToListAsync(string containerName, string? prefix, CancellationToken cancellationToken) {
        var results = new List<string>();
        await foreach (var name in this.ListBlobsAsync(containerName, prefix, cancellationToken).ConfigureAwait(false)) {
            results.Add(name);
        }

        return results;
    }

    /// <summary>
    /// Gets or creates a cached <see cref="BlobContainerClient"/> for the specified container.
    /// </summary>
    /// <param name="containerName">The name of the blob container.</param>
    /// <param name="createIfNotExists">Whether to create the container if it does not exist.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="BlobContainerClient"/> for the container.</returns>
    protected async Task<BlobContainerClient> GetContainerClient(string containerName, bool createIfNotExists, CancellationToken cancellationToken) {
        string key = $"blob:{this.config.AccountName}:{containerName}";
        return await this.GetOrCreateCachedAsync(key, async () => {
            Uri storageUri = new($"https://{this.config.AccountName}.{this.config.Endpoint}/{containerName}");
            var client = new BlobContainerClient(storageUri, await this.credential.GetCredential(cancellationToken));

            if (createIfNotExists) {
                await client.CreateIfNotExistsAsync()
                    .ConfigureAwait(false);
            }

            return client;
        }).ConfigureAwait(false);
    }
}
