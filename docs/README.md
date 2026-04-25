# KServerTools Documentation

Deep-dive guides for each area of the library. Start with [Getting Started](getting-started.md) if you are new.

## Guides

| Guide | Description |
|-------|-------------|
| [Getting Started](getting-started.md) | Installation, prerequisites, minimal setup |
| [Fluent Builder](fluent-builder.md) | `KSTBuilder` API — register all services in one chain |
| [Blob Storage](blob-storage.md) | Upload, download, append, delete, list blobs |
| [Queue Storage](queue-storage.md) | Enqueue, dequeue, peek, batch, and manage queues |
| [Cosmos DB](cosmos-db.md) | CRUD operations, queries, `ICosmosEntity` |
| [SQL Server](sql-server.md) | Token auth vs connection string, queries |
| [Key Vault & Secrets](key-vault.md) | Secret retrieval, `akv://` resolution scheme |
| [Credentials](credentials.md) | Default credentials, service principals, multi-tenant |
| [Logging](logging.md) | `IJsonLogger`, `ILogger<T>` adapter, storage logger |
| [HTTP Client](http-client.md) | Build typed HTTP clients with `HttpClientBase` |
| [Error Handling](error-handling.md) | `ServiceException` hierarchy, retry with jitter |
| [Security](security.md) | Credential isolation, URL sanitization, encryption |
