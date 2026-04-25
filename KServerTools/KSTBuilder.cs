namespace KServerTools.Common;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Fluent builder for configuring KServerTools services.
/// Set a default credential once with UseCredential, then add services without repeating the credential type.
/// </summary>
public class KSTBuilder {
    private Type? defaultCredentialType;
    private bool commonRegistered;

    /// <summary>
    /// Initializes a new instance of the <see cref="KSTBuilder"/> class.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    internal KSTBuilder(IServiceCollection services) {
        this.Services = services;
    }

    /// <summary>
    /// Gets the underlying service collection being configured.
    /// </summary>
    internal IServiceCollection Services { get; }

    /// <summary>
    /// Registers common services (ConfigurationHelper, DefaultCredential, MemoryCache).
    /// Called automatically by the first Add* method if not called explicitly.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
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
    /// <typeparam name="C">The credential type implementing <see cref="ITokenCredentialService"/>.</typeparam>
    /// <returns>The builder instance for chaining.</returns>
    public KSTBuilder UseCredential<C>() where C : class, ITokenCredentialService {
        this.defaultCredentialType = typeof(C);
        return this.AddCommon();
    }

    /// <summary>
    /// Registers Azure Key Vault using the default credential.
    /// </summary>
    /// <typeparam name="T">The Key Vault configuration type.</typeparam>
    /// <param name="sectionName">The configuration section name.</param>
    /// <returns>The builder instance for chaining.</returns>
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
    /// <typeparam name="T">The Key Vault configuration type.</typeparam>
    /// <typeparam name="C">The credential type.</typeparam>
    /// <param name="sectionName">The configuration section name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public KSTBuilder AddKeyVault<T, C>(string sectionName) where T : class, IAzureKeyVaultConfiguration where C : ITokenCredentialService {
        this.EnsureCommon();
        this.Services.KSTAddKeyVault<T, C>(sectionName);
        return this;
    }

    /// <summary>
    /// Registers Azure Blob Storage using the default credential.
    /// </summary>
    /// <typeparam name="T">The storage configuration type.</typeparam>
    /// <param name="sectionName">The configuration section name.</param>
    /// <returns>The builder instance for chaining.</returns>
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
    /// <typeparam name="T">The storage configuration type.</typeparam>
    /// <typeparam name="C">The credential type.</typeparam>
    /// <param name="sectionName">The configuration section name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public KSTBuilder AddBlobStorage<T, C>(string sectionName) where T : class, IAzureStorageServiceConfig where C : class, ITokenCredentialService {
        this.EnsureCommon();
        this.Services.KSTAddAzureStorageService<T, C>(sectionName);
        return this;
    }

    /// <summary>
    /// Registers Azure Storage Queue using the default credential.
    /// </summary>
    /// <typeparam name="T">The storage configuration type.</typeparam>
    /// <param name="sectionName">The configuration section name.</param>
    /// <returns>The builder instance for chaining.</returns>
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
    /// <typeparam name="T">The storage configuration type.</typeparam>
    /// <typeparam name="C">The credential type.</typeparam>
    /// <param name="sectionName">The configuration section name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public KSTBuilder AddQueue<T, C>(string sectionName) where T : class, IAzureStorageServiceConfig where C : class, ITokenCredentialService {
        this.EnsureCommon();
        this.Services.KSTAddAzureStorageQueue<T, C>(sectionName);
        return this;
    }

    /// <summary>
    /// Registers Azure Cosmos DB using the default credential.
    /// </summary>
    /// <typeparam name="T">The Cosmos DB configuration type.</typeparam>
    /// <param name="sectionName">The configuration section name.</param>
    /// <returns>The builder instance for chaining.</returns>
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
    /// <typeparam name="T">The Cosmos DB configuration type.</typeparam>
    /// <typeparam name="C">The credential type.</typeparam>
    /// <param name="sectionName">The configuration section name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public KSTBuilder AddCosmosDb<T, C>(string sectionName) where T : class, IAzureCosmosDbConfiguration where C : class, ITokenCredentialService {
        this.EnsureCommon();
        this.Services.KSTAddAzureCosmosDb<T, C>(sectionName);
        return this;
    }

    /// <summary>
    /// Registers SQL Server with token-based auth using the default credential.
    /// </summary>
    /// <typeparam name="T">The SQL Server configuration type.</typeparam>
    /// <returns>The builder instance for chaining.</returns>
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
    /// <typeparam name="T">The SQL Server configuration type.</typeparam>
    /// <typeparam name="C">The credential type.</typeparam>
    /// <returns>The builder instance for chaining.</returns>
    public KSTBuilder AddSql<T, C>() where T : ISqlServerDatabaseConfiguration where C : class, ITokenCredentialService {
        this.EnsureCommon();
        this.Services.KSTAddSqlService<T, C>();
        return this;
    }

    /// <summary>
    /// Registers SQL Server with connection string auth (no credential needed).
    /// </summary>
    /// <typeparam name="T">The SQL Server configuration type.</typeparam>
    /// <returns>The builder instance for chaining.</returns>
    public KSTBuilder AddSqlConnectionString<T>() where T : ISqlServerDatabaseConfiguration {
        this.EnsureCommon();
        this.Services.KSTAddSqlServiceConnectionString<T>();
        return this;
    }

    /// <summary>
    /// Registers the console JSON logger.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public KSTBuilder AddLogger() {
        this.EnsureCommon();
        this.Services.KSTAddLogger();
        return this;
    }

    /// <summary>
    /// Registers IJsonLogger backed by Microsoft.Extensions.Logging.ILogger&lt;T&gt;.
    /// </summary>
    /// <typeparam name="T">The category type for the underlying logger.</typeparam>
    /// <returns>The builder instance for chaining.</returns>
    public KSTBuilder AddILogger<T>() {
        this.EnsureCommon();
        this.Services.KSTAddLogger<T>();
        return this;
    }

    /// <summary>
    /// Registers a storage-backed logger using the default credential.
    /// </summary>
    /// <typeparam name="T">The storage log configuration type.</typeparam>
    /// <param name="sectionName">The configuration section name.</param>
    /// <returns>The builder instance for chaining.</returns>
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
    /// <typeparam name="T">The storage log configuration type.</typeparam>
    /// <typeparam name="C">The credential type.</typeparam>
    /// <param name="sectionName">The configuration section name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public KSTBuilder AddStorageLogger<T, C>(string sectionName) where T : AzureStorageServiceLogConfig where C : class, ITokenCredentialService {
        this.EnsureCommon();
        this.Services.KSTAddLogger<T, C>(sectionName);
        return this;
    }

    /// <summary>
    /// Registers the request context with the specified type.
    /// </summary>
    /// <typeparam name="T">The request context type.</typeparam>
    /// <returns>The builder instance for chaining.</returns>
    public KSTBuilder AddRequestContext<T>() where T : class, IRequestContext, new() {
        this.EnsureCommon();
        this.Services.KSTAddRequestContext<T>();
        return this;
    }

    /// <summary>
    /// Registers the secret resolver.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public KSTBuilder AddSecretResolver() {
        this.EnsureCommon();
        this.Services.KSTAddSecretResolver();
        return this;
    }

    /// <summary>
    /// Registers a service principal credential with configuration.
    /// </summary>
    /// <typeparam name="T">The service principal configuration type.</typeparam>
    /// <param name="sectionName">The configuration section name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public KSTBuilder AddServicePrincipal<T>(string sectionName) where T : class, IServicePrincipalConfig {
        this.EnsureCommon();
        this.Services.KSTAddServicePrincipalCredentialWithConfig<T>(sectionName);
        return this;
    }

    /// <summary>
    /// Ensures common services are registered.
    /// </summary>
    private void EnsureCommon() {
        if (!this.commonRegistered) {
            this.AddCommon();
        }
    }

    /// <summary>
    /// Resolves the default credential type, throwing if none has been configured.
    /// </summary>
    /// <returns>The default credential <see cref="Type"/>.</returns>
    private Type ResolveCredential() =>
        this.defaultCredentialType ?? throw new InvalidOperationException(
            "No default credential configured. Call UseCredential<C>() first, or use an Add* overload that specifies the credential type.");
}
