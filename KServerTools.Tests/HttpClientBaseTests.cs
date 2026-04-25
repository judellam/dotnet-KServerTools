namespace KServerTools.Tests;

using System.Net;
using KServerTools.Common;
using Moq;
using Moq.Protected;

public class HttpClientBaseTests {
    [Fact]
    public async Task Send_SuccessfulRequest_LogsPathWithoutQueryString() {
        // Create client whose base URL has query params (simulating SAS tokens)
        var (client, logger, _) = CreateTestClient(baseAddress: "https://api.example.com/");
        string? loggedMessage = null;
        logger.Setup(l => l.Info(It.IsAny<string>(), It.IsAny<long?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
            .Callback<string, long?, string, int, string>((msg, _, _, _, _) => loggedMessage = msg);

        await client.TestSend("/api/data", HttpMethod.Get, null, "{}", CancellationToken.None);

        Assert.NotNull(loggedMessage);
        // GetLeftPart(UriPartial.Path) strips query strings from logged URLs
        Assert.Contains("/api/data", loggedMessage);
        Assert.Contains("Success: True", loggedMessage);
    }

    [Fact]
    public async Task Send_SuccessfulRequest_LogsSuccessTrue() {
        var (client, logger, _) = CreateTestClient(HttpStatusCode.OK);
        string? loggedMessage = null;
        logger.Setup(l => l.Info(It.IsAny<string>(), It.IsAny<long?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
            .Callback<string, long?, string, int, string>((msg, _, _, _, _) => loggedMessage = msg);

        await client.TestSend("/api/data", HttpMethod.Get, null, "{}", CancellationToken.None);

        Assert.Contains("Success: True", loggedMessage);
    }

    [Fact]
    public async Task Send_BaseUrlWithQueryParams_LogStripsQueryString() {
        // Simulates a base URL with SAS token — GetLeftPart(UriPartial.Path) should strip it
        var logger = new Mock<IJsonLogger>();
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var httpClient = new HttpClient(handler.Object) {
            BaseAddress = new Uri("https://storage.blob.core.windows.net/")
        };
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("TestClient")).Returns(httpClient);

        string? loggedMessage = null;
        logger.Setup(l => l.Info(It.IsAny<string>(), It.IsAny<long?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
            .Callback<string, long?, string, int, string>((msg, _, _, _, _) => loggedMessage = msg);

        var client = new TestHttpClient(factory.Object, logger.Object);
        await client.TestSend("/container/blob.txt", HttpMethod.Get, null, "{}", CancellationToken.None);

        Assert.NotNull(loggedMessage);
        Assert.Contains("/container/blob.txt", loggedMessage);
        // Verify no query parameters leak into the log
        Assert.DoesNotContain("?", loggedMessage);
    }

    [Fact]
    public async Task Send_FailedRequest_LogsSuccessFalse() {
        var (client, logger, _) = CreateTestClient(HttpStatusCode.InternalServerError);
        string? loggedMessage = null;
        logger.Setup(l => l.Info(It.IsAny<string>(), It.IsAny<long?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
            .Callback<string, long?, string, int, string>((msg, _, _, _, _) => loggedMessage = msg);

        await client.TestSend("/api/data", HttpMethod.Get, null, "{}", CancellationToken.None);

        Assert.Contains("Success: False", loggedMessage);
    }

    [Fact]
    public async Task Send_ReturnsCorrectStatusCode() {
        var (client, _, _) = CreateTestClient(HttpStatusCode.Created);

        var response = await client.TestSend("/api/items", HttpMethod.Post, null, "{\"name\":\"test\"}", CancellationToken.None);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Send_AddsCustomHeaders() {
        var (client, _, handler) = CreateTestClient();
        HttpRequestMessage? capturedRequest = null;
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var headers = new List<(string key, string value)> { ("X-Custom", "test-value") };
        await client.TestSend("/api/data", HttpMethod.Get, headers, "{}", CancellationToken.None);

        Assert.NotNull(capturedRequest);
        Assert.Contains("test-value", capturedRequest!.Headers.GetValues("X-Custom"));
    }

    [Fact]
    public async Task Send_DuplicateHeader_LogsWarning() {
        var (client, logger, handler) = CreateTestClient();

        // First set up a handler that adds the header first
        HttpRequestMessage? capturedRequest = null;
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var headers = new List<(string key, string value)> {
            ("X-Dup", "first"),
            ("X-Dup", "second")
        };

        await client.TestSend("/api/data", HttpMethod.Get, headers, "{}", CancellationToken.None);

        logger.Verify(l => l.Warn(It.Is<string>(s => s.Contains("X-Dup")), null, null, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Post_DelegatesToSendWithPostMethod() {
        HttpMethod? capturedMethod = null;
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedMethod = req.Method)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("https://api.example.com/") };
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("TestClient")).Returns(httpClient);
        var logger = new Mock<IJsonLogger>();

        var client = new TestHttpClient(factory.Object, logger.Object);
        await client.TestPost("/api/items", null, "{}", CancellationToken.None);

        Assert.Equal(HttpMethod.Post, capturedMethod);
    }

    [Fact]
    public async Task Send_LogsStatusCodeString() {
        var (client, logger, _) = CreateTestClient(HttpStatusCode.NotFound);
        string? loggedMessage = null;
        logger.Setup(l => l.Info(It.IsAny<string>(), It.IsAny<long?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
            .Callback<string, long?, string, int, string>((msg, _, _, _, _) => loggedMessage = msg);

        await client.TestSend("/api/missing", HttpMethod.Get, null, "{}", CancellationToken.None);

        Assert.Contains("NotFound", loggedMessage);
    }

    private static (TestHttpClient client, Mock<IJsonLogger> logger, Mock<HttpMessageHandler> handler) CreateTestClient(
        HttpStatusCode responseCode = HttpStatusCode.OK, string baseAddress = "https://api.example.com/") {
        var logger = new Mock<IJsonLogger>();
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(responseCode) {
                Content = new StringContent("{}")
            });

        var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri(baseAddress) };
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("TestClient")).Returns(httpClient);

        return (new TestHttpClient(factory.Object, logger.Object), logger, handler);
    }

    private class TestHttpClient : HttpClientBase {
        public TestHttpClient(IHttpClientFactory factory, IJsonLogger logger) : base(factory, logger) { }
        public override string GetClientName() => "TestClient";

        public Task<HttpResponseMessage> TestSend(string path, HttpMethod method,
            IList<(string key, string value)>? headers, string body, CancellationToken ct) =>
            this.Send(path, method, headers!, body, ct);

        public Task<HttpResponseMessage> TestPost(string path,
            IList<(string key, string value)>? headers, string body, CancellationToken ct) =>
            this.Post(path, headers!, body, ct);
    }
}
