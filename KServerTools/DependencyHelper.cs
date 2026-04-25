namespace KServerTools.Common;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Helper class for dependency injection.
/// </summary>
public static class DependencyHelper {
    /// <summary>
    /// Fluent entry point for configuring KServerTools services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">The builder configuration action.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddKServerTools(this IServiceCollection services, Action<KSTBuilder> configure) {
        var builder = new KSTBuilder(services);
        configure(builder);
        return services;
    }

    /// <summary>
    /// Add the configuration helper to the service collection. This helps parse the appsettions.json file.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection KSTAddCommon(this IServiceCollection services) =>
        services
            .AddSingleton<ConfigurationHelper>()
            .AddSingleton<DefaultCredentialConfig>()
            .AddSingleton<IDefaultCredential, DefaultCredential<DefaultCredentialConfig>>()
            .AddMemoryCache();

    /// <summary>
    /// Adds a generic request context.
    /// </summary>
    /// <typeparam name="T">The request context type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection KSTAddRequestContext<T>(this IServiceCollection services)
        where T : class, IRequestContext, new() =>
        services
            .AddSingleton<IRequestContextAccessor, RequestContextAccessor<T>>()
            .AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

    /// <summary>
    /// Provide the section name and your class implementation for IServicePrincipalConfig for config class to register a service principal credential.
    /// </summary>
    /// <typeparam name="T">The service principal configuration type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="sectionName">The configuration section name.</param>
    /// <exception cref="InvalidOperationException">Thrown when the section name is null.</exception>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection KSTAddServicePrincipalCredentialWithConfig<T>(this IServiceCollection services, string sectionName) where T : class, IServicePrincipalConfig {
        ArgumentNullException.ThrowIfNull(sectionName, nameof(sectionName));
        return services
            .AddSingleton<IServicePrincipalCredential<T>, ServicePrincipalCredential<T>>()
            .AddSingleton<ServicePrincipalCredential<T>>();
    }

    /// <summary>
    /// Add the key vault service to the service collection.
    /// </summary>
    /// <typeparam name="T">The Key Vault configuration type.</typeparam>
    /// <typeparam name="C">The credential type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="sectionName">The configuration section name.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection KSTAddKeyVault<T, C>(this IServiceCollection services, string sectionName) where T : class, IAzureKeyVaultConfiguration where C : ITokenCredentialService =>
        services
            .AddConfigSection<T>(sectionName)
            .AddSingleton<IAzureKeyVaultService<T>, AzureKeyVaultService<T, C>>();

    /// <summary>
    /// Add the storage-backed logger.
    /// </summary>
    /// <typeparam name="T">The storage log configuration type.</typeparam>
    /// <typeparam name="C">The credential type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="storageLogConfigSectionName">The configuration section name for storage logging.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection KSTAddLogger<T, C>(this IServiceCollection services, string storageLogConfigSectionName) where T : AzureStorageServiceLogConfig where C : class, ITokenCredentialService {
        ArgumentNullException.ThrowIfNullOrEmpty(storageLogConfigSectionName, nameof(storageLogConfigSectionName));
        return services
            .AddConfigSection<T>(storageLogConfigSectionName)
            .AddSingleton<AzureStorageServiceInternal<T, C>>()
            .AddSingleton<IJsonLogger, JsonStorageLogger<T, C>>();
    }

    /// <summary>
    /// Add the console JSON logger.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection KSTAddLogger(this IServiceCollection services) =>
        services.AddSingleton<IJsonLogger, JsonLogger>();

    /// <summary>
    /// Register IJsonLogger backed by Microsoft.Extensions.Logging.ILogger&lt;T&gt;.
    /// Use this when you want to integrate with the standard ILogger pipeline (e.g., Serilog, Application Insights).
    /// </summary>
    /// <typeparam name="T">The category type for the underlying logger.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection KSTAddLogger<T>(this IServiceCollection services) =>
        services.AddSingleton<IJsonLogger, ILoggerAdapter<T>>();

    /// <summary>
    /// Add the secret resolver to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection KSTAddSecretResolver(this IServiceCollection services) =>
        services.AddSingleton<ISecretResolver, SecretResolver>();

    /// <summary>
    /// Add the SQL service to the service collection.
    /// </summary>
    /// <typeparam name="T">The SQL Server configuration type.</typeparam>
    /// <typeparam name="C">The credential type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection KSTAddSqlService<T, C>(this IServiceCollection services)
        where T : ISqlServerDatabaseConfiguration
        where C : class, ITokenCredentialService =>
            services.AddSingleton<ISqlServerService<T>, SqlServerService<T, C>>();

    /// <summary>
    /// Add the SQL service with connection string auth to the service collection.
    /// </summary>
    /// <typeparam name="T">The SQL Server configuration type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection KSTAddSqlServiceConnectionString<T>(this IServiceCollection services)
        where T : ISqlServerDatabaseConfiguration => services.AddSingleton<ISqlServerService<T>, SqlServerConnstionString<T>>();

    /// <summary>
    /// Adds an Azure Storage service to the service collection.
    /// </summary>
    /// <typeparam name="T">The storage configuration type.</typeparam>
    /// <typeparam name="C">The credential type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="sectionName">The configuration section name.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection KSTAddAzureStorageService<T, C>(this IServiceCollection services, string sectionName) where T : class, IAzureStorageServiceConfig where C : class, ITokenCredentialService {
        ArgumentNullException.ThrowIfNullOrEmpty(sectionName, nameof(sectionName));
        return services
            .AddConfigSection<T>(sectionName)
            .AddSingleton<IAzureStorageService<T>, AzureStorageService<T, C>>()
            .AddSingleton<IAzureBlobManagementService<T>>(sp => (IAzureBlobManagementService<T>)sp.GetRequiredService<IAzureStorageService<T>>());
    }

    /// <summary>
    /// Adds an Azure Cosmos DB service to the service collection.
    /// </summary>
    /// <typeparam name="T">The Cosmos DB configuration type.</typeparam>
    /// <typeparam name="C">The credential type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="sectionName">The configuration section name.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection KSTAddAzureCosmosDb<T, C>(this IServiceCollection services, string sectionName) where T : class, IAzureCosmosDbConfiguration where C : class, ITokenCredentialService {
        ArgumentNullException.ThrowIfNullOrEmpty(sectionName, nameof(sectionName));
        return services
            .AddConfigSection<T>(sectionName)
            .AddSingleton<IAzureCosmosDb<T>, AzureCosmosDb<T, C>>();
    }

    /// <summary>
    /// Adds an Azure Storage Queue service to the service collection.
    /// </summary>
    /// <typeparam name="T">The storage configuration type.</typeparam>
    /// <typeparam name="C">The credential type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="sectionName">The configuration section name.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection KSTAddAzureStorageQueue<T, C>(this IServiceCollection services, string sectionName) where T : class, IAzureStorageServiceConfig where C : class, ITokenCredentialService {
        ArgumentNullException.ThrowIfNullOrEmpty(sectionName, nameof(sectionName));
        return services
            .AddConfigSection<T>(sectionName)
            .AddSingleton<IAzureStorageQueueService<T>, AzureStorageQueueService<T, C>>()
            .AddSingleton<IAzureQueueManagementService<T>>(sp => (IAzureQueueManagementService<T>)sp.GetRequiredService<IAzureStorageQueueService<T>>());
    }

    /// <summary>
    /// Registers a configuration section as a singleton, loaded via ConfigurationHelper.
    /// </summary>
    /// <typeparam name="T">The configuration type to bind.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="sectionName">The configuration section name.</param>
    /// <returns>The configured service collection.</returns>
    internal static IServiceCollection AddConfigSection<T>(this IServiceCollection services, string sectionName) where T : class =>
        services.AddSingleton<T>(impl => {
            ConfigurationHelper configHelper = impl.GetConfigurationHelper();
            return configHelper.TryGet<T>(sectionName) ?? throw new InvalidOperationException($"{typeof(T).Name} could not be retrieved from section '{sectionName}'.");
        });

    /// <summary>
    /// Gets the <see cref="ConfigurationHelper"/> from the service provider.
    /// </summary>
    /// <param name="provider">The service provider.</param>
    /// <returns>The resolved <see cref="ConfigurationHelper"/> instance.</returns>
    internal static ConfigurationHelper GetConfigurationHelper(this IServiceProvider provider) =>
        provider.GetService<ConfigurationHelper>() ?? throw new InvalidOperationException("ConfigurationHelper service is not available.");
}
