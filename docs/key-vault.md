# Key Vault & Secret Resolution

KServerTools provides Azure Key Vault access and a pluggable secret resolution system using the `akv://` URI scheme.

## Registration

```csharp
services.AddKServerTools(kst => kst
    .AddCommon()
    .AddSecretResolver()
    .AddKeyVault<MyAkvConfig>("AzureKeyVaultConfiguration")
);
```

## Configuration

```json
{
  "AzureKeyVaultConfiguration": {
    "Uri": "https://my-vault.vault.azure.net/",
    "CacheDurationInSeconds": 300
  }
}
```

```csharp
public class MyAkvConfig : IAzureKeyVaultConfiguration {
    public string Uri { get; set; } = "";
    public int CacheDurationInSeconds { get; set; } = 300;
}
```

## Usage — Key Vault Service

### Retrieve a Secret

```csharp
public class MyService(IAzureKeyVaultService<MyAkvConfig> keyVault) {

    public async Task<string> GetApiKeyAsync(CancellationToken ct) {
        return await keyVault.GetSecretAsync("MyApiKey", ct);
    }
}
```

### Retrieve a Certificate

```csharp
X509Certificate2 cert = await keyVault.GetCertificate("MyCert", ct);
```

Secrets are cached in-memory for the duration specified by `CacheDurationInSeconds`.

## Secret Resolution (`akv://` Scheme)

The `ISecretResolver` resolves secrets from different sources based on URI scheme. Currently supported:

| Scheme | Source | Example |
|--------|--------|---------|
| `akv://` | Azure Key Vault | `akv://MyDatabasePassword` |
| *(plain string)* | Returned as-is | `my-literal-value` |

### Wiring the Secret Resolver

The secret resolver requires explicit wiring to connect it to a Key Vault service. This is typically done in your DI registration:

```csharp
services.AddKServerTools(kst => kst
    .AddCommon()
    .AddSecretResolver()
    .AddKeyVault<MyAkvConfig>("AzureKeyVaultConfiguration")
    .AddServicePrincipal<MySpConfig>("ServicePrincipal")
);

// Wire the secret resolver to the Key Vault service
services.AddSingleton<MySpConfig>(sp => {
    var config = sp.GetRequiredService<ConfigurationHelper>()
        .TryGet<MySpConfig>() ?? throw new InvalidOperationException("Missing SP config");

    // Connect the secret resolver to Key Vault
    var resolver = sp.GetRequiredService<ISecretResolver>();
    var akv = sp.GetRequiredService<IAzureKeyVaultService<MyAkvConfig>>();
    resolver.RegisterKeyVaultService(akv);

    config.SecretResolver = resolver;
    return config;
});
```

### Using Secret Resolution in Config

```json
{
  "ServicePrincipal": {
    "TenantId": "my-tenant-id",
    "ApplicationId": "my-app-id",
    "SecretData": "akv://SpClientSecret"
  }
}
```

When `SecretData` is `akv://SpClientSecret`, calling `GetSecret()` on the config will resolve it through the secret resolver, which fetches the `SpClientSecret` secret from Key Vault.

## Interface Reference

### `IAzureKeyVaultService<T>`

```csharp
public interface IAzureKeyVaultService<T> : IAzureKeyVaultInternal
    where T : IAzureKeyVaultConfiguration {
}

public interface IAzureKeyVaultInternal {
    Task<string> GetSecretAsync(string secretName, CancellationToken ct);
    Task<X509Certificate2> GetCertificate(string certificateName, CancellationToken ct);
}
```

### `ISecretResolver`

```csharp
public interface ISecretResolver {
    ValueTask<string> Resolve(string secret, CancellationToken ct);
    void RegisterKeyVaultService(IAzureKeyVaultInternal keyVaultService);
}
```

## Notes

- `RegisterKeyVaultService()` is immutable after first call — subsequent calls are ignored. This prevents accidental re-binding.
- Secret names in `akv://` URIs are case-insensitive (the URI host is lowercased automatically).
- Plain strings (no `akv://` prefix) pass through the resolver unchanged.
