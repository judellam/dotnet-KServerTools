# KServerTools
KServerTools is a .NET Core package that provides common functionality for Kestrel servers. This package aims to simplify the development and maintenance of Kestrel-based applications by offering a set of reusable tools and utilities.

## Features
- **Request Logging**: Easily log incoming requests and responses.
- **Error Handling**: Centralized error handling and custom error responses.
- **Configuration Management**: Simplified configuration setup and management.
- **Performance Monitoring**: Tools for monitoring and improving server performance.

## Installation
To install KServerTools nuget, run the following command in your project directory:

```bash
dotnet add package KServerTools
```

### Example Code repo
[GitHub Example Repository](https://github.com/judellam/dotnet-KServerTools-example)

## Usage
Here's a basic example of how to use KServerTools in your Kestrel server:

```csharp
// TODO
using KServerTools.Common;

var builder = WebApplication.CreateBuilder(args);
IServiceCollection services = builder.Services;
services.AddControllers();
services
    // KST Add-ons
    .KSTAddRequestContext<RequestContext>()
    .KSTAddCommon()
    .KSTAddLogger()
    .KSTAddSqlServiceConnectionString<UserDatabaseSqlServerConfiguration>()

    // Configs
    .AddSingleton(static impl=> {
        var configHelper = impl.GetService<ConfigurationHelper>() ?? throw new InvalidOperationException("ConfigurationHelper service is not available.");
        var config = configHelper.TryGet<UserDatabaseSqlServerConfiguration>() ?? throw new InvalidOperationException("UserDatabaseSqlServerConfiguration could not be retrieved.");
        return config;
    })

var app = builder.Build();
app.Run();
```

Example Config
Update the {{USER_ID}} with a real user Id
Update the {{PASSWORD_TO_BE_SET_HERE_EXAMPLE}} with your real password
```json
  "UserDatabaseSqlServerConfiguration": {
    "ConnectionStringData": "Server=tcp:localhost,1433;Initial Catalog=UserDb;Persist Security Info=False;User ID={{USER_ID}};Password={{PASSWORD_TO_BE_SET_HERE_EXAMPLE}};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;",
    "Server": "localhost,1433",
    "Database": "UserDb",
    "Scopes": [
      "https://database.windows.net/.default"
    ]
  }
```

### Service Principal Configuration / AKV Example ###

Example on how to use the injection and create the configuration objects.
The DefaultServicePrincipalConfiguration inherits from ServicePrincipalConfiguration and allows you to load any number of SPs. You can just use ServicePrincipalConfiguration if you only have one.

When loading the SP via DI, you can specify the configuration name "ServicePrincipalConfiguration" in the line:
    .KSTAddServicePrincipalCredentialWithConfig<DefaultServicePrincipalConfiguration>(nameof(ServicePrincipalConfiguration))

Notice that the configuration uses akv://SpClientSecret. This means you need a secret resolver and an AKV.

```csharp
// The configuration object where the details will be loaded into.
public class DefaultServicePrincipalConfiguration : ServicePrincipalConfiguration {
}

/// ...
var builder = WebApplication.CreateBuilder(args);
IServiceCollection services = builder.Services;
services
    .KSTAddSecretResolver()
    .KSTAddKeyVault<DefaultAzureKeyVaultConfiguration, IDefaultCredential>(nameof(AzureKeyVaultConfiguration)) // use the Default Credential
    .KSTAddServicePrincipalCredentialWithConfig<DefaultServicePrincipalConfiguration>(nameof(ServicePrincipalConfiguration))
    .KSTAddSqlService<UserDatabaseSqlServerConfiguration, IServicePrincipalCredential<DefaultServicePrincipalConfiguration>>()
    .AddSingleton<UserDatabaseSqlServerConfiguration>(static impl=> {
        var configHelper = impl.GetService<ConfigurationHelper>() ?? throw new InvalidOperationException("ConfigurationHelper service is not available.");
        // Read from the appsettings.json
        var config = configHelper.TryGet<UserDatabaseSqlServerConfiguration>() ?? throw new InvalidOperationException("UserDatabaseSqlServerConfiguration could not be retrieved.");
        config.SecretResolver = GetSecretResolver<DefaultAzureKeyVaultConfiguration>(impl);
        return config;
    })

    // ** In DI Code we need to register the AKV with the secret resolver //
private static ISecretResolver GetSecretResolver<AKVConfig>(this IServiceProvider serviceProvider) where AKVConfig: IAzureKeyVaultConfiguration {
    ISecretResolver secretResolver = serviceProvider.GetService<ISecretResolver>() ?? throw new InvalidOperationException("ISecretResolver service is not available.");
    IAzureKeyVaultService<AKVConfig> akvService = serviceProvider.GetService<IAzureKeyVaultService<AKVConfig> >() ?? throw new InvalidOperationException("ISecretResolver service is not available.");
    secretResolver.RegisterKeyVaultService(akvService);
    return secretResolver;
}
```

Example of a configuration to get the SP secrets. The configuration name can be configured.
```json
  "AzureKeyVaultConfiguration": {
    "Uri": "https://{{AKV_NAME}}.vault.azure.net/",
    "CacheDurationInSeconds": 300
  },
  "ServicePrincipalConfiguration": {
    "TenantId": "tenant-id",
    "ApplicationId": "app-id",
    "SecretData": "akv://SpClientSecret"
  },
```

### Azure Queue Example ###
```csharp
    private static IServiceCollection AddServerTools(this IServiceCollection serviceCollection) =>
        serviceCollection
            .KSTAddCommon() // Adds the IDefaultCredential
            .KSTAddAzureStorageQueue<AzureStorageQueueConfig, IDefaultCredential>(nameof(AzureStorageQueueConfig));
```
```json
  "AzureStorageQueueConfig" : {
    "AccountName": "dastr",
    "Endpoint": "queue.core.windows.net"
  }
```

## Contributing

We welcome contributions! Please see our [contributing guidelines](CONTRIBUTING.md) for more information.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.