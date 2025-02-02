namespace KServerTools.Common;

/// <summary>
/// Interface for an Azure service principal credential.
/// </summary>
/// <typeparam name="T">Configuration for the credential.</typeparam>
public interface IServicePrincipalCredential<T> : ITokenCredentialService where T: ICredentialConfig {
}