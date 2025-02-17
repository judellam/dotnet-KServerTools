namespace KServerTools.Common;

/// <summary>
/// Represents the configuration settings for a service principal.
/// </summary>
public class ServicePrincipalConfiguration : IServicePrincipalConfig {
    private string? secret = null;
    private DateTimeOffset credentialExpiration = DateTimeOffset.MinValue;
    /// <summary>
    /// Gets or sets the application ID of the service principal.
    /// </summary>
    public required string ApplicationId { get; set; }
    /// <summary>
    /// Gets or sets the tenant ID of the service principal.
    /// </summary>
    public required string TenantId { get; set; }
    public required string SecretData { get; set; }
    public ServiceCredentalType CredentialType { get => ServiceCredentalType.ServicePrincipal; }

    [Obsolete("Owning service should set")]
    public ISecretResolver? SecretResolver { get; set; }

    public async Task<string> GetResolvedSecret(CancellationToken cancellationToken) {
        InternalServerErrorException.ThrowIfArgumentIsNull(this.SecretResolver, "SecretResolver must be set before calling GetResolvedSecret. This must be done during dependency injection.");
        if (this.secret == null || DateTimeOffset.UtcNow > this.credentialExpiration) {
            this.credentialExpiration = DateTimeOffset.UtcNow.AddMinutes(45);
            this.secret = await this.SecretResolver!
                .Resolve(this.SecretData, cancellationToken)
                .ConfigureAwait(false);
        }

        return this.secret;
    }
}