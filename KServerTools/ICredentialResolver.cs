namespace KServerTools.Common;

using Azure.Core;

/// <summary>
/// Defines a method to resolve credentials.
/// </summary>
public interface ICredentialResolver {
    /// <summary>
    /// Retrieves the credential.
    /// </summary>
    /// <returns>A <see cref="TokenCredential"/> instance.</returns>
    Task<TokenCredential> GetCredential(CancellationToken cancellationToken);
}