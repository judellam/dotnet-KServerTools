namespace KServerTools.Common;

using Microsoft.Data.SqlClient;

/// <summary>
/// Service for interacting with a SQL Server database.
/// todo: Remove the service principal credential if not needed.
/// </summary>
/// <typeparam name="T">The configuration for the database.</typeparam>
/// <param name="config"></param>
/// <param name="logger"></param>
/// <param name="servicePrincipalCredential"></param>
/// <remarks>
/// Required package: Microsoft.Data.SqlClient
/// Example configuration found in appsettings.json:
///   "UserDatabaseSqlServerConfiguration": {
///     "ConnectionStringDatax": "Server={{server}};Connection Timeout=30;",
///     "Server": "{{name}}.database.windows.net",
///     "Database": "{{database name}}",
///     "Scopes": [
///       "https://database.windows.net/.default"
///     ]
///   }
/// </remarks>
internal class SqlServerService<T>(
    T config,
    IJsonLogger logger, 
    IServicePrincipalCredential<ServicePrincipalConfiguration> servicePrincipalCredential) 
        : ISqlServerService<T> where T : ISqlServerDatabaseConfiguration {
    private readonly T config = config;
    private readonly IJsonLogger logger = logger;
    private readonly IServicePrincipalCredential<ServicePrincipalConfiguration>? servicePrincipalCredential = servicePrincipalCredential;

    public async Task<int> NonQueryAsync(string query, IList<SqlParameter>? parameters, CancellationToken cancellationToken) {
        InternalServerErrorException.ThrowIfArgumentIsNull(query, nameof(query));
        cancellationToken.ThrowIfCancellationRequested();

        using SqlConnection connection = await this.GetOrCreateConnection(cancellationToken);
        using SqlCommand command = new(query, connection);
        if (parameters is not null) {
            command.Parameters.AddRange([.. parameters]);
        }

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>
    /// Execute a query and return a SqlDataReader. The data reader must be disposed of.
    /// </summary>
    public async Task<M> QueryAsync<M>(string query, IList<SqlParameter>? parameters, Func<SqlDataReader, Task<M>> onRead, CancellationToken cancellationToken) {
        InternalServerErrorException.ThrowIfArgumentIsNull(query, nameof(query));
        cancellationToken.ThrowIfCancellationRequested();

        using SqlConnection connection = await this.GetOrCreateConnection(cancellationToken);
        using SqlCommand command = new(query, connection);
        if (parameters is not null) {
            command.Parameters.AddRange([.. parameters]);
        }

        using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        return await onRead(reader);
    }

    public virtual async Task<SqlConnection> GetOrCreateConnection(CancellationToken cancellationToken) {
        if (!string.IsNullOrEmpty(this.config.ConnectionStringData)) {
            return new SqlConnection(await this.config.GetConnectionString(cancellationToken));
        }

        InternalServerErrorException.ThrowIfArgumentIsNull(
            this.servicePrincipalCredential,
            nameof(this.servicePrincipalCredential));

        string accessToken = await this.servicePrincipalCredential!
            .GetAccessToken(this.config.Scopes, cancellationToken)
            .ConfigureAwait(false);

        SqlConnectionStringBuilder sqlConnectionStringBuilder = new() {
            DataSource = this.config.Server,
            InitialCatalog = this.config.Database,
            ConnectTimeout = 30,
            Pooling = true,
        };

        SqlConnection connection = new(sqlConnectionStringBuilder.ConnectionString) {
            AccessToken = accessToken
        };

        // Depending on SQL server, initial connection may fail incase if you have a WoL (wake on lan) type of Azure Sql Server.
        // This will retry making a connection a few times to allow the server to wake up if it's the first time it's used.
        // If you're not cheap and buy a more premium skew, this retry will not be needed. -- I'm cheap. :-)
        cancellationToken.ThrowIfCancellationRequested();
        await Retry.DoAsync(async () => 
            await connection.OpenAsync(cancellationToken),
            3,
            500);

        return connection;
    }
}

/// <summary>
/// When only a connection string is provided.
/// </summary>
internal class SqlServerConnstionString<T>(T config, IJsonLogger logger) 
    : SqlServerService<T>(config, logger, null!)
    where T : ISqlServerDatabaseConfiguration
{
}