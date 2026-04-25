# Credentials

KServerTools supports multiple credential types for authenticating to Azure services.

## Credential Types

| Interface | Use Case | Backed By |
|-----------|----------|-----------|
| `IDefaultCredential` | Default Azure identity | `DefaultAzureCredential` |
| `IServicePrincipalCredential<T>` | Service principal (app registration) | Client ID + secret |

Both implement `ITokenCredentialService`:

```csharp
public interface ITokenCredentialService {
    AccessToken GetToken(TokenRequestContext requestContext, CancellationToken ct);
    ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken ct);
    Task<TokenCredential> GetCredential(CancellationToken ct);
}
```

## Default Credential

Registered automatically by `AddCommon()`. Uses Azure's `DefaultAzureCredential` chain (managed identity → environment → Azure CLI → etc.):

```csharp
services.AddKServerTools(kst => kst
    .AddCommon()   // registers IDefaultCredential
    .AddBlobStorage<StorageConfig>("Storage")  // uses IDefaultCredential
);
```

## Service Principal Credential

For scenarios where you need a specific app registration (multi-tenant, cross-subscription, etc.):

### Registration

```csharp
services.AddKServerTools(kst => kst
    .AddCommon()
    .AddSecretResolver()
    .AddKeyVault<AkvConfig>("AzureKeyVaultConfiguration")
    .AddServicePrincipal<MySpConfig>("ServicePrincipal")
);
```

### Configuration

```json
{
  "ServicePrincipal": {
    "TenantId": "00000000-0000-0000-0000-000000000000",
    "ApplicationId": "11111111-1111-1111-1111-111111111111",
    "SecretData": "akv://MySpClientSecret"
  }
}
```

The `SecretData` field supports the `akv://` scheme for Key Vault resolution. See [Key Vault & Secrets](key-vault.md).

### Config Class

```csharp
public class MySpConfig : ServicePrincipalConfiguration { }
```

`ServicePrincipalConfiguration` implements `IServicePrincipalConfig`:

```csharp
public interface IServicePrincipalConfig : ICredentialConfig {
    string ApplicationId { get; set; }
    string TenantId { get; set; }
    string SecretData { get; set; }
    Task<string> GetSecret(CancellationToken ct);
}
```

### Using a Service Principal Per Service

```csharp
services.AddKServerTools(kst => kst
    .AddCommon()
    .AddServicePrincipal<MySpConfig>("ServicePrincipal")
    // Use the SP credential for Cosmos, but default credential for storage
    .AddBlobStorage<StorageConfig>("Storage")
    .AddCosmosDb<CosmosConfig, IServicePrincipalCredential<MySpConfig>>("Cosmos")
);
```

## Multi-Tenant Cache Isolation

Cache keys are scoped by credential type to prevent cross-tenant data leakage:

```
{credentialTypeName}:{servicePrefix}:{resourceIdentifiers}
```

For example, two different service principals accessing the same Cosmos database will have separate cache entries.

## API Compatibility Notes

The enum `ServiceCredentalType` contains a spelling error (`Credential` → `Credental`). This is preserved for backward compatibility:

```csharp
public enum ServiceCredentalType {
    Certificate,
    DefaultCredential,
    ServicePrincipal,
}
```
