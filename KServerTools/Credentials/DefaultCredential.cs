namespace KServerTools.Common;

using Azure.Core;
using Azure.Identity;

public class DefaultCredential : IDefaultCredential {
    /// <summary>
    /// Retrieves the default Azure credential.
    /// </summary>
    /// <returns>A <see cref="TokenCredential"/> instance representing the default Azure credential.</returns>
    public Task<TokenCredential> GetCredential(CancellationToken cancellationToken) {
        return Task.FromResult<TokenCredential>(new DefaultAzureCredential());
    }
}