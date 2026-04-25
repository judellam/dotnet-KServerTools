namespace KServerTools.Common;

using Microsoft.Extensions.Configuration;

/// <summary>
/// Helper class for reading configuration settings.
/// </summary>
/// <param name="configuration">The configuration section in the appsettings.json.</param>
/// <remarks>
/// Used to parse configuration settings from appsettings.json. Will parse objects of type T. See IAzureKeyVaultConfiguration for an example.
/// Required: Microsoft.Extensions.Configuration.
/// </remarks>
public class ConfigurationHelper(IConfiguration configuration) {
    private readonly IConfiguration configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

    /// <summary>
    /// Attempts to retrieve a configuration section of type <typeparamref name="T"/> using the type name as the section name.
    /// </summary>
    /// <typeparam name="T">The configuration type to bind.</typeparam>
    /// <returns>The bound configuration instance, or <see langword="null"/> if not found.</returns>
    public T? TryGet<T>() where T : class =>
        this.TryGet<T>(typeof(T).Name);

    /// <summary>
    /// Attempts to retrieve a configuration section of type <typeparamref name="T"/> using the specified section name.
    /// </summary>
    /// <typeparam name="T">The configuration type to bind.</typeparam>
    /// <param name="sectionName">The configuration section name.</param>
    /// <returns>The bound configuration instance, or <see langword="null"/> if not found.</returns>
    public T? TryGet<T>(string sectionName) where T : class {
        try {
            var section = this.configuration.GetSection(sectionName);
            if (section != null) {
                var result = section.Get<T>();
                return result ?? null;
            }

            return null;
        } catch (Exception ex) {
            Console.WriteLine($"Error reading configuration section {sectionName}: {ex.Message}");
            return null;
        }
    }
}
