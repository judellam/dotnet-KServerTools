namespace KServerTools.Common;

/// <summary>
/// Specifies the type of credential used to authenticate with Azure services.
/// </summary>
public enum ServiceCredentalType {
    /// <summary>A certificate-based credential.</summary>
    Certificate,

    /// <summary>The default Azure credential (managed identity, environment, etc.).</summary>
    DefaultCredential,

    /// <summary>A service principal credential using client ID and secret.</summary>
    ServicePrincipal,
}
