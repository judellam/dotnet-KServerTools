namespace KServerTools.Common;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Helper class for dependency injection.
/// </summary>
public static class DependencyHelper {
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
    /// todo: inject the type of credential to use instead of the default credential.
    /// </summary>
    /// <remarks>
    /// Requires the ConfigurationHelper service to be registered. See <see cref="ServerToolsAddConfigurationHelper()"/>.
    /// Requires the DefaultCredential service to be registered. See <see cref="ServerToolsAddDefaultCredential()"/>.
    /// </remarks>
    public static IServiceCollection KSTAddKeyVault<T, C>(this IServiceCollection services, string sectionName) where T: class, IAzureKeyVaultConfiguration where C: ITokenCredentialService =>
        services.AddSingleton<T>(impl => {
            ConfigurationHelper configHelper = impl.GetConfigurationHelper();
            var config = configHelper.TryGet<T>(sectionName) ?? throw new InvalidOperationException("AzureKeyVaultConfiguration could not be retrieved.");
            return config;
        }).AddSingleton<IAzureKeyVaultService<T>, AzureKeyVaultService<T, C>>();
    
    /// <summary>
    /// Add the configuration helper to the service collection.
    /// </summary>
    /// <typeparam name="T">An impl of IAzureStorageServiceConfig</typeparam>
    /// <typeparam name="C">The Credential you want to use to write to storage</typeparam>
    public static IServiceCollection KSTAddLogger<T, C>(this IServiceCollection services, string storageLogConfigSectionName) where T: AzureStorageServiceLogConfig where C: class, ITokenCredentialService {
        ArgumentNullException.ThrowIfNullOrEmpty(storageLogConfigSectionName, nameof(storageLogConfigSectionName));
        return services.AddSingleton<T>(impl => {
                ConfigurationHelper configHelper = impl.GetConfigurationHelper();
                return configHelper.TryGet<T>(storageLogConfigSectionName) ?? throw new InvalidOperationException("AzureStorageServiceConfig could not be retrieved.");
            })
            .AddSingleton<AzureStorageServiceInternal<T, C>>()
            .AddSingleton<IJsonLogger, JsonStorageLogger<T, C>>();
    }

    public static IServiceCollection KSTAddLogger(this IServiceCollection services) => 
        services.AddSingleton<IJsonLogger, JsonLogger>();

    /// <summary>
    /// Add the secret resolver to the service collection.
    /// </summary>
    /// <remarks>
    /// Key Vault Resolver is required to be registered. See <see cref="ServerToolsAddKeyVault()"/>.
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

    private static ConfigurationHelper GetConfigurationHelper(this IServiceProvider provider) =>
        provider.GetService<ConfigurationHelper>() ?? throw new InvalidOperationException("ConfigurationHelper service is not available.");

    /// <summary>
    /// Adds a Azure Storage  service to the service collection. It can be referenced like IAzureStorageService<typeparamref name="T"/>>
    /// </summary>
    /// <typeparam name="T">Configuration to the queue</typeparam>
    /// <typeparam name="C">Credential to be used when accessing</typeparam>
    /// <param name="services"></param>
    /// <param name="sectionName">Configuration name</param>
    public static IServiceCollection KSTAddAzureStorageService<T, C>(this IServiceCollection services, string sectionName) where T: class, IAzureStorageServiceConfig where C: class, ITokenCredentialService {
        ArgumentNullException.ThrowIfNullOrEmpty(sectionName, nameof(sectionName));
        return services.AddSingleton<T>(impl => {
                ConfigurationHelper configHelper = impl.GetConfigurationHelper();
                return configHelper.TryGet<T>(sectionName) ?? throw new InvalidOperationException("AzureStorageServiceConfig could not be retrieved.");
            })
            .AddSingleton<IAzureStorageService<T>, AzureStorageService<T, C>>();
    }

    /// <summary>
    /// Adds a Azure Storage Queue service to the service collection. It can be referenced like IAzureStorageQueueService<typeparamref name="T"/>>
    /// </summary>
    /// <typeparam name="T">Configuration to the queue</typeparam>
    /// <typeparam name="C">Credential to be used when accessing</typeparam>
    /// <param name="services"></param>
    /// <param name="sectionName">Configuration name</param>
    public static IServiceCollection KSTAddAzureStorageQueue<T,C>(this IServiceCollection services, string sectionName) where T: class, IAzureStorageServiceConfig where C: class, ITokenCredentialService {
        ArgumentNullException.ThrowIfNullOrEmpty(sectionName, nameof(sectionName));
        return services.AddSingleton<T>(impl => {
                ConfigurationHelper configHelper = impl.GetConfigurationHelper();
                return configHelper.TryGet<T>(sectionName) ?? throw new InvalidOperationException("AzureStorageServiceConfig could not be retrieved.");
            })
            .AddSingleton<IAzureStorageQueueService<T>, AzureStorageQueueService<T, C>>();
    }
}