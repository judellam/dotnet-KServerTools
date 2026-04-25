# Security

KServerTools applies several security practices by default.

## Credential-Scoped Caching

All cached data (connections, secrets, clients) is keyed by credential type to prevent cross-tenant leakage in multi-tenant applications:

```
{credentialTypeName}:{servicePrefix}:{resourceIdentifiers}
```

Two different credentials accessing the same Azure resource produce separate cache entries.

## Secret Resolution

The `ISecretResolver` resolves `akv://` URIs to Key Vault secrets. Important security properties:

- **Immutable binding:** `RegisterKeyVaultService()` can only be called once. Subsequent calls are silently ignored, preventing secret source replacement at runtime.
- **No plaintext in config:** Use `akv://SecretName` in `appsettings.json` instead of raw secrets.
- **Case-insensitive:** `akv://` URIs normalize to lowercase automatically.

See [Key Vault & Secrets](key-vault.md) for setup.

## SQL Encryption

The library enforces `Encrypt=True` on all SQL Server connections, even if omitted from the connection string. This ensures TLS encryption for data in transit.

For production, always set `TrustServerCertificate=False` in your connection string to validate the server's TLS certificate.

## URL Sanitization

`HttpClientBase` strips query strings from log output to prevent leaking tokens, SAS signatures, or other sensitive URL parameters:

```
// Logged as:
GET /api/v1/data — Status: 200, Success: True

// NOT logged:
GET /api/v1/data?sig=abc123&token=xyz — Status: 200, Success: True
```

## Container Auto-Creation

Blob storage operations that write data (upload, append) will auto-create containers if they do not exist. Read operations (`DownloadBlobAsync`) do not auto-create to prevent:
- Masking configuration errors
- Creating unintended resources from typos

## Recommendations

### Azure RBAC

Use managed identities and Azure RBAC instead of connection strings where possible:

```csharp
kst.AddCommon()                        // DefaultAzureCredential
   .AddBlobStorage<Config>("Storage")  // Token-based auth to storage
```

### Secret Management

1. Never commit secrets to source control
2. Use `akv://` references in `appsettings.json`
3. Use Key Vault for all sensitive configuration values
4. Use separate Key Vaults per environment (dev, staging, production)

### Network Security

1. Enable Azure Private Endpoints for production workloads
2. Set `Encrypt=True;TrustServerCertificate=False` for SQL connections
3. Use HTTPS endpoints for all Azure services

### Parameterized Queries

Always use `SqlParameter` for SQL queries to prevent injection:

```csharp
// Good
await sql.QueryAsync(
    "SELECT * FROM Users WHERE Id = @Id",
    new List<SqlParameter> { new("@Id", userId) },
    reader => ..., ct);

// Bad — SQL injection risk
await sql.QueryAsync(
    $"SELECT * FROM Users WHERE Id = '{userId}'",
    null, reader => ..., ct);
```

### Cosmos DB Parameterized Queries

Use `QueryDefinition` instead of string interpolation:

```csharp
// Good
var query = new QueryDefinition("SELECT * FROM c WHERE c.tenant = @t")
    .WithParameter("@t", tenantId);

// Bad — injection risk
var query = $"SELECT * FROM c WHERE c.tenant = '{tenantId}'";
```
