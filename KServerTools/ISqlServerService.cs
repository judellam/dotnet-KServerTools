namespace KServerTools.Common;

using Microsoft.Data.SqlClient;

/// <summary>
/// Service for interacting with a SQL Server database.
/// </summary>
/// <typeparam name="T">The configuration that can resolve a secret.</typeparam>
/// <remarks>
/// Required package: Microsoft.Data.SqlClient.
/// </remarks>
public interface ISqlServerService<T> where T : ISqlServerDatabaseConfiguration {
    /// <summary>
    /// Executes a non-query SQL command and returns the number of rows affected.
    /// </summary>
    /// <param name="query">The SQL query to execute.</param>
    /// <param name="parameters">Optional list of SQL parameters.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The number of rows affected.</returns>
    Task<int> NonQueryAsync(string query, IList<SqlParameter>? parameters, CancellationToken cancellationToken);

    /// <summary>
    /// Executes a SQL query and processes the result using the provided reader delegate.
    /// </summary>
    /// <typeparam name="M">The type of the result to return.</typeparam>
    /// <param name="query">The SQL query to execute.</param>
    /// <param name="parameters">Optional list of SQL parameters.</param>
    /// <param name="onRead">A delegate that reads from the <see cref="SqlDataReader"/> and produces the result.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The result produced by the <paramref name="onRead"/> delegate.</returns>
    Task<M> QueryAsync<M>(string query, IList<SqlParameter>? parameters, Func<SqlDataReader, Task<M>> onRead, CancellationToken cancellationToken);

    /// <summary>
    /// Gets an existing SQL connection or creates a new one.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>An open <see cref="SqlConnection"/>.</returns>
    Task<SqlConnection> GetOrCreateConnection(CancellationToken cancellationToken);
}
