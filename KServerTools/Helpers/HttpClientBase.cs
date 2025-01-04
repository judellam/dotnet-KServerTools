namespace KServerTools.Common;

using System.Diagnostics;
using System.Net.Http;
using System.Text;

using CustomHeaders = System.Collections.Generic.IList<(string key, string value)>;

/// <summary>
/// This class should be overridden to provide a base class for HTTP clients.
/// </summary>
/// <example>
/// public class MyHttpClient : HttpClientBase {
///    public MyHttpClient(IHttpClientFactory clientFactory, IJsonLogger logger) : base(clientFactory, logger) { }
///    public override string GetClientName() => "MyHttpClient";
///    public async Task<HttpResponseMessage> MyPost(string path, CustomHeaders headers, string body, CancellationToken cancellationToken) {
///         return await this.Post(path, headers, body, cancellationToken); // Send is also available and more flexible.
///    }
/// }
/// </example>
/// <seealso cref="HttpClient"/>
/// </remarks>
/// todo: Remove JSON logging and allow own logger to be passed in. The HttpClient logs on it's own, so a mix of logs come through on the console.
/// </remarks>
public abstract class HttpClientBase {
    private readonly IHttpClientFactory clientFactory;
    private readonly IJsonLogger logger;

    public HttpClientBase(IHttpClientFactory clientFactory, IJsonLogger logger) {
        this.clientFactory = clientFactory;
        this.logger = logger;
    }

    public abstract string GetClientName();

    protected async Task<HttpResponseMessage> Post(string path, CustomHeaders headers, string body, CancellationToken cancellationToken) => 
        await this.Send(path, HttpMethod.Post, headers, body, cancellationToken);

    protected async Task <HttpResponseMessage> Send(string path, HttpMethod httpMethod, CustomHeaders headers, string body, CancellationToken cancellationToken) {
        Stopwatch sw = Stopwatch.StartNew();
        string statusCode = "";
        bool success = false;

        // Create the client
        HttpClient client = this.clientFactory.CreateClient(this.GetClientName());

        ArgumentNullException.ThrowIfNull(client, "Unable to create client");
        ArgumentNullException.ThrowIfNull(client.BaseAddress, "Client base address is null");
        
        UriBuilder uriBuilder = new (client.BaseAddress) {
            Path = path,
        };
        Uri endpoint = uriBuilder.Uri;

        try {
            // Create the request
            using HttpRequestMessage requestMessage = new(httpMethod, endpoint);
            if (headers?.Any() ?? false) {
                foreach (var kvp in headers) {
                    if (requestMessage.Headers.Contains(kvp.key)) {
                        this.logger.Warn($"Replacing request key: {kvp.key}", null);
                    }
                    requestMessage.Headers.Add(kvp.key, kvp.value);
                }
            }

            requestMessage.Content = new StringContent(body, Encoding.UTF8, "application/json");

            cancellationToken.ThrowIfCancellationRequested();

            // Send the request, get the response
            HttpResponseMessage message = await client.SendAsync(requestMessage, cancellationToken);
            statusCode = message.StatusCode.ToString();
            success = message.IsSuccessStatusCode;

            return message;
        }
        finally {
            sw.Stop();
            this.logger.Info($"Ending request to: {endpoint}, StatusCode: {statusCode}, Success: {success}", sw.ElapsedMilliseconds);
        }
    }
}