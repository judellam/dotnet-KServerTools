# Getting Started

## Prerequisites

- **.NET 9.0** or later
- An Azure subscription (for Azure services)
- Appropriate Azure RBAC roles for the services you plan to use

## Installation

```bash
dotnet add package KServerTools
```

## Namespace

All public types live under a single namespace:

```csharp
using KServerTools.Common;
```

## Minimal Setup

The fastest way to register services is the **fluent builder**:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddKServerTools(kst => kst
    .AddCommon()                                          // IDefaultCredential, ConfigurationHelper, IMemoryCache
    .AddRequestContext<RequestContext>()                   // IRequestContext, IRequestContextAccessor
    .AddILogger<Program>()                                // IJsonLogger via ILogger<Program>
    .AddBlobStorage<MyStorageConfig>("AzureStorage")      // IAzureStorageService<T>, IAzureBlobManagementService<T>
);

var app = builder.Build();
app.Run();
```

> **Note:** `AddCommon()` registers `IDefaultCredential` (backed by `DefaultAzureCredential`). All `Add*` methods that need a credential will use it automatically. To use a different credential, call `UseCredential<C>()` or use the two-type-parameter overload (e.g., `AddBlobStorage<T, C>(...)`).

## Configuration

Each Azure service reads its configuration from `appsettings.json`. The section name is the string you pass to the builder method:

```json
{
  "AzureStorage": {
    "AccountName": "mystorageaccount",
    "Endpoint": "blob.core.windows.net"
  }
}
```

Create a configuration class that implements the matching interface:

```csharp
public class MyStorageConfig : IAzureStorageServiceConfig {
    public string AccountName { get; set; } = "";
    public string Endpoint { get; set; } = "";
}
```

The builder auto-registers the config from `appsettings.json` via `IOptions<T>` binding.

## Legacy Registration

If you prefer registering services individually (without the fluent builder), use the `KSTAdd*` extension methods directly:

```csharp
services
    .KSTAddCommon()
    .KSTAddRequestContext<RequestContext>()
    .KSTAddLogger()
    .KSTAddAzureStorageService<MyStorageConfig, IDefaultCredential>("AzureStorage");
```

Both approaches produce identical DI registrations. See the [Fluent Builder](fluent-builder.md) guide for the full API.

## Example Repository

A full working example is available at:
[github.com/judellam/dotnet-KServerTools-example](https://github.com/judellam/dotnet-KServerTools-example)

## Next Steps

- [Fluent Builder](fluent-builder.md) — learn the full builder API
- [Blob Storage](blob-storage.md) / [Queue Storage](queue-storage.md) / [Cosmos DB](cosmos-db.md) / [SQL Server](sql-server.md) — service-specific guides
- [Credentials](credentials.md) — multi-tenant and service principal setup
- [Security](security.md) — security practices baked into the library
