namespace KServerTools.Common;

/// <summary>
/// Represents the configuration settings for a service principal.
/// </summary>
public abstract class ServicePrincipalConfiguration : IServicePrincipalConfig {
    /// <summary>
    /// Gets or sets the application ID of the service principal.
    /// </summary>
    public required string ApplicationId { get; set; }

    /// <summary>
    /// Gets or sets the tenant ID of the service principal.
    /// </summary>
    public required string TenantId { get; set; }

    /// <summary>
    /// Gets or sets the secret data of the service principal.
    /// </summary>
    public required string SecretData { get; set; }

    /// <summary>
    /// Gets the type of credential.
    /// </summary>
    public ServiceCredentalType CredentialType { get => ServiceCredentalType.ServicePrincipal; }

    /// <summary>
    /// Gets the secret for the service principal.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The resolved secret value.</returns>
    public abstract Task<string> GetSecret(CancellationToken cancellationToken);
}
