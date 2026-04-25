namespace KServerTools.Common;

/// <summary>
/// Extended blob storage operations: delete, list, and existence checks.
/// </summary>
/// <typeparam name="T">The Azure Storage service configuration type.</typeparam>
/// <remarks>
/// Separated from <see cref="IAzureStorageService{T}"/> to avoid breaking existing implementations.
/// </remarks>
public interface IAzureBlobManagementService<T> where T : IAzureStorageServiceConfig {
    /// <summary>
    /// Deletes a blob from the specified container. Returns true if the blob was deleted, false if it did not exist.
    /// </summary>
    /// <param name="containerName">The name of the blob container.</param>
    /// <param name="blobName">The name of the blob to delete.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns><see langword="true"/> if the blob was deleted; <see langword="false"/> if it did not exist.</returns>
    Task<bool> DeleteBlobAsync(string containerName, string blobName, CancellationToken cancellationToken);

    /// <summary>
    /// Checks whether a blob exists in the specified container.
    /// </summary>
    /// <param name="containerName">The name of the blob container.</param>
    /// <param name="blobName">The name of the blob to check.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns><see langword="true"/> if the blob exists; otherwise, <see langword="false"/>.</returns>
    Task<bool> BlobExistsAsync(string containerName, string blobName, CancellationToken cancellationToken);

    /// <summary>
    /// Lists blob names in a container, optionally filtered by prefix. Streams results for large containers.
    /// </summary>
    /// <param name="containerName">The name of the blob container.</param>
    /// <param name="prefix">An optional prefix to filter blob names.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>An asynchronous enumerable of blob names.</returns>
    IAsyncEnumerable<string> ListBlobsAsync(string containerName, string? prefix, CancellationToken cancellationToken);

    /// <summary>
    /// Lists all blob names in a container, optionally filtered by prefix. Materializes the full list.
    /// For large containers, prefer the streaming <see cref="ListBlobsAsync"/> overload.
    /// </summary>
    /// <param name="containerName">The name of the blob container.</param>
    /// <param name="prefix">An optional prefix to filter blob names.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of blob names.</returns>
    Task<IReadOnlyList<string>> ListBlobsToListAsync(string containerName, string? prefix, CancellationToken cancellationToken);
}
