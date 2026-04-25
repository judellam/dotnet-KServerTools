namespace KServerTools.Common;

/// <summary>
/// Configuration for an Azure SQL Server database.
/// </summary>
/// <remarks>
/// Required package: Microsoft.Data.SqlClient.
/// </remarks>
public interface ISqlServerDatabaseConfiguration {
    /// <summary>
    /// Gets the server hostname or address.
    /// </summary>
    public string Server { get; }

    /// <summary>
    /// Gets the database name.
    /// </summary>
    public string Database { get; }

    /// <summary>
    /// Gets the authentication scopes used for Azure AD authentication.
    /// </summary>
    public string[] Scopes { get; }

    /// <summary>
    /// Gets the raw connection string data, if available.
    /// </summary>
    public string? ConnectionStringData { get; }

    /// <summary>
    /// Gets the resolved connection string, potentially resolving secrets.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The resolved connection string, or <see langword="null"/> if unavailable.</returns>
    Task<string?> GetConnectionString(CancellationToken cancellationToken);
}
