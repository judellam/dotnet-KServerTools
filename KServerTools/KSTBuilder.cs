namespace KServerTools.Common;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Fluent builder for configuring KServerTools services.
/// Set a default credential once with UseCredential, then add services without repeating the credential type.
/// </summary>
public class KSTBuilder {
    internal IServiceCollection Services { get; }
    private Type? defaultCredentialType;
    private bool commonRegistered;

    internal KSTBuilder(IServiceCollection services) {
        this.Services = services;
    }

    /// <summary>
    /// Registers common services (ConfigurationHelper, DefaultCredential, MemoryCache).
    /// Called automatically by the first Add* method if not called explicitly.
    /// </summary>
    /// <returns></returns>
    public KSTBuilder AddCommon() {
        if (!this.commonRegistered) {
            this.Services.KSTAddCommon();
            this.commonRegistered = true;
        }

        return this;
    }

    /// <summary>
    /// Sets the default credential type used by all subsequent Add* calls that don't specify one.
    /// Also registers common services if not already registered.
    /// </summary>
    /// <returns></returns>
    public KSTBuilder UseCredential<C>() where C : class, ITokenCredentialService {
        this.defaultCredentialType = typeof(C);
        return this.AddCommon();
    }

    /// <summary>
    /// Registers Azure Key Vault using the default credential.
    /// </summary>
    /// <returns></returns>
    public KSTBuilder AddKeyVault<T>(string sectionName) where T : class, IAzureKeyVaultConfiguration {
        this.EnsureCommon();
        Type credType = this.ResolveCredential();
        // KSTAddKeyVault<T, C> — use reflection to call with the resolved credential type
        typeof(DependencyHelper)
            .GetMethod(nameof(DependencyHelper.KSTAddKeyVault))!
            .MakeGenericMethod(typeof(T), credType)
            .Invoke(null, [this.Services, sectionName]);
        return this;
    }

    /// <summary>
    /// Registers Azure Key Vault with an explicit credential type.
    /// </summary>
    /// <returns></returns>
    public KSTBuilder AddKeyVault<T, C>(string sectionName) where T : class, IAzureKeyVaultConfiguration where C : ITokenCredentialService {
        this.EnsureCommon();
        this.Services.KSTAddKeyVault<T, C>(sectionName);
        return this;
    }

    /// <summary>
    /// Registers Azure Blob Storage using the default credential.
    /// </summary>
    /// <returns></returns>
    public KSTBuilder AddBlobStorage<T>(string sectionName) where T : class, IAzureStorageServiceConfig {
        this.EnsureCommon();
        Type credType = this.ResolveCredential();
        typeof(DependencyHelper)
            .GetMethod(nameof(DependencyHelper.KSTAddAzureStorageService))!
            .MakeGenericMethod(typeof(T), credType)
            .Invoke(null, [this.Services, sectionName]);
        return this;
    }

    /// <summary>
    /// Registers Azure Blob Storage with an explicit credential type.
    /// </summary>
    /// <returns></returns>
    public KSTBuilder AddBlobStorage<T, C>(string sectionName) where T : class, IAzureStorageServiceConfig where C : class, ITokenCredentialService {
        this.EnsureCommon();
        this.Services.KSTAddAzureStorageService<T, C>(sectionName);
        return this;
    }

    /// <summary>
    /// Registers Azure Storage Queue using the default credential.
    /// </summary>
    /// <returns></returns>
    public KSTBuilder AddQueue<T>(string sectionName) where T : class, IAzureStorageServiceConfig {
        this.EnsureCommon();
        Type credType = this.ResolveCredential();
        typeof(DependencyHelper)
            .GetMethod(nameof(DependencyHelper.KSTAddAzureStorageQueue))!
            .MakeGenericMethod(typeof(T), credType)
            .Invoke(null, [this.Services, sectionName]);
        return this;
    }

    /// <summary>
    /// Registers Azure Storage Queue with an explicit credential type.
    /// </summary>
    /// <returns></returns>
    public KSTBuilder AddQueue<T, C>(string sectionName) where T : class, IAzureStorageServiceConfig where C : class, ITokenCredentialService {
        this.EnsureCommon();
        this.Services.KSTAddAzureStorageQueue<T, C>(sectionName);
        return this;
    }

    /// <summary>
    /// Registers Azure Cosmos DB using the default credential.
    /// </summary>
    /// <returns></returns>
    public KSTBuilder AddCosmosDb<T>(string sectionName) where T : class, IAzureCosmosDbConfiguration {
        this.EnsureCommon();
        Type credType = this.ResolveCredential();
        typeof(DependencyHelper)
            .GetMethod(nameof(DependencyHelper.KSTAddAzureCosmosDb))!
            .MakeGenericMethod(typeof(T), credType)
            .Invoke(null, [this.Services, sectionName]);
        return this;
    }

    /// <summary>
    /// Registers Azure Cosmos DB with an explicit credential type.
    /// </summary>
    /// <returns></returns>
    public KSTBuilder AddCosmosDb<T, C>(string sectionName) where T : class, IAzureCosmosDbConfiguration where C : class, ITokenCredentialService {
        this.EnsureCommon();
        this.Services.KSTAddAzureCosmosDb<T, C>(sectionName);
        return this;
    }

    /// <summary>
    /// Registers SQL Server with token-based auth using the default credential.
    /// </summary>
    /// <returns></returns>
    public KSTBuilder AddSql<T>() where T : ISqlServerDatabaseConfiguration {
        this.EnsureCommon();
        Type credType = this.ResolveCredential();
        typeof(DependencyHelper)
            .GetMethod(nameof(DependencyHelper.KSTAddSqlService))!
            .MakeGenericMethod(typeof(T), credType)
            .Invoke(null, [this.Services]);
        return this;
    }

    /// <summary>
    /// Registers SQL Server with token-based auth using an explicit credential type.
    /// </summary>
    /// <returns></returns>
    public KSTBuilder AddSql<T, C>() where T : ISqlServerDatabaseConfiguration where C : class, ITokenCredentialService {
        this.EnsureCommon();
        this.Services.KSTAddSqlService<T, C>();
        return this;
    }

    /// <summary>
    /// Registers SQL Server with connection string auth (no credential needed).
    /// </summary>
    /// <returns></returns>
    public KSTBuilder AddSqlConnectionString<T>() where T : ISqlServerDatabaseConfiguration {
        this.EnsureCommon();
        this.Services.KSTAddSqlServiceConnectionString<T>();
        return this;
    }

    /// <summary>
    /// Registers the console JSON logger.
    /// </summary>
    /// <returns></returns>
    public KSTBuilder AddLogger() {
        this.EnsureCommon();
        this.Services.KSTAddLogger();
        return this;
    }

    /// <summary>
    /// Registers IJsonLogger backed by Microsoft.Extensions.Logging.ILogger&lt;T&gt;.
    /// </summary>
    /// <returns></returns>
    public KSTBuilder AddILogger<T>() {
        this.EnsureCommon();
        this.Services.KSTAddLogger<T>();
        return this;
    }

    /// <summary>
    /// Registers a storage-backed logger using the default credential.
    /// </summary>
    /// <returns></returns>
    public KSTBuilder AddStorageLogger<T>(string sectionName) where T : AzureStorageServiceLogConfig {
        this.EnsureCommon();
        Type credType = this.ResolveCredential();
        typeof(DependencyHelper)
            .GetMethod(nameof(DependencyHelper.KSTAddLogger), [typeof(IServiceCollection), typeof(string)])!
            .MakeGenericMethod(typeof(T), credType)
            .Invoke(null, [this.Services, sectionName]);
        return this;
    }

    /// <summary>
    /// Registers a storage-backed logger with an explicit credential type.
    /// </summary>
    /// <returns></returns>
    public KSTBuilder AddStorageLogger<T, C>(string sectionName) where T : AzureStorageServiceLogConfig where C : class, ITokenCredentialService {
        this.EnsureCommon();
        this.Services.KSTAddLogger<T, C>(sectionName);
        return this;
    }

    /// <summary>
    /// Registers the request context with the specified type.
    /// </summary>
    /// <returns></returns>
    public KSTBuilder AddRequestContext<T>() where T : class, IRequestContext, new() {
        this.EnsureCommon();
        this.Services.KSTAddRequestContext<T>();
        return this;
    }

    /// <summary>
    /// Registers the secret resolver.
    /// </summary>
    /// <returns></returns>
    public KSTBuilder AddSecretResolver() {
        this.EnsureCommon();
        this.Services.KSTAddSecretResolver();
        return this;
    }

    /// <summary>
    /// Registers a service principal credential with configuration.
    /// </summary>
    /// <returns></returns>
    public KSTBuilder AddServicePrincipal<T>(string sectionName) where T : class, IServicePrincipalConfig {
        this.EnsureCommon();
        this.Services.KSTAddServicePrincipalCredentialWithConfig<T>(sectionName);
        return this;
    }

    private void EnsureCommon() {
        if (!this.commonRegistered) {
            this.AddCommon();
        }
    }

    private Type ResolveCredential() =>
        this.defaultCredentialType ?? throw new InvalidOperationException(
            "No default credential configured. Call UseCredential<C>() first, or use an Add* overload that specifies the credential type.");
}
