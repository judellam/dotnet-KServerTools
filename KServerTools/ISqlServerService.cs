namespace KServerTools.Common;

using Microsoft.Data.SqlClient;

/// <summary>
/// Service for interacting with a SQL Server database.
/// </summary>
/// <typeparam name="T">The configuration that can resolve a secret.</typeparam>
/// <remarks>
/// Required package: Microsoft.Data.SqlClient
/// </remarks>
public interface ISqlServerService<T> where T : ISqlServerDatabaseConfiguration {
    Task<int> NonQueryAsync(string query, IList<SqlParameter>? parameters, CancellationToken cancellationToken);
    Task<M> QueryAsync<M>(string query, IList<SqlParameter>? parameters, Func<SqlDataReader, Task<M>> onRead, CancellationToken cancellationToken);
    Task<SqlConnection> GetOrCreateConnection(CancellationToken cancellationToken);
}
