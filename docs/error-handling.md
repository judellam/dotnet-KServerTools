# Error Handling

KServerTools provides a typed exception hierarchy and a retry helper with exponential backoff.

## ServiceException Hierarchy

All service-level exceptions inherit from `ServiceException`, which carries a `ServiceError` enum:

```
ServiceException
├── NotFoundException          (404)
├── BadRequestException        (400)
├── UnauthorizedException      (401)
├── ForbiddenException         (403)
├── ConflictException          (409)
├── NoResponseException        (444)
├── InternalServerErrorException (500)
└── UnauthorizedAccessException (401)
```

### ServiceError Enum

```csharp
public enum ServiceError {
    BadRequest         = 400,
    Unauthorized       = 401,
    Forbidden          = 403,
    NotFound           = 404,
    Conflict           = 409,
    NoResponse         = 444,
    InternalServerError = 500,
    ServiceUnavailable = 503,
    GatewayTimeout     = 504,
    Unknown            = -1
}
```

### Usage

```csharp
try {
    var user = await repository.GetAsync(lookup, ct);
} catch (NotFoundException) {
    return NotFound();
} catch (ServiceException ex) when (ex.ServiceError == ServiceError.Unauthorized) {
    return Unauthorized();
}
```

### Null-Check Helpers

```csharp
// Throws BadRequestException if null
var user = BadRequestException.ThrowIfArgumentIsNull(input, "user");

// Throws InternalServerErrorException if null
var config = InternalServerErrorException.ThrowIfArgumentIsNull(config, "Config missing");
```

## Retry Helper

The `Retry` class provides async retry with exponential backoff and decorrelated jitter.

### Basic Retry

```csharp
await Retry.DoAsync(async () => {
    await httpClient.PostAsync(url, content, ct);
});
// Retries up to 3 times with ~1s base delay
```

### Custom Parameters

```csharp
await Retry.DoAsync(
    action: async () => await SendAsync(ct),
    maxRetries: 5,
    delay: 2000  // 2s base delay
);
```

### With Return Value

```csharp
var result = await Retry.DoAsync(async () => {
    return await httpClient.GetStringAsync(url, ct);
});
```

### Selective Retry

Use `shouldRetry` to skip non-retryable exceptions:

```csharp
await Retry.DoAsync(
    action: async () => await CallExternalApi(ct),
    shouldRetry: ex => ex is not UnauthorizedException
);
```

Non-retryable exceptions are thrown immediately without waiting for remaining attempts.

### Backoff Algorithm

The retry uses **full jitter exponential backoff**:

```
delay = random(maxDelay / 2, maxDelay)
where maxDelay = baseDelay × 2^attempt
```

- Attempt 0: `random(500, 1000)` ms
- Attempt 1: `random(1000, 2000)` ms
- Attempt 2: `random(2000, 4000)` ms
- Capped at attempt 8 to prevent overflow

If all retries fail, an `AggregateException` is thrown containing all caught exceptions.

### API Reference

```csharp
public static class Retry {
    static Task DoAsync(
        Func<Task> action,
        int maxRetries = 3,
        int delay = 1000,
        Func<Exception, bool>? shouldRetry = null);

    static Task<T> DoAsync<T>(
        Func<Task<T>> action,
        int maxRetries = 3,
        int delay = 1000,
        Func<Exception, bool>? shouldRetry = null);
}
```
