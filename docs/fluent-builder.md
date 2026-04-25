# Fluent Builder — `KSTBuilder`

The `KSTBuilder` provides a chainable API for registering all KServerTools services in a single block.

## Entry Point

```csharp
services.AddKServerTools(kst => kst
    .AddCommon()
    .AddBlobStorage<MyStorageConfig>("AzureStorage")
    // ... more services
);
```

## Credential Strategy

### Default Credential (Recommended)

`AddCommon()` registers `IDefaultCredential`, which wraps `DefaultAzureCredential`. All subsequent `Add*` calls use it automatically:

```csharp
kst.AddCommon()                                    // registers IDefaultCredential
   .AddBlobStorage<StorageConfig>("AzureStorage")  // uses IDefaultCredential
   .AddCosmosDb<CosmosConfig>("CosmosDb")          // uses IDefaultCredential
```

### Explicit Default Credential

Use `UseCredential<C>()` to change the default for all subsequent calls:

```csharp
kst.UseCredential<IServicePrincipalCredential<MySp>>()
   .AddBlobStorage<StorageConfig>("AzureStorage")  // uses the SP credential
```

### Per-Service Credential

Use the two-type-parameter overload to target a specific service:

```csharp
kst.AddCommon()
   .AddBlobStorage<StorageConfig>("AzureStorage")                            // IDefaultCredential
   .AddCosmosDb<CosmosConfig, IServicePrincipalCredential<MySp>>("Cosmos")   // SP credential
```

## API Reference

### Common & Infrastructure

| Method | Config Auto-Bind | Credential Required | Description |
|--------|:---:|:---:|-------------|
| `AddCommon()` | — | — | `IDefaultCredential`, `ConfigurationHelper`, `IMemoryCache` |
| `AddRequestContext<T>()` | — | — | `IRequestContext`, `IRequestContextAccessor`, `IHttpContextAccessor` |
| `AddSecretResolver()` | — | — | `ISecretResolver` for `akv://` scheme |
| `UseCredential<C>()` | — | — | Sets the default credential type |

### Azure Services

| Method | Config Auto-Bind | Credential Required | Registers |
|--------|:---:|:---:|-----------|
| `AddKeyVault<T>(section)` | ✅ | ✅ | `IAzureKeyVaultService<T>` |
| `AddBlobStorage<T>(section)` | ✅ | ✅ | `IAzureStorageService<T>`, `IAzureBlobManagementService<T>` |
| `AddQueue<T>(section)` | ✅ | ✅ | `IAzureStorageQueueService<T>`, `IAzureQueueManagementService<T>` |
| `AddCosmosDb<T>(section)` | ✅ | ✅ | `IAzureCosmosDb<T>` |
| `AddSql<T>()` | — | ✅ | `ISqlServerService<T>` (token auth) |
| `AddSqlConnectionString<T>()` | — | — | `ISqlServerService<T>` (connection string) |

> **Config Auto-Bind**: When ✅, the builder calls `AddConfigSection<T>(sectionName)` to bind `appsettings.json` → singleton `T`. When —, you must register the config yourself.

### Logging

| Method | Config Auto-Bind | Description |
|--------|:---:|-------------|
| `AddLogger()` | — | Console `JsonLogger` (requires `AddRequestContext<T>()`) |
| `AddILogger<T>()` | — | `IJsonLogger` via `ILogger<T>` adapter |
| `AddStorageLogger<T>(section)` | ✅ | `JsonStorageLogger` backed by blob storage |

### Credentials

| Method | Config Auto-Bind | Description |
|--------|:---:|-------------|
| `AddServicePrincipal<T>(section)` | ✅ | `IServicePrincipalCredential<T>` |

## Full Example

```csharp
builder.Services.AddKServerTools(kst => kst
    .AddCommon()
    .AddRequestContext<RequestContext>()
    .AddSecretResolver()
    .AddKeyVault<AkvConfig>("AzureKeyVaultConfiguration")
    .AddBlobStorage<StorageConfig>("AzureStorage")
    .AddQueue<QueueConfig>("AzureQueue")
    .AddCosmosDb<CosmosConfig>("CosmosDb")
    .AddSql<SqlConfig>()
    .AddILogger<Program>()
    .AddServicePrincipal<SpConfig>("ServicePrincipal")
);
```

## Idempotency

`AddCommon()` is idempotent — calling it multiple times is safe. Other `Add*` methods that require common services will call `EnsureCommon()` internally if `AddCommon()` was not yet called.

## DI Prerequisites

Some services depend on others being registered:

| Service | Requires |
|---------|----------|
| `AddLogger()` (console) | `AddRequestContext<T>()` |
| `AddStorageLogger<T>(...)` | `AddCommon()` (or auto via `EnsureCommon`) |
| Secret resolution (`akv://`) | `AddSecretResolver()` + `AddKeyVault<T>(...)` + explicit `RegisterKeyVaultService()` call |

See [Key Vault & Secrets](key-vault.md) for the full secret resolution wiring example.
