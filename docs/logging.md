# Logging

KServerTools provides structured JSON logging with automatic latency capture and caller information.

## Logging Options

| Option | Builder Method | Description |
|--------|---------------|-------------|
| `ILogger<T>` Adapter | `AddILogger<T>()` | Bridges to ASP.NET Core `ILogger<T>` (recommended) |
| Console JSON Logger | `AddLogger()` | Custom JSON format to stdout |
| Storage Logger | `AddStorageLogger<T>(section)` | Writes logs to Azure Blob Storage |

## Registration

### ILogger Adapter (Recommended)

Integrates with the standard ASP.NET Core logging pipeline:

```csharp
services.AddKServerTools(kst => kst
    .AddCommon()
    .AddILogger<Program>()
);
```

This registers `IJsonLogger` as an adapter around `ILogger<Program>`, so logs flow through your existing providers (console, Application Insights, Serilog, etc.).

### Console JSON Logger

Writes structured JSON events directly to the console:

```csharp
services.AddKServerTools(kst => kst
    .AddCommon()
    .AddRequestContext<RequestContext>()  // Required for console logger
    .AddLogger()
);
```

> **Prerequisite:** `AddLogger()` requires `AddRequestContext<T>()` to be called first.

### Storage Logger

Writes logs to Azure Blob Storage:

```csharp
services.AddKServerTools(kst => kst
    .AddCommon()
    .AddStorageLogger<LogStorageConfig>("LogStorage")
);
```

```json
{
  "LogStorage": {
    "ContainerName": "app-logs",
    "AccountName": "mystorageaccount",
    "Endpoint": "blob.core.windows.net"
  }
}
```

## IJsonLogger Interface

All KServerTools services log through `IJsonLogger`. Every method automatically captures:

- **Caller file path** — which source file produced the log
- **Line number** — exact line in the source
- **Member name** — method that produced the log
- **Latency** — optional duration in milliseconds

```csharp
public interface IJsonLogger {
    void Info(string message, long? latency = null, ...);
    void Warn(string message, Exception? exception = null, long? latency = null, ...);
    void Error(string message, Exception exception, long? latency = null, ...);
    void IfInfo(bool condition, string message, long? latency = null, ...);
    void IfError(bool condition, string message, Exception exception, long? latency = null, ...);
}
```

> The `...` parameters are `[CallerFilePath]`, `[CallerLineNumber]`, and `[CallerMemberName]` — automatically captured by the compiler.

## Usage

### Basic Logging

```csharp
public class OrderService(IJsonLogger logger) {

    public async Task ProcessAsync(string orderId, CancellationToken ct) {
        logger.Info($"Processing order {orderId}");
        var sw = Stopwatch.StartNew();

        // ... do work ...

        logger.Info($"Order {orderId} processed", latency: sw.ElapsedMilliseconds);
    }
}
```

### Conditional Logging

Avoid string formatting when a log level is disabled:

```csharp
logger.IfInfo(expensiveList.Count > 100, $"Large result set: {expensiveList.Count} items");
```

### Error Logging

```csharp
try {
    await DoWorkAsync();
} catch (Exception ex) {
    logger.Error("Failed to process order", ex, latency: sw.ElapsedMilliseconds);
    throw;
}
```

## Built-in Logging

All KServerTools Azure service methods log automatically via `LoggedOperationAsync`:

- Operation name and duration on success
- Full exception details on failure
- Cache hits/misses for cached operations

No additional logging code is needed in most cases.
