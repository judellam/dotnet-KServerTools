# Cosmos DB

KServerTools wraps Azure Cosmos DB with full CRUD and query support.

## Registration

```csharp
services.AddKServerTools(kst => kst
    .AddCommon()
    .AddCosmosDb<MyCosmosConfig>("CosmosDb")
);
```

## Configuration

```json
{
  "CosmosDb": {
    "EndpointUri": "https://myaccount.documents.azure.com:443/",
    "PrimaryKey": ""
  }
}
```

```csharp
public class MyCosmosConfig : IAzureCosmosDbConfiguration {
    public required string EndpointUri { get; set; }
    public string PrimaryKey { get; set; } = "";
}
```

> When using `IDefaultCredential`, the `PrimaryKey` can be left empty — authentication uses the token credential instead.

## Entity Model

All Cosmos items must implement `ICosmosEntity`:

```csharp
public interface ICosmosEntity {
    string Id { get; }
    string PartitionKey { get; }
}
```

Example:

```csharp
public class UserEntity : ICosmosEntity {
    public string Id { get; set; } = "";
    public string PartitionKey => TenantId;
    public string TenantId { get; set; } = "";
    public string Name { get; set; } = "";
}
```

## Usage

### Create a Database and Container

```csharp
public class DataSetup(IAzureCosmosDb<MyCosmosConfig> cosmos) {

    public async Task InitializeAsync(CancellationToken ct) {
        await cosmos.CreateDatabaseAsync("MyDb", ct);
        await cosmos.CreateContainerAsync("MyDb", "Users", "/partitionKey", ct);
    }
}
```

### Add an Item

```csharp
var user = new UserEntity { Id = "user-1", TenantId = "tenant-a", Name = "Alice" };
var created = await cosmos.AddItemAsync("MyDb", "Users", user, ct);
```

### Get a Single Item

```csharp
var user = await cosmos.GetItemAsync<UserEntity>("MyDb", "Users", "user-1", "tenant-a", ct);
```

### Update an Item

```csharp
user.Name = "Alice Smith";
var updated = await cosmos.UpdateItemAsync("MyDb", "Users", user, ct);
```

### Delete an Item

```csharp
await cosmos.DeleteItemAsync("MyDb", "Users", user, ct);
```

### Query Items (String)

```csharp
var users = await cosmos.GetItemsAsync<UserEntity>(
    "MyDb", "Users",
    "SELECT * FROM c WHERE c.tenantId = 'tenant-a'",
    ct
);
```

### Query Items (Parameterized — Recommended)

Use `QueryDefinition` to prevent injection:

```csharp
var query = new QueryDefinition("SELECT * FROM c WHERE c.tenantId = @tenantId")
    .WithParameter("@tenantId", tenantId);

var users = await cosmos.GetItemsAsync<UserEntity>(
    "MyDb", "Users", query, ct
);
```

You can also pass `QueryRequestOptions` for partition key hints, max item count, etc.:

```csharp
var options = new QueryRequestOptions { PartitionKey = new PartitionKey(tenantId) };
var users = await cosmos.GetItemsAsync<UserEntity>("MyDb", "Users", query, ct, options);
```

## Interface Reference

```csharp
public interface IAzureCosmosDb<T> where T : IAzureCosmosDbConfiguration {

    Task<bool> CreateDatabaseAsync(string database, CancellationToken ct);

    Task<bool> CreateContainerAsync(
        string database, string container, string partitionKey, CancellationToken ct);

    Task<I> GetItemAsync<I>(
        string database, string container, string itemId, string partitionKey, CancellationToken ct)
        where I : ICosmosEntity;

    Task<IEnumerable<I>> GetItemsAsync<I>(
        string database, string container, string query, CancellationToken ct)
        where I : ICosmosEntity;

    Task<IEnumerable<I>> GetItemsAsync<I>(
        string database, string container, QueryDefinition queryDefinition, CancellationToken ct,
        QueryRequestOptions? requestOptions = null)
        where I : ICosmosEntity;

    Task<I> AddItemAsync<I>(
        string database, string container, I item, CancellationToken ct)
        where I : ICosmosEntity;

    Task<I> UpdateItemAsync<I>(
        string database, string container, I item, CancellationToken ct)
        where I : ICosmosEntity;

    Task DeleteItemAsync<I>(
        string database, string container, I item, CancellationToken ct)
        where I : ICosmosEntity;
}
```

## Notes

- A single `CosmosClient` is created per service instance and reused across all databases.
- The client is properly disposed when the DI container is disposed.
- All operations are logged via `IJsonLogger` with latency tracking.
