# SQL Server

KServerTools supports two authentication modes for Azure SQL / SQL Server:

| Mode | Builder Method | Description |
|------|---------------|-------------|
| Token Auth | `AddSql<T>()` | Azure AD token via credential (recommended for Azure SQL) |
| Connection String | `AddSqlConnectionString<T>()` | Traditional connection string with embedded credentials |

## Registration

### Token Auth (Recommended)

```csharp
services.AddKServerTools(kst => kst
    .AddCommon()
    .AddSql<MySqlConfig>()
);
```

### Connection String Auth

```csharp
services.AddKServerTools(kst => kst
    .AddSqlConnectionString<MySqlConfig>()
);
```

> **Note:** SQL config is **not** auto-bound from `appsettings.json`. You must register the config yourself.

## Configuration

```json
{
  "MySqlConfig": {
    "Server": "tcp:myserver.database.windows.net,1433",
    "Database": "MyDb",
    "Scopes": ["https://database.windows.net/.default"],
    "ConnectionStringData": "Server=tcp:myserver.database.windows.net,1433;Initial Catalog=MyDb;Persist Security Info=False;User ID=myuser;Password=mypassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

```csharp
public class MySqlConfig : ISqlServerDatabaseConfiguration {
    public string Server { get; set; } = "";
    public string Database { get; set; } = "";
    public string[] Scopes { get; set; } = [];
    public string? ConnectionStringData { get; set; }

    public Task<string?> GetConnectionString(CancellationToken ct) =>
        Task.FromResult(ConnectionStringData);
}
```

Register the config in DI:

```csharp
services.AddSingleton(sp => {
    var helper = sp.GetRequiredService<ConfigurationHelper>();
    return helper.TryGet<MySqlConfig>() ?? throw new InvalidOperationException("Missing MySqlConfig");
});
```

## Usage

### Execute a Query

```csharp
public class UserRepository(ISqlServerService<MySqlConfig> sql) {

    public async Task<User?> GetUserAsync(string userId, CancellationToken ct) {
        var parameters = new List<SqlParameter> {
            new("@UserId", userId)
        };

        return await sql.QueryAsync(
            "SELECT Id, Name, Email FROM Users WHERE Id = @UserId",
            parameters,
            async reader => {
                if (await reader.ReadAsync(ct)) {
                    return new User {
                        Id = reader.GetString(0),
                        Name = reader.GetString(1),
                        Email = reader.GetString(2)
                    };
                }
                return null;
            },
            ct
        );
    }
}
```

### Execute a Non-Query

```csharp
int rowsAffected = await sql.NonQueryAsync(
    "UPDATE Users SET Name = @Name WHERE Id = @UserId",
    new List<SqlParameter> {
        new("@Name", "Alice Smith"),
        new("@UserId", "user-1")
    },
    ct
);
```

### Get a Raw Connection

For advanced scenarios (transactions, bulk operations):

```csharp
using var connection = await sql.GetOrCreateConnection(ct);
// Use connection directly
```

## Interface Reference

```csharp
public interface ISqlServerService<T> where T : ISqlServerDatabaseConfiguration {

    Task<int> NonQueryAsync(
        string query, IList<SqlParameter>? parameters, CancellationToken ct);

    Task<M> QueryAsync<M>(
        string query, IList<SqlParameter>? parameters,
        Func<SqlDataReader, Task<M>> onRead, CancellationToken ct);

    Task<SqlConnection> GetOrCreateConnection(CancellationToken ct);
}
```

## Configuration Interface

```csharp
public interface ISqlServerDatabaseConfiguration {
    string Server { get; }
    string Database { get; }
    string[] Scopes { get; }
    string? ConnectionStringData { get; }
    Task<string?> GetConnectionString(CancellationToken ct);
}
```

## Security

- **Token auth** connections use `Encrypt=True` by default.
- **Connection string** auth: ensure your connection string includes `Encrypt=True;TrustServerCertificate=False` for production. The library enforces `Encrypt=True` even if omitted.
- Always use parameterized queries (`SqlParameter`) to prevent SQL injection.
