# HTTP Client

`HttpClientBase` is an abstract base class for building typed HTTP clients with automatic logging, latency tracking, and header management.

## Creating a Typed Client

```csharp
public class PaymentClient : HttpClientBase {
    public PaymentClient(IHttpClientFactory factory, IJsonLogger logger)
        : base(factory, logger) { }

    public override string GetClientName() => "PaymentService";

    public async Task<PaymentResult> ChargeAsync(ChargeRequest request, CancellationToken ct) {
        var body = JsonSerializer.Serialize(request);
        var headers = new List<(string key, string value)> {
            ("X-Idempotency-Key", request.IdempotencyKey)
        };

        var response = await Send("/api/v1/charges", HttpMethod.Post, headers, body, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<PaymentResult>(json)!;
    }
}
```

## Registration

Register with `IHttpClientFactory`:

```csharp
services.AddHttpClient("PaymentService", client => {
    client.BaseAddress = new Uri("https://api.payment-provider.com");
    client.Timeout = TimeSpan.FromSeconds(30);
});
services.AddSingleton<PaymentClient>();
```

## Features

### Automatic Latency Logging

Every `Send` call logs:
- HTTP method and path (without query strings for security)
- Response status code
- Duration in milliseconds
- Success/failure status

### URL Sanitization

Query strings (which may contain tokens or SAS signatures) are automatically stripped from log output. Only the path is logged:

```
GET /api/v1/charges — Status: 200, Success: True
```

### Header Management

Headers are applied per-request. If a header already exists, a warning is logged and the value is replaced:

```csharp
var headers = new List<(string key, string value)> {
    ("Authorization", "Bearer token123"),
    ("X-Request-Id", Guid.NewGuid().ToString())
};
```

### Post Helper

A convenience `Post` method wraps `Send` with `HttpMethod.Post`:

```csharp
var response = await Post("/api/data", headers, body, ct);
```

## Protected API

```csharp
public abstract class HttpClientBase {
    public HttpClientBase(IHttpClientFactory clientFactory, IJsonLogger logger);

    public abstract string GetClientName();

    protected Task<HttpResponseMessage> Post(
        string path, CustomHeaders headers, string body, CancellationToken ct);

    protected Task<HttpResponseMessage> Send(
        string path, HttpMethod httpMethod, CustomHeaders headers, string body, CancellationToken ct);
}

// CustomHeaders is an alias for:
using CustomHeaders = IList<(string key, string value)>;
```
