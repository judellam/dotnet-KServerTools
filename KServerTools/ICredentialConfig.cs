namespace KServerTools.Common;

/// <summary>
/// Interface for a credential configuration in Azure.
/// </summary>
public interface ICredentialConfig {
    public ServiceCredentalType CredentialType { get; }
}

public interface IServicePrincipalConfig : ICredentialConfig {
    public string ApplicationId { get; set; }
    public string TenantId { get; set; }
    public string SecretData { get; set; }
    public Task<string> GetSecret(CancellationToken cancellationToken);
}