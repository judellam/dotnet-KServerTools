namespace KServerTools.Common;

/// <summary>
/// A SQL Server service variant that authenticates using only a connection string,
/// without requiring a token credential.
/// </summary>
/// <typeparam name="T">The configuration type for the database.</typeparam>
/// <param name="config">The SQL Server database configuration.</param>
/// <param name="logger">The logger for structured JSON output.</param>
internal class SqlServerConnstionString<T>(T config, IJsonLogger logger)
    : SqlServerService<T, ITokenCredentialService>(config, logger, null!)
    where T : ISqlServerDatabaseConfiguration {
}
