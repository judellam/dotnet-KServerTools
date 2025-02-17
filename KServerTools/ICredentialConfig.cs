namespace KServerTools.Common;

/// <summary>
/// Interface for a credential configuration in Azure.
/// </summary>
public interface ICredentialConfig {
    public ServiceCredentalType CredentialType { get; }

    /// <summary>
    /// Get the resolved secret.
    /// </summary>
    Task<string> GetResolvedSecret(CancellationToken cancellationToken);
}

public interface IServicePrincipalConfig : ICredentialConfig {
    public string ApplicationId { get; set; }
    public string TenantId { get; set; }
    public string SecretData { get; set; }

    [Obsolete("Owning service should set")]
    public ISecretResolver? SecretResolver { get; set; } // needs to be set via DI
}