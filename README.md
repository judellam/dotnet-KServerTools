# KServerTools

KServerTools is a .NET Core package that provides common functionality for Kestrel servers. This package aims to simplify the development and maintenance of Kestrel-based applications by offering a set of reusable tools and utilities.

## Features

- **Request Logging**: Easily log incoming requests and responses.
- **Error Handling**: Centralized error handling and custom error responses.
- **Configuration Management**: Simplified configuration setup and management.
- **Performance Monitoring**: Tools for monitoring and improving server performance.

## Installation

To install KServerTools, run the following command in your project directory:

```bash
TODO|||| dotnet add package KServerTools
```

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

## Contributing

We welcome contributions! Please see our [contributing guidelines](CONTRIBUTING.md) for more information.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Contact

For questions or support, please contact us at [support@example.com](mailto:support@example.com).
