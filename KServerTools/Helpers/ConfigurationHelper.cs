namespace KServerTools.Common;

using Microsoft.Extensions.Configuration;

/// <summary>
/// Helper class for reading configuration settings.
/// </summary>
/// <param name="configuration">The configuration section in the appsettings.json</param>
/// <remarks>
/// Used to parse configuration settings from appsettings.json. Will parse objects of type T. See IAzureKeyVaultConfiguration for an example.
/// Required: Microsoft.Extensions.Configuration;
/// </remarks>
public class ConfigurationHelper(IConfiguration configuration) {
    private readonly IConfiguration configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

    public T? TryGet<T>() where T : class => 
        this.TryGet<T>(typeof(T).Name);

    public T? TryGet<T>(string sectionName) where T: class {  
        try {
            var section = this.configuration.GetSection(sectionName);
            if (section != null) {
                var result = section.Get<T>();
                return result ?? null;
            }

            return null;
        }
        catch {
            return null;
        }
    }
}