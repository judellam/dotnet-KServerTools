# Blob Storage

KServerTools wraps Azure Blob Storage with two interfaces:

| Interface | Purpose |
|-----------|---------|
| `IAzureStorageService<T>` | Upload, download, and append blobs |
| `IAzureBlobManagementService<T>` | Delete, list, and check existence of blobs |

Both are registered automatically when you call `AddBlobStorage<T>(...)`.

## Registration

```csharp
// Fluent builder
services.AddKServerTools(kst => kst
    .AddCommon()
    .AddBlobStorage<MyStorageConfig>("AzureStorage")
);

// Legacy
services
    .KSTAddCommon()
    .KSTAddAzureStorageService<MyStorageConfig, IDefaultCredential>("AzureStorage");
```

## Configuration

```json
{
  "AzureStorage": {
    "AccountName": "mystorageaccount",
    "Endpoint": "blob.core.windows.net"
  }
}
```

```csharp
public class MyStorageConfig : IAzureStorageServiceConfig {
    public string AccountName { get; set; } = "";
    public string Endpoint { get; set; } = "";
}
```

## Usage — `IAzureStorageService<T>`

### Upload a Blob

```csharp
public class MyService(IAzureStorageService<MyStorageConfig> storage) {

    public async Task UploadAsync(string data, CancellationToken ct) {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
        await storage.UploadBlobAsync("my-container", "path/to/file.json", stream, ct);
    }
}
```

### Download a Blob

```csharp
using var stream = await storage.DownloadBlobAsync("my-container", "path/to/file.json", ct);
using var reader = new StreamReader(stream);
string content = await reader.ReadToEndAsync();
```

### Append to a Blob

```csharp
using var stream = new MemoryStream(Encoding.UTF8.GetBytes(newLine));
await storage.AppendAsync("my-container", "logs/app.log", stream, ct);
```

> Containers are created automatically if they do not exist when uploading or appending.

## Usage — `IAzureBlobManagementService<T>`

### Check if a Blob Exists

```csharp
public class MyService(IAzureBlobManagementService<MyStorageConfig> blobs) {

    public async Task<bool> CheckAsync(CancellationToken ct) {
        return await blobs.BlobExistsAsync("my-container", "path/to/file.json", ct);
    }
}
```

### Delete a Blob

```csharp
bool deleted = await blobs.DeleteBlobAsync("my-container", "path/to/file.json", ct);
// Returns false if the blob did not exist
```

### List Blobs (Streaming)

```csharp
await foreach (var blobName in blobs.ListBlobsAsync("my-container", "prefix/", ct)) {
    Console.WriteLine(blobName);
}
```

### List Blobs (Materialized)

```csharp
IReadOnlyList<string> names = await blobs.ListBlobsToListAsync("my-container", "prefix/", ct);
```

## Interface Reference

### `IAzureStorageService<T>`

```csharp
Task UploadBlobAsync(string containerName, string blobName, Stream stream, CancellationToken ct);
Task AppendAsync(string containerName, string blobName, Stream stream, CancellationToken ct);
Task<Stream> DownloadBlobAsync(string containerName, string blobName, CancellationToken ct);
```

### `IAzureBlobManagementService<T>`

```csharp
Task<bool> DeleteBlobAsync(string containerName, string blobName, CancellationToken ct);
Task<bool> BlobExistsAsync(string containerName, string blobName, CancellationToken ct);
IAsyncEnumerable<string> ListBlobsAsync(string containerName, string? prefix, CancellationToken ct);
Task<IReadOnlyList<string>> ListBlobsToListAsync(string containerName, string? prefix, CancellationToken ct);
```
