namespace KServerTools.Common;

using System.Diagnostics;
using System.Net.Http;
using System.Text;

using CustomHeaders = System.Collections.Generic.IList<(string key, string value)>;

/// <summary>
/// Abstract base class for HTTP clients that provides logging and common HTTP operations.
/// </summary>
/// <remarks>
/// <para>This class should be overridden to provide a base class for HTTP clients.</para>
/// <para>
/// Example usage:
/// <code>
/// public class MyHttpClient : HttpClientBase {
///    public MyHttpClient(IHttpClientFactory clientFactory, IJsonLogger logger) : base(clientFactory, logger) { }
///    public override string GetClientName() =&gt; "MyHttpClient";
/// }
/// </code>
/// </para>
/// </remarks>
public abstract class HttpClientBase {
    private readonly IHttpClientFactory clientFactory;
    private readonly IJsonLogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpClientBase"/> class.
    /// </summary>
    /// <param name="clientFactory">The HTTP client factory for creating clients.</param>
    /// <param name="logger">The JSON logger instance.</param>
    public HttpClientBase(IHttpClientFactory clientFactory, IJsonLogger logger) {
        this.clientFactory = clientFactory;
        this.logger = logger;
    }

    /// <summary>
    /// Gets the registered HTTP client name used by the factory.
    /// </summary>
    /// <returns>The HTTP client name.</returns>
    public abstract string GetClientName();

    /// <summary>
    /// Sends an HTTP POST request.
    /// </summary>
    /// <param name="path">The request path relative to the base address.</param>
    /// <param name="headers">Custom headers to include in the request.</param>
    /// <param name="body">The request body as a JSON string.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The HTTP response message.</returns>
    protected async Task<HttpResponseMessage> Post(string path, CustomHeaders headers, string body, CancellationToken cancellationToken) =>
        await this.Send(path, HttpMethod.Post, headers, body, cancellationToken);

    /// <summary>
    /// Sends an HTTP request with the specified method, headers, and body.
    /// </summary>
    /// <param name="path">The request path relative to the base address.</param>
    /// <param name="httpMethod">The HTTP method to use.</param>
    /// <param name="headers">Custom headers to include in the request.</param>
    /// <param name="body">The request body as a JSON string.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The HTTP response message.</returns>
    protected async Task<HttpResponseMessage> Send(string path, HttpMethod httpMethod, CustomHeaders headers, string body, CancellationToken cancellationToken) {
        Stopwatch sw = Stopwatch.StartNew();
        string statusCode = string.Empty;
        bool success = false;

        // Create the client
        HttpClient client = this.clientFactory.CreateClient(this.GetClientName());

        ArgumentNullException.ThrowIfNull(client, "Unable to create client");
        ArgumentNullException.ThrowIfNull(client.BaseAddress, "Client base address is null");

        UriBuilder uriBuilder = new(client.BaseAddress) {
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
        } finally {
            sw.Stop();
            // Log path only — query strings may contain tokens or secrets
            string safePath = endpoint.GetLeftPart(UriPartial.Path);
            this.logger.Info($"Ending request to: {safePath}, StatusCode: {statusCode}, Success: {success}", sw.ElapsedMilliseconds);
        }
    }
}
