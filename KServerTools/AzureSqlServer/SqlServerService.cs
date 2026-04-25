namespace KServerTools.Common;

using Azure.Core;
using Microsoft.Data.SqlClient;

/// <summary>
/// Service for interacting with a SQL Server database.
/// </summary>
/// <typeparam name="T">The configuration type for the database.</typeparam>
/// <typeparam name="C">The credential type used for token-based authentication.</typeparam>
/// <param name="config">The SQL Server database configuration.</param>
/// <param name="logger">The logger for structured JSON output.</param>
/// <param name="credential">The credential service used to obtain access tokens.</param>
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
///   }.
/// </remarks>
internal class SqlServerService<T, C>(T config, IJsonLogger logger, C credential) : ISqlServerService<T> where T : ISqlServerDatabaseConfiguration where C : class, ITokenCredentialService {
    private readonly T config = config;
    private readonly IJsonLogger logger = logger;
    private readonly C? credential = credential;

    /// <summary>
    /// Executes a non-query SQL command (INSERT, UPDATE, DELETE) and returns the number of rows affected.
    /// </summary>
    /// <param name="query">The SQL query to execute.</param>
    /// <param name="parameters">Optional SQL parameters for the query.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>The number of rows affected by the command.</returns>
    public async Task<int> NonQueryAsync(string query, IList<SqlParameter>? parameters, CancellationToken cancellationToken) {
        InternalServerErrorException.ThrowIfArgumentIsNull(query, nameof(query));
        cancellationToken.ThrowIfCancellationRequested();

        return await AzureServiceBaseHelpers.LoggedOperationAsync(this.logger, $"SQL NonQuery on {this.config.Database}", async () => {
            using SqlConnection connection = await this.GetOrCreateConnection(cancellationToken);
            using SqlCommand command = new(query, connection);
            if (parameters is not null) {
                command.Parameters.AddRange([.. parameters]);
            }

            return await command.ExecuteNonQueryAsync(cancellationToken);
        }, cancellationToken);
    }

    /// <summary>
    /// Executes a SQL query and processes the result set through a reader callback.
    /// </summary>
    /// <typeparam name="M">The return type produced by the reader callback.</typeparam>
    /// <param name="query">The SQL query to execute.</param>
    /// <param name="parameters">Optional SQL parameters for the query.</param>
    /// <param name="onRead">A callback that processes the <see cref="SqlDataReader"/> and produces a result.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>The result produced by the <paramref name="onRead"/> callback.</returns>
    public async Task<M> QueryAsync<M>(string query, IList<SqlParameter>? parameters, Func<SqlDataReader, Task<M>> onRead, CancellationToken cancellationToken) {
        InternalServerErrorException.ThrowIfArgumentIsNull(query, nameof(query));
        cancellationToken.ThrowIfCancellationRequested();

        return await AzureServiceBaseHelpers.LoggedOperationAsync(this.logger, $"SQL Query on {this.config.Database}", async () => {
            using SqlConnection connection = await this.GetOrCreateConnection(cancellationToken);
            using SqlCommand command = new(query, connection);
            if (parameters is not null) {
                command.Parameters.AddRange([.. parameters]);
            }

            using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
            return await onRead(reader);
        }, cancellationToken);
    }

    /// <summary>
    /// Gets or creates a SQL connection using either a connection string or token-based authentication.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>An open <see cref="SqlConnection"/>.</returns>
    public virtual async Task<SqlConnection> GetOrCreateConnection(CancellationToken cancellationToken) {
        if (!string.IsNullOrEmpty(this.config.ConnectionStringData)) {
            var connStringConnection = new SqlConnection(await this.config.GetConnectionString(cancellationToken));
            await connStringConnection.OpenAsync(cancellationToken).ConfigureAwait(false);
            return connStringConnection;
        }

        InternalServerErrorException.ThrowIfArgumentIsNull(
            this.credential,
            nameof(this.credential));

        AccessToken accessToken = await this.credential!
            .GetTokenAsync(new TokenRequestContext(this.config.Scopes), cancellationToken)
            .ConfigureAwait(false);

        SqlConnectionStringBuilder sqlConnectionStringBuilder = new() {
            DataSource = this.config.Server,
            InitialCatalog = this.config.Database,
            ConnectTimeout = 30,
            Pooling = true,
            Encrypt = true,
        };

        SqlConnection connection = new(sqlConnectionStringBuilder.ConnectionString) {
            AccessToken = accessToken.Token
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
