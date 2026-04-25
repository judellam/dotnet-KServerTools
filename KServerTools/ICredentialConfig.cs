namespace KServerTools.Common;

/// <summary>
/// Interface for a credential configuration in Azure.
/// </summary>
public interface ICredentialConfig {
    /// <summary>
    /// Gets the type of credential.
    /// </summary>
    public ServiceCredentalType CredentialType { get; }
}

/// <summary>
/// Configuration for an Azure service principal credential.
/// </summary>
public interface IServicePrincipalConfig : ICredentialConfig {
    /// <summary>
    /// Gets or sets the application (client) identifier.
    /// </summary>
    public string ApplicationId { get; set; }

    /// <summary>
    /// Gets or sets the Azure AD tenant identifier.
    /// </summary>
    public string TenantId { get; set; }

    /// <summary>
    /// Gets or sets the client secret data.
    /// </summary>
    public string SecretData { get; set; }

    /// <summary>
    /// Gets the resolved client secret.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The resolved client secret value.</returns>
    public Task<string> GetSecret(CancellationToken cancellationToken);
}
