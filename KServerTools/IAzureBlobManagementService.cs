namespace KServerTools.Common;

/// <summary>
/// Extended blob storage operations: delete, list, and existence checks.
/// </summary>
/// <remarks>
/// Separated from <see cref="IAzureStorageService{T}"/> to avoid breaking existing implementations.
/// </remarks>
public interface IAzureBlobManagementService<T> where T : IAzureStorageServiceConfig {
    /// <summary>
    /// Deletes a blob from the specified container. Returns true if the blob was deleted, false if it did not exist.
    /// </summary>
    Task<bool> DeleteBlobAsync(string containerName, string blobName, CancellationToken cancellationToken);

    /// <summary>
    /// Checks whether a blob exists in the specified container.
    /// </summary>
    Task<bool> BlobExistsAsync(string containerName, string blobName, CancellationToken cancellationToken);

    /// <summary>
    /// Lists blob names in a container, optionally filtered by prefix. Streams results for large containers.
    /// </summary>
    IAsyncEnumerable<string> ListBlobsAsync(string containerName, string? prefix, CancellationToken cancellationToken);

    /// <summary>
    /// Lists all blob names in a container, optionally filtered by prefix. Materializes the full list.
    /// For large containers, prefer the streaming <see cref="ListBlobsAsync"/> overload.
    /// </summary>
    Task<IReadOnlyList<string>> ListBlobsToListAsync(string containerName, string? prefix, CancellationToken cancellationToken);
}
