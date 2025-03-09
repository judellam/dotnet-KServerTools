namespace KServerTools.Common;

public interface IAzureCosmosDbConfiguration {
    string EndpointUri { get; set; }
    string PrimaryKey { get; set; }
}

public class AzureCosmosDbConfiguration : IAzureCosmosDbConfiguration {
    private string? primaryKey;
    public required string EndpointUri { get; set; }
    public string PrimaryKey { 
        get {
            this.primaryKey = this.GetSecret(CancellationToken.None).Result;
            return this.primaryKey;
        }
        set {
            this.primaryKey = value;
        }
    }
    public virtual Task<string> GetSecret(CancellationToken cancellationToken) {
        return Task.FromResult(this.primaryKey ?? string.Empty);
    }
}