namespace KServerTools.Common;

/// <summary>
/// Interface for a credential configuration in Azure.
/// </summary>
public interface ICredentialConfig {
    /// <summary>
    /// The application id of the service principal.
    /// </summary>
    string ApplicationId { get; set; }

    /// <summary>
    /// The tenant id of the service principal.
    /// </summary>
    string TenantId { get; set; }

    /// <summary>
    /// The secret resolver should operate on this data. See <see cref="GetResolvedSecret"/> in ServicePrincipalCredential.
    /// </summary>
    string SecretData { get; }

    /// <summary>
    /// Get the resolved secret.
    /// </summary>
    Task<string> GetResolvedSecret(CancellationToken cancellationToken);
}