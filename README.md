# KServerTools

A .NET 9 library that simplifies Azure service integration for ASP.NET Core applications. Register Azure Blob Storage, Queue Storage, Cosmos DB, SQL Server, and Key Vault with a single fluent builder — including credential management, caching, structured logging, and retry logic.

## Installation

```bash
dotnet add package KServerTools
```

Requires **.NET 9.0** or later.

## Quick Start

```csharp
using KServerTools.Common;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddKServerTools(kst => kst
    .AddCommon()                                        // IDefaultCredential, ConfigurationHelper, IMemoryCache
    .AddRequestContext<RequestContext>()                 // Request tracking
    .AddILogger<Program>()                              // IJsonLogger via ILogger<T>
    .AddBlobStorage<StorageConfig>("AzureStorage")      // Blob upload/download/list/delete
    .AddQueue<QueueConfig>("AzureQueue")                // Queue enqueue/dequeue/peek/batch
    .AddCosmosDb<CosmosConfig>("CosmosDb")              // Cosmos CRUD and queries
    .AddSql<SqlConfig>()                                // SQL Server with token auth
    .AddKeyVault<AkvConfig>("KeyVault")                 // Secret and certificate retrieval
);

var app = builder.Build();
app.Run();
```

```json
{
  "AzureStorage": {
    "AccountName": "mystorageaccount",
    "Endpoint": "blob.core.windows.net"
  },
  "AzureQueue": {
    "AccountName": "mystorageaccount",
    "Endpoint": "queue.core.windows.net"
  },
  "CosmosDb": {
    "EndpointUri": "https://myaccount.documents.azure.com:443/",
    "PrimaryKey": ""
  },
  "KeyVault": {
    "Uri": "https://my-vault.vault.azure.net/",
    "CacheDurationInSeconds": 300
  }
}
```

## Features

| Feature | Description | Docs |
|---------|-------------|------|
| **Fluent Builder** | Chain `Add*` calls to register all services in one block | [Guide](docs/fluent-builder.md) |
| **Blob Storage** | Upload, download, append, delete, list, check existence | [Guide](docs/blob-storage.md) |
| **Queue Storage** | Enqueue, dequeue, peek, batch, count, clear | [Guide](docs/queue-storage.md) |
| **Cosmos DB** | CRUD, string and parameterized queries | [Guide](docs/cosmos-db.md) |
| **SQL Server** | Token auth and connection string modes | [Guide](docs/sql-server.md) |
| **Key Vault** | Secret and certificate retrieval, `akv://` resolution | [Guide](docs/key-vault.md) |
| **Credentials** | Default credential, service principals, multi-tenant isolation | [Guide](docs/credentials.md) |
| **Logging** | `IJsonLogger` with latency, caller info; `ILogger<T>` adapter | [Guide](docs/logging.md) |
| **HTTP Client** | Base class with logging, headers, URL sanitization | [Guide](docs/http-client.md) |
| **Error Handling** | Typed exceptions, retry with exponential backoff and jitter | [Guide](docs/error-handling.md) |
| **Security** | Credential-scoped caching, encryption enforcement, URL sanitization | [Guide](docs/security.md) |

📖 **[Full documentation →](docs/README.md)**

## Service Registration at a Glance

Each `Add*` method registers one or more DI interfaces:

| Builder Method | Interfaces Registered |
|---------------|----------------------|
| `AddBlobStorage<T>(section)` | `IAzureStorageService<T>`, `IAzureBlobManagementService<T>` |
| `AddQueue<T>(section)` | `IAzureStorageQueueService<T>`, `IAzureQueueManagementService<T>` |
| `AddCosmosDb<T>(section)` | `IAzureCosmosDb<T>` |
| `AddSql<T>()` | `ISqlServerService<T>` |
| `AddSqlConnectionString<T>()` | `ISqlServerService<T>` |
| `AddKeyVault<T>(section)` | `IAzureKeyVaultService<T>` |

All services support an explicit credential overload: `AddBlobStorage<T, C>(section)` where `C` is an `ITokenCredentialService`.

## Service Principal with Key Vault Secrets

For multi-tenant or cross-subscription scenarios:

```csharp
builder.Services.AddKServerTools(kst => kst
    .AddCommon()
    .AddSecretResolver()
    .AddKeyVault<AkvConfig>("AzureKeyVaultConfiguration")
    .AddServicePrincipal<SpConfig>("ServicePrincipal")
    .AddSql<SqlConfig, IServicePrincipalCredential<SpConfig>>()
);
```

```json
{
  "AzureKeyVaultConfiguration": {
    "Uri": "https://my-vault.vault.azure.net/",
    "CacheDurationInSeconds": 300
  },
  "ServicePrincipal": {
    "TenantId": "00000000-0000-0000-0000-000000000000",
    "ApplicationId": "11111111-1111-1111-1111-111111111111",
    "SecretData": "akv://SpClientSecret"
  }
}
```

The `akv://SpClientSecret` reference is resolved at runtime through Key Vault. See the [Key Vault & Secrets guide](docs/key-vault.md) for full wiring details.

## Legacy Registration

If you prefer registering services individually without the fluent builder:

```csharp
services
    .KSTAddCommon()
    .KSTAddRequestContext<RequestContext>()
    .KSTAddLogger()
    .KSTAddAzureStorageService<StorageConfig, IDefaultCredential>("AzureStorage")
    .KSTAddAzureStorageQueue<QueueConfig, IDefaultCredential>("AzureQueue");
```

Both approaches produce identical DI registrations.

## Example Repository

A full working example is available at:
[github.com/judellam/dotnet-KServerTools-example](https://github.com/judellam/dotnet-KServerTools-example)

## API Compatibility Notes

The following public API names contain spelling errors that are preserved for backward compatibility:

| Identifier | Type | Intended Spelling |
|-----------|------|-------------------|
| `ServiceCredentalType` | Enum | `ServiceCredentialType` |
| `EnqueMessageAsync` | Method on `IAzureStorageQueueService<T>` | `EnqueueMessageAsync` |
| `DequeMessageAsync` | Method on `IAzureStorageQueueService<T>` | `DequeueMessageAsync` |

These will not be renamed in the current major version to avoid breaking existing consumers. Newer interfaces (`IAzureQueueManagementService<T>`) use correct spelling.

## Contributing

We welcome contributions! Please see our [contributing guidelines](CONTRIBUTING.md) for more information.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.