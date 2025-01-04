namespace KServerTools.Common;

/// <summary>
/// Configuration for an Azure SQL Server database.
/// </summary>
/// <remarks>
/// Required package: Microsoft.Data.SqlClient
/// </remarks>
public interface ISqlServerDatabaseConfiguration {
    public string Server { get; }
    public string Database { get; }
    public string[] Scopes { get; } // for azure authentication
    public string? ConnectionStringData {get;}
    Task<string?> GetConnectionString(CancellationToken cancellationToken);
}
