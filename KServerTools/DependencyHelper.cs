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
    public static IServiceCollection AddKServerTools(this IServiceCollection services, Action<KSTBuilder> configure) {
        var builder = new KSTBuilder(services);
        configure(builder);
        return services;
    }

    /// <summary>
    /// Add the configuration helper to the service collection. This helps parse the appsettions.json file.
    /// </summary>
    public static IServiceCollection KSTAddCommon(this IServiceCollection services) =>
        services
            .AddSingleton<ConfigurationHelper>()
            .AddSingleton<DefaultCredentialConfig>()
            .AddSingleton<IDefaultCredential, DefaultCredential<DefaultCredentialConfig>>()
            .AddMemoryCache();

    /// <summary>
    /// Adds a generic request context.
    /// </summary>
    public static IServiceCollection KSTAddRequestContext<T>(this IServiceCollection services) 
        where T: class, IRequestContext, new() =>
        services
            .AddSingleton<IRequestContextAccessor, RequestContextAccessor<T>>()
            .AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

    /// <summary>
    /// Provide the section name and your class implementation for IServicePrincipalConfig for config class to register a service principal credential.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public static IServiceCollection KSTAddServicePrincipalCredentialWithConfig<T>(this IServiceCollection services, string sectionName) where T: class, IServicePrincipalConfig {
        ArgumentNullException.ThrowIfNull(sectionName, nameof(sectionName));
        return services
            .AddSingleton<IServicePrincipalCredential<T>, ServicePrincipalCredential<T>>()
            .AddSingleton<ServicePrincipalCredential<T>>();
    }

    /// <summary>
    /// Add the key vault service to the service collection.
    /// </summary>
    public static IServiceCollection KSTAddKeyVault<T, C>(this IServiceCollection services, string sectionName) where T: class, IAzureKeyVaultConfiguration where C: ITokenCredentialService =>
        services
            .AddConfigSection<T>(sectionName)
            .AddSingleton<IAzureKeyVaultService<T>, AzureKeyVaultService<T, C>>();
    
    /// <summary>
    /// Add the storage-backed logger.
    /// </summary>
    public static IServiceCollection KSTAddLogger<T, C>(this IServiceCollection services, string storageLogConfigSectionName) where T: AzureStorageServiceLogConfig where C: class, ITokenCredentialService {
        ArgumentNullException.ThrowIfNullOrEmpty(storageLogConfigSectionName, nameof(storageLogConfigSectionName));
        return services
            .AddConfigSection<T>(storageLogConfigSectionName)
            .AddSingleton<AzureStorageServiceInternal<T, C>>()
            .AddSingleton<IJsonLogger, JsonStorageLogger<T, C>>();
    }

    public static IServiceCollection KSTAddLogger(this IServiceCollection services) => 
        services.AddSingleton<IJsonLogger, JsonLogger>();

    /// <summary>
    /// Register IJsonLogger backed by Microsoft.Extensions.Logging.ILogger&lt;T&gt;.
    /// Use this when you want to integrate with the standard ILogger pipeline (e.g., Serilog, Application Insights).
    /// </summary>
    public static IServiceCollection KSTAddLogger<T>(this IServiceCollection services) =>
        services.AddSingleton<IJsonLogger, ILoggerAdapter<T>>();

    /// <summary>
    /// Add the secret resolver to the service collection.
    /// </summary>
    public static IServiceCollection KSTAddSecretResolver(this IServiceCollection services) =>
        services.AddSingleton<ISecretResolver, SecretResolver>();

    /// <summary>
    /// Add the SQL service to the service collection.
    /// </summary>
    public static IServiceCollection KSTAddSqlService<T, C>(this IServiceCollection services)
        where T: ISqlServerDatabaseConfiguration  
        where C: class, ITokenCredentialService =>
            services.AddSingleton<ISqlServerService<T>, SqlServerService<T, C>>();

    /// <summary>
    /// Add the SQL service to the service collection.
    /// </summary>
    public static IServiceCollection KSTAddSqlServiceConnectionString<T>(this IServiceCollection services)
        where T: ISqlServerDatabaseConfiguration => services.AddSingleton<ISqlServerService<T>, SqlServerConnstionString<T>>();

    /// <summary>
    /// Adds an Azure Storage service to the service collection.
    /// </summary>
    public static IServiceCollection KSTAddAzureStorageService<T, C>(this IServiceCollection services, string sectionName) where T: class, IAzureStorageServiceConfig where C: class, ITokenCredentialService {
        ArgumentNullException.ThrowIfNullOrEmpty(sectionName, nameof(sectionName));
        return services
            .AddConfigSection<T>(sectionName)
            .AddSingleton<IAzureStorageService<T>, AzureStorageService<T, C>>()
            .AddSingleton<IAzureBlobManagementService<T>>(sp => (IAzureBlobManagementService<T>)sp.GetRequiredService<IAzureStorageService<T>>());
    }

    public static IServiceCollection KSTAddAzureCosmosDb<T, C>(this IServiceCollection services, string sectionName) where T: class, IAzureCosmosDbConfiguration where C: class, ITokenCredentialService {
        ArgumentNullException.ThrowIfNullOrEmpty(sectionName, nameof(sectionName));
        return services
            .AddConfigSection<T>(sectionName)
            .AddSingleton<IAzureCosmosDb<T>, AzureCosmosDb<T, C>>();
    }

    /// <summary>
    /// Adds an Azure Storage Queue service to the service collection.
    /// </summary>
    public static IServiceCollection KSTAddAzureStorageQueue<T,C>(this IServiceCollection services, string sectionName) where T: class, IAzureStorageServiceConfig where C: class, ITokenCredentialService {
        ArgumentNullException.ThrowIfNullOrEmpty(sectionName, nameof(sectionName));
        return services
            .AddConfigSection<T>(sectionName)
            .AddSingleton<IAzureStorageQueueService<T>, AzureStorageQueueService<T, C>>()
            .AddSingleton<IAzureQueueManagementService<T>>(sp => (IAzureQueueManagementService<T>)sp.GetRequiredService<IAzureStorageQueueService<T>>());
    }

    /// <summary>
    /// Registers a configuration section as a singleton, loaded via ConfigurationHelper.
    /// </summary>
    internal static IServiceCollection AddConfigSection<T>(this IServiceCollection services, string sectionName) where T : class =>
        services.AddSingleton<T>(impl => {
            ConfigurationHelper configHelper = impl.GetConfigurationHelper();
            return configHelper.TryGet<T>(sectionName) ?? throw new InvalidOperationException($"{typeof(T).Name} could not be retrieved from section '{sectionName}'.");
        });

    internal static ConfigurationHelper GetConfigurationHelper(this IServiceProvider provider) =>
        provider.GetService<ConfigurationHelper>() ?? throw new InvalidOperationException("ConfigurationHelper service is not available.");
}