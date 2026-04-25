namespace KServerTools.Common;

/// <summary>
/// Configuration for the default Azure credential.
/// </summary>
internal class DefaultCredentialConfig : ICredentialConfig {
    /// <summary>
    /// Gets the credential type, which is always <see cref="ServiceCredentalType.DefaultCredential"/>.
    /// </summary>
    public ServiceCredentalType CredentialType => ServiceCredentalType.DefaultCredential;

    /// <summary>
    /// Not supported for default credentials. Always throws <see cref="NotImplementedException"/>.
    /// </summary>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>This method always throws.</returns>
    /// <exception cref="NotImplementedException">Always thrown because default credentials do not use secrets.</exception>
    public Task<string> GetResolvedSecret(CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }
}
