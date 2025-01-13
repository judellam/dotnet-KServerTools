namespace KServerTools.Common;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Helper class for dependency injection.
/// </summary>
public static class DependencyHelper {
    /// <summary>
    /// Add the configuration helper to the service collection. This helps parse the appsettions.json file.
    /// </summary>
    public static IServiceCollection ServerToolsAddConfigurationHelper(this IServiceCollection services) =>
        services.AddSingleton<ConfigurationHelper>();

    /// <summary>
    /// Default local credential
    /// </summary>
    public static IServiceCollection ServerToolsAddDefaultCredential(this IServiceCollection services) =>
        services.AddSingleton<IDefaultCredential, DefaultCredential>();

    /// <summary>
    /// Add the key vault service to the service collection.
    /// todo: inject the type of credential to use instead of the default credential.
    /// </summary>
    /// <remarks>
    /// Requires the ConfigurationHelper service to be registered. See <see cref="ServerToolsAddConfigurationHelper()"/>.
    /// Requires the DefaultCredential service to be registered. See <see cref="ServerToolsAddDefaultCredential()"/>.
    /// </remarks>
    public static IServiceCollection ServerToolsAddKeyVault(this IServiceCollection services) {
        services.AddSingleton<IAzureKeyVaultService, AzureKeyVaultService<IDefaultCredential>>();
        return services.AddSingleton<IAzureKeyVaultConfiguration, AzureKeyVaultConfiguration>(static impl => {
            var configHelper = impl.GetService<ConfigurationHelper>() ?? throw new InvalidOperationException("ConfigurationHelper service is not available.");
            var config = configHelper.TryGet<AzureKeyVaultConfiguration>() ?? throw new InvalidOperationException("AzureKeyVaultConfiguration could not be retrieved.");
            return config;
        });
    }

    /// <summary>
    /// Add the service principal configuration to the service collection.
    /// </summary>
    /// <remarks>
    /// Requires the ConfigurationHelper service to be registered. See <see cref="ServerToolsAddConfigurationHelper()"/>.
    /// Requires the SecretResolver service to be registered. See <see cref="ServerToolsAddSecretResolver()"/>.
    /// </remarks>
    public static IServiceCollection ServerToolsAddServicePrincipal(this IServiceCollection services) {
        return services
            .AddSingleton<ServicePrincipalConfiguration>(static impl => {
            var configHelper = impl.GetService<ConfigurationHelper>() ?? throw new InvalidOperationException("ConfigurationHelper service is not available.");
            var config = configHelper.TryGet<ServicePrincipalConfiguration>() ?? throw new InvalidOperationException("ServicePrincipalConfiguration could not be retrieved.");
            ISecretResolver secretResolver = impl.GetService<ISecretResolver>() ?? throw new InvalidOperationException("ISecretResolver service is not available.");
            config.SecretResolver = secretResolver;
            return config;
            }).AddSingleton<IServicePrincipalCredential<ServicePrincipalConfiguration>, ServicePrincipalCredential<ServicePrincipalConfiguration>>();
    }

    public static IServiceCollection ServerToolsAddJsonLogger(this IServiceCollection services) => 
        services.AddSingleton<IJsonLogger, JsonLogger>();

    /// <summary>
    /// Add the secret resolver to the service collection.
    /// </summary>
    /// <remarks>
    /// Key Vault Resolver is required to be registered. See <see cref="ServerToolsAddKeyVault()"/>.
    public static IServiceCollection ServerToolsAddSecretResolver(this IServiceCollection services) =>
        services.AddSingleton<ISecretResolver, SecretResolver>();

    /// <summary>
    /// Add the SQL service to the service collection.
    /// </summary>
    public static IServiceCollection ServerToolsAddSqlService<T>(this IServiceCollection services, bool connectionStringOnly = false)
        where T: ISqlServerDatabaseConfiguration  =>
        connectionStringOnly ? 
            services.AddSingleton<ISqlServerService<T>, SqlServerConnstionString<T>>() :
            services.AddSingleton<ISqlServerService<T>, SqlServerService<T>>();

    /// <summary>
    /// Adds a generic request context.
    /// </summary>
    public static IServiceCollection ServerToolsAddRequestContext<T>(this IServiceCollection services) 
        where T: class, IRequestContext, new() =>
        services.AddSingleton<IRequestContextAccessor, RequestContextAccessor<T>>();
}