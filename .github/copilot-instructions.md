# Copilot Instructions for KServerTools

## Build Commands

```bash
# Restore and build
dotnet restore KServerTools
dotnet build KServerTools --no-restore

# Build the solution from root
dotnet build dotnet-KServerTools.sln
```

## Tests

```bash
dotnet test KServerTools.Tests
```

Test project uses xUnit + Moq. Tests cover: Retry, SecretResolver, ServiceExceptions, AzureServiceBase, KSTBuilder, ILoggerAdapter, ConfigurationHelper.

CI runs restore + build (`.github/workflows/dotnet.yml`).

## Architecture

KServerTools is a .NET 9 NuGet package (`KServerTools.Common` namespace) that provides reusable middleware for Kestrel-based servers interacting with Azure services. It is a library — there is no runnable application here.

### Generic Service Pattern

All Azure service wrappers follow a `Service<TConfig, TCredential>` pattern:
- `TConfig` — a configuration POCO bound from `appsettings.json` sections via `ConfigurationHelper.TryGet<T>()`
- `TCredential` — an `ITokenCredentialService` implementation (e.g., `IDefaultCredential`, `IServicePrincipalCredential<T>`)

Services: `AzureKeyVaultService<T,C>`, `SqlServerService<T,C>`, `AzureStorageService<T,C>`, `AzureCosmosDb<T,C>`, `AzureStorageQueueService<T,C>`.

### Dependency Injection via KST Extension Methods

All DI registration goes through `DependencyHelper.cs`. There are two approaches:

**Fluent Builder (preferred)** — set credential once, chain service registrations:

```csharp
services.AddKServerTools(kst => kst
    .UseCredential<IDefaultCredential>()
    .AddRequestContext<RequestContext>()
    .AddLogger()
    .AddKeyVault<MyKvConfig>("AzureKeyVaultConfiguration")
    .AddBlobStorage<MyBlobConfig>("BlobStorageConfig")
    .AddQueue<MyQueueConfig>("QueueConfig")
    .AddCosmosDb<MyCosmosConfig>("CosmosConfig")
    .AddSql<MySqlConfig>()
    .AddSecretResolver()
    .AddServicePrincipal<MySpConfig>(nameof(ServicePrincipalConfiguration))
);
```

Each `Add*` method uses the credential from `UseCredential<C>()`. Override per-service with `Add*<T, C>()` overloads.

**Individual methods** (still supported):

```csharp
services
    .KSTAddCommon()                    // ConfigurationHelper, DefaultCredential, MemoryCache
    .KSTAddRequestContext<T>()         // Request context with AsyncLocal scoping
    .KSTAddLogger()                    // Console JSON logger (or KSTAddLogger<T,C>() for storage-backed)
    .KSTAddKeyVault<T, C>()           // Azure Key Vault
    .KSTAddSqlService<T, C>()         // SQL Server with token auth
    .KSTAddSqlServiceConnectionString<T>()  // SQL Server with connection string
    .KSTAddAzureStorageService<T, C>() // Blob storage
    .KSTAddAzureStorageQueue<T, C>()   // Queue storage
    .KSTAddAzureCosmosDb<T, C>()       // Cosmos DB
    .KSTAddSecretResolver()            // Secret resolver (akv:// URI scheme)
    .KSTAddServicePrincipalCredentialWithConfig<T>() // Service Principal credential
```

### Secret Resolution

`ISecretResolver` routes secrets by URI scheme:
- `akv://secretName` → fetches from a registered `IAzureKeyVaultService`
- Plain string → returned as-is (local/development secrets)

Register the Key Vault service with the resolver post-DI via `RegisterKeyVaultService()`.

### Logging

Two logger implementations of `IJsonLogger`:
- `JsonLogger` — writes structured JSON to console via `ILogger<JsonLogger>`
- `JsonStorageLogger<T,C>` — batches logs in a `ConcurrentQueue` (max 1000) and flushes to Azure Blob Storage every 30 seconds as JSONL

All `IJsonLogger` methods capture caller info (`[CallerFilePath]`, `[CallerLineNumber]`, `[CallerMemberName]`) automatically. Azure service wrappers log every operation with `Stopwatch`-based latency tracking.

### Request Context

`RequestContextAccessor<T>` uses `AsyncLocal<T>` for per-request scoping. `IRequestContext` tracks `RequestId` (from headers or auto-generated GUID) and `UserAgent`.

## Conventions

- **KST prefix**: All public DI extension methods start with `KST` (e.g., `KSTAddCommon`).
- **Nullable reference types** and **implicit usings** are enabled.
- **Configuration classes** follow the naming pattern `{ServiceName}Configuration` and implement a corresponding `I{ServiceName}Configuration` interface. They bind to `appsettings.json` sections matching the class name.
- **Exception hierarchy**: Custom exceptions in `Exceptions/` map to HTTP status codes via `ServiceError` enum (e.g., `NotFoundException` → 404, `BadRequestException` → 400). Use `BadRequestException.ThrowIfArgumentIsNull()` for argument validation.
- **Memory caching**: Azure services inherit from `AzureServiceBase<TConfig>` which provides shared `IMemoryCache` (from DI) with namespaced cache keys and `GetOrCreateCachedAsync()` helper. Use `LoggedOperationAsync()` for Stopwatch + catch-log-rethrow instrumentation.
- **Repository pattern**: `IRepository<M, L>` defines CRUD with separate Model (`M`) and Lookup (`L`) types. Extend with `IGetMultiple<M, L>` for batch retrieval.
- **Retry utility**: `Retry.DoAsync()` provides exponential backoff (default 3 retries).
- **Known typo**: `IDefailtCredential.cs` and `ServiceCredentalType.cs` contain intentional(?) typos — maintain consistency with existing names.

## Security

- **Multi-tenant cache isolation**: `AzureServiceBase` automatically prefixes all cache keys with the credential type identity (`typeof(C).FullName`), preventing cross-tenant cache sharing when different credentials access the same resource names.
- **SQL connections**: Token-based SQL connections enforce `Encrypt=True`. Never use `TrustServerCertificate=True` in production connection strings.
- **Log sanitization**: `HttpClientBase` strips query strings from logged URLs to avoid leaking SAS tokens. When adding new logging, never log secret values, connection strings, or full URLs with query parameters.
- **Immutable secret resolver**: `SecretResolver.RegisterKeyVaultService()` can only be called once. Subsequent calls throw `InvalidOperationException` to prevent runtime mutation of the vault binding.
- **No auto-provisioning on reads**: Blob `GetContainerClient` and queue `GetQueueClient` only call `CreateIfNotExistsAsync` on write/enqueue paths, not on reads or deletes.
