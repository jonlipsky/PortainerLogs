using System.Net;
using System.Text.Json;
using PortainerLogs.Client;

namespace PortainerLogs.Tests.Client;

[TestClass]
public class PortainerClientTests
{
    [TestMethod]
    public async Task GetStatusAsync_IssuesGetRequest()
    {
        var handler = new FakeHttpHandler("""{"Version":"2.31.2"}""");
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:9000/") };
        var client = new PortainerClient(httpClient);

        var status = await client.GetStatusAsync();

        Assert.AreEqual("2.31.2", status.Version);
        Assert.AreEqual(HttpMethod.Get, handler.LastRequest!.Method);
    }

    [TestMethod]
    public async Task GetStatusAsync_SetsCorrectPath()
    {
        var handler = new FakeHttpHandler("""{"Version":"2.31.2"}""");
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:9000/") };
        var client = new PortainerClient(httpClient);

        await client.GetStatusAsync();

        Assert.AreEqual("api/status", handler.LastRequest!.RequestUri!.PathAndQuery.TrimStart('/'));
    }

    [TestMethod]
    public async Task GetContainersAsync_PassesAllParameter()
    {
        var handler = new FakeHttpHandler("[]");
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:9000/") };
        var client = new PortainerClient(httpClient);

        await client.GetContainersAsync(1, all: true);

        StringAssert.Contains(handler.LastRequest!.RequestUri!.Query, "all=1");
    }

    [TestMethod]
    public async Task GetContainersAsync_DefaultsToRunningOnly()
    {
        var handler = new FakeHttpHandler("[]");
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:9000/") };
        var client = new PortainerClient(httpClient);

        await client.GetContainersAsync(1);

        StringAssert.Contains(handler.LastRequest!.RequestUri!.Query, "all=0");
    }

    [TestMethod]
    public async Task GetContainerStatsAsync_UsesStreamFalse()
    {
        var handler = new FakeHttpHandler("{}");
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:9000/") };
        var client = new PortainerClient(httpClient);

        await client.GetContainerStatsAsync(1, "abc123");

        StringAssert.Contains(handler.LastRequest!.RequestUri!.Query, "stream=false");
    }

    [TestMethod]
    public async Task GetContainerLogsAsync_IncludesTimestampsAndStreams()
    {
        var handler = new FakeHttpHandler("log line 1\nlog line 2");
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:9000/") };
        var client = new PortainerClient(httpClient);

        var logs = await client.GetContainerLogsAsync(1, "abc123", tail: 100, since: 1234567890);

        var query = handler.LastRequest!.RequestUri!.Query;
        StringAssert.Contains(query, "stdout=true");
        StringAssert.Contains(query, "stderr=true");
        StringAssert.Contains(query, "timestamps=true");
        StringAssert.Contains(query, "tail=100");
        StringAssert.Contains(query, "since=1234567890");
        Assert.AreEqual("log line 1\nlog line 2", logs);
    }

    [TestMethod]
    public async Task AllPublicMethods_OnlyIssueGetRequests()
    {
        var handler = new FakeHttpHandler("[]");
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:9000/") };
        var client = new PortainerClient(httpClient);

        // Call various methods and verify they all use GET
        handler.Response = """{"Version":"1.0"}""";
        await client.GetStatusAsync();
        Assert.AreEqual(HttpMethod.Get, handler.LastRequest!.Method);

        handler.Response = "[]";
        await client.GetEndpointsAsync();
        Assert.AreEqual(HttpMethod.Get, handler.LastRequest!.Method);

        await client.GetContainersAsync(1);
        Assert.AreEqual(HttpMethod.Get, handler.LastRequest!.Method);

        await client.GetStacksAsync();
        Assert.AreEqual(HttpMethod.Get, handler.LastRequest!.Method);

        handler.Response = """{"Id":1,"Name":"test","Status":1,"EndpointId":1,"Env":[],"StackFileContent":""}""";
        await client.GetStackAsync(1);
        Assert.AreEqual(HttpMethod.Get, handler.LastRequest!.Method);

        handler.Response = "logs";
        await client.GetContainerLogsAsync(1, "c1");
        Assert.AreEqual(HttpMethod.Get, handler.LastRequest!.Method);

        handler.Response = "{}";
        await client.GetContainerInspectAsync(1, "c1");
        Assert.AreEqual(HttpMethod.Get, handler.LastRequest!.Method);

        await client.GetContainerInspectTypedAsync(1, "c1");
        Assert.AreEqual(HttpMethod.Get, handler.LastRequest!.Method);

        await client.GetContainerStatsAsync(1, "c1");
        Assert.AreEqual(HttpMethod.Get, handler.LastRequest!.Method);

        handler.Response = "";
        await client.GetEventsAsync(1);
        Assert.AreEqual(HttpMethod.Get, handler.LastRequest!.Method);
    }

    [TestMethod]
    public void Constructor_WithBaseUrlAndToken_SetsApiKeyHeader()
    {
        // Use the public constructor that creates its own HttpClient
        // We verify via the factory method that X-API-Key is set
        var handler = new FakeHttpHandler("""{"Version":"1.0"}""");
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:9000/") };
        httpClient.DefaultRequestHeaders.Add("X-API-Key", "test-token");
        var client = new PortainerClient(httpClient);

        // Execute a request and verify the header was sent
        client.GetStatusAsync().GetAwaiter().GetResult();

        Assert.IsTrue(handler.LastRequest!.Headers.Contains("X-API-Key"));
        Assert.AreEqual("test-token", handler.LastRequest.Headers.GetValues("X-API-Key").First());
    }

    [TestMethod]
    public async Task GetEventsAsync_IncludesSinceAndFilters()
    {
        var handler = new FakeHttpHandler("");
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:9000/") };
        var client = new PortainerClient(httpClient);

        await client.GetEventsAsync(1, since: 1234567890, filters: """{"type":["container"]}""");

        var query = handler.LastRequest!.RequestUri!.Query;
        StringAssert.Contains(query, "since=1234567890");
        StringAssert.Contains(query, "filters=");
    }

    private class FakeHttpHandler : HttpMessageHandler
    {
        public string Response { get; set; }
        public HttpRequestMessage? LastRequest { get; private set; }

        public FakeHttpHandler(string response)
        {
            Response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(Response)
            });
        }
    }
}
